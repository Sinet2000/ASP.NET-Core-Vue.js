import Vue from 'vue';

import hello from 'hellojs';
import jwtDecode from 'jwt-decode';
import Cookies from 'js-cookie';
import qs from 'qs';

import ApiError from 'utils/api-error';

import StorageFactory from './storage/storage-factory'
import { isEmptyObject, objectExtend, compare } from './utils';
import defaults from './defaults';

/**
* OpenID Connect/OAuth 2.0 auth manager.
*
* Handles user authentication and token-based authorization by using OpenID Connect/OAuth 2.0 standards.
*/
export default class AuthManager {

    constructor($http, options) {

        this.$http = $http;
        this.options = objectExtend(defaults, options);
        this.storage = StorageFactory(options);
        this.router = options.router;

        //define internal reactive state
        this.internalState = new Vue({
            data: function () {
                return {
                    tokenData: null,
                    userInfo: null,
                    loginProviders: null
                };
            }
        });

        //define getters and setters
        Object.defineProperties(this, {
            tokenData: {
                get() {
                    if (!this.storage.getItem('tokenData')) return null;

                    return JSON.parse(this.storage.getItem('tokenData'));
                },
                set(object) {
                    this.internalState.tokenData = object;
                    this.storage.setItem('tokenData', JSON.stringify(object));
                }
            },

            userInfo: {
                get() {
                    if (!this.storage.getItem('userInfo')) return null;

                    return JSON.parse(this.storage.getItem('userInfo'));
                },
                set(object) {
                    this.internalState.userInfo = object;
                    this.storage.setItem('userInfo', JSON.stringify(object));
                }
            }
        });

        //add Axios interceptors
        this.$http.interceptors.request.use(config => {

            if (this._tokenExists())
                this._setAuthHeader(config);

            return config;
        });

        this.$http.interceptors.response.use(response => {
            return response;
        },
            error => {
                if (this._isInvalidToken(error.response))
                    return this._refreshToken(error.config);

                if (error.response) {
                    if (error.response.status === 401)
                        this.router.push({ name: this.options.unauthorizedRedirectTo, query: { redirect: this.router.currentRoute.path } });

                    if (error.response.status === 403)
                        this.router.replace({ name: this.options.forbiddenRedirectTo });
                }

                return Promise.reject(error);
            });
    }

    /**
     * Reactive getter - checks if user is authenticated.
     *
    * @return {boolean}
    */
    get isAuthenticated() {
        return this.internalState.tokenData != null;
    }

    /**
     * Reactive getter - User info received from userinfo endpoint.
     *
    * @return {Object} user info
    */
    get user() {
        if (!this.isAuthenticated)
            return null;

        return this.internalState.userInfo;
    }

    /**
     * Reactive getter - checks if admin is logged in as another user.
     *
    * @return {boolean}
    */
    get hasLocalAccount() {
        if (!this.isAuthenticated)
            return false;

        if (!this.user)
            return false;

        return this.user.has_local_account;
    }

    /**
     * Reactive getter - checks if admin is logged in as another user.
     *
    * @return {boolean}
    */
    get isLoggedInAsAnotherUser() {
        if (!this.isAuthenticated)
            return null;

        return this._decodeAccessToken().admin_username != null;
    }

    /**
     * Reactive getter - Login providers.
     *
    * @return {Array} login providers
    */
    get loginProviders() {
        return this.internalState.loginProviders;
    }

    /**
     * Initialization
     *
     * Check if user session exists and initializes the authentication state.
     *
     * @return {void}
    */
    init(forceRedirectTo) {
        if (this._ensureDataIntegrity()) {
            //restore state from storage if exists
            this.internalState.tokenData = this.tokenData;
            this.internalState.userInfo = this.userInfo;
        }
        else
            this._endSession();

        this._initLoginProviders();
        this._initNavigationGuards(forceRedirectTo);
    }

    /**
     * Login
     *
     * @param {string} username Username for logging in.
     * @param {string} password Password for logging in.
     * @param {boolean} rememberMe Remember user or not.
     * @param {string|null} redirect The route to redirect to.
     * @return {void}
     */
    async login(username, password, rememberMe, redirect) {
        const data = {
            grant_type: 'password',
            username: username,
            password: password,
            scope: this.options.scope
        }

        await this._login(data, rememberMe, redirect);
    }

    /**
    * Admin login as user
    *
    * @param {string} username Username to login as.
    * @return {void}
    */
    async loginAsUser(username) {
        const data = {
            grant_type: 'login_as_user',
            username: username,
            return_to: this.router.currentRoute.path,
            scope: this.options.scope
        }

        await this._login(data, false, { name: 'default' });
    }

    /**
    * Login back as admin
    *
    * @return {void}
    */
    async loginBackAsAdmin(username) {
        const data = {
            grant_type: 'login_back_as_admin',
            scope: this.options.scope
        }

        await this._login(data, false, this._decodeAccessToken().return_to);
    }

    /**
   * Provider Login
   *
   * @param {string} provider Login provider.
   * @param {string|null} redirect The route to redirect to.
   * @return {Promise}
   */
    async providerLogin(provider, redirect, addToExisting = false) {
        try {
            let providerResponse = await hello.login(provider, { scope: 'email' });
            let userProfile = await hello(provider).api('me');

            let grantType = this.loginProviders.filter((provider) => { return provider.key === providerResponse.network; })[0].grantType;

            const data = {
                grant_type: grantType,
                assertion: providerResponse.authResponse.access_token,
                name: userProfile.name,
                scope: this.options.scope
            }

            let loginUrl = addToExisting ? this.options.loginProviders.addLoginUri : this.options.tokenUri;

            let response = await this.$http.post(loginUrl, qs.stringify(data));

            if (!addToExisting) {
                this._storeToken(response);

                Cookies.set('userSessionMarker', 'userSessionMarker')

                await this.updateUserInfo();
            }

            if (redirect)
                this.router.push(redirect);

            return response;
        }
        catch (error) {
            throw new ApiError(error);
        }
    }

    /**
     * Updates user info
     *
     * @return {void}
     */
    async updateUserInfo() {
        try {
            let response = await this.$http.get(this.options.userInfoUri);
            this._storeUserInfo(response);
        }
        catch (error) {
            throw new ApiError(error);
        }
    }

    /**
     * Logout
     *
     * Clear all data in storage (which resets logged-in status) and redirect.
     *
     * @return {void}
     */
    async logout() {

        if (this.tokenData)
            await this.$http.get(this.options.logoutUri);

        if (this.loginProviders) {
            for (let provider of this.loginProviders) {
                if (this.storage.getItem(provider.key)) {
                    //async call - no need to wait for completion
                    hello.logout(provider.key);
                }
            }
        }

        this._endSession();
    }

    /**
     * Check if user belongs to specific role.
     *
    * @param {string} role A role.
    * @return {boolean}
    */
    isInRole(role) {
        if (!this.isAuthenticated)
            return false;

        return compare(role, this._decodeAccessToken().role);
    }

    isRouteAccessible(route) {
        if (route.meta && route.meta.auth) {
            if (!this.isAuthenticated)
                return false;

            if (!this._tokenExists())
                return false;

            if (route.meta.auth.roles)
                return this.isInRole(route.meta.auth.roles);
        }

        return true;
    }

    /**
     * Internal login
     *
     * @param {object} data Data to send to the token endpoint.
     * @param {boolean} rememberMe Remember user or not.
     * @param {string|null} redirect The route to redirect to.
     * @return {void}
     */
    async _login(data, rememberMe, redirect) {
        try {
            let response = await this.$http.post(this.options.tokenUri, qs.stringify(data));

            this._storeToken(response);

            if (rememberMe)
                Cookies.set('userSessionMarker', 'userSessionMarker', { expires: this.options.rememberMeDuration });
            else
                Cookies.set('userSessionMarker', 'userSessionMarker');

            await this.updateUserInfo();

            if (redirect)
                this.router.push(redirect)
        }
        catch (error) {
            throw new ApiError(error);
        }
    }

    /**
    * Initialize Login Providers
    *
    * @return {void}
    */
    async _initLoginProviders() {
        try {
            let providers = null;
            let response = await this.$http.get(this.options.loginProviders.configuredProvidersUri);

            if (response.data && Array.isArray(response.data)) {
                providers = response.data;

                let helloProviders = {};

                for (let provider of providers)
                    helloProviders[provider.key] = provider.id;

                hello.init(helloProviders, { redirect_uri: this.options.loginProviders.redirectUri });
            }

            this.internalState.loginProviders = providers;
        }
        catch (error) {
            throw new ApiError(error);
        }
    }

    /**
    * Decodes the stored access token.
    *
    * @private
    * @return {object} Decoded access token
    */
    _decodeAccessToken() {
        return jwtDecode(this.internalState.tokenData.accessToken);
    }

    /**
    * Clears user's authentication data and redirect's to configured logoutRedirectTo route.
    *
    * @private
    * @return {void}
    */
    _endSession() {
        Cookies.remove('userSessionMarker');

        this.tokenData = null;
        this.userInfo = null;

        this.router.push({ name: this.options.logoutRedirectTo });
    }

    /**
    * Set the Authorization header on Axios Request.
    *
    * @private
    * @param {Object} config The Axios request's config to set the header on.
    * @return {void}
    */
    _setAuthHeader(config) {
        config.headers['Authorization'] = 'Bearer ' + this.tokenData.accessToken;
    }

    /**
     * Retry the original request.
     *
     * Let's retry the user's original target request that had recieved an invalid token response (which we fixed with a token refresh).
     *
     * @private
     * @param {Object} config The Axios request's config to use to repeat an http call.
     * @return {Promise}
     */
    async _retryAfterTokenRefresh(config) {
        //re-write the original request's access token with a refreshed one
        this._setAuthHeader(config);

        //Fix - clear the base url as it was already added to the config's url during the original request, otherwise it will be added to the config's url once again
        config.baseURL = '';

        return await this.$http.request(config);
    }

    /**
     * Refresh the access token
     *
     * Make an ajax call to the OpenID Connect server to refresh the access token (using our refresh token).
     *
     * @private
     * @param {Object} config The Axios original request's config that we'll retry.
     * @return {Promise}
     */
    async _refreshToken(config) {
        const data = {
            grant_type: 'refresh_token',
            'refresh_token': this.tokenData.refreshToken
        }

        try {
            let response = await this.$http.post(this.options.tokenUri, qs.stringify(data));

            this._storeToken(response);

            return this._retryAfterTokenRefresh(config);
        }
        catch (error) {
            this._endSession();
            throw error;
        }
    }

    /**
     * Store tokens
     *
     * Update the storage with the access/refresh tokens received from the token endpoint from the OpenID Connect server.
     *
     * @private
     * @param {Object} response Axios's response instance from the server that contains our tokens.
     * @return {void}
     */
    _storeToken(response) {
        const newTokenData = this.tokenData || {}

        newTokenData.accessToken = response.data.access_token

        if (response.data.refresh_token)
            newTokenData.refreshToken = response.data.refresh_token

        this.tokenData = newTokenData;
    }

    /**
     * Store user info
     *
     * Update the storage with the user info received from the userinfo endpoint from the OpenID Connect server.
     *
     * @private
     * @param {Object} response Axios's response instance from the server that contains userinfo.
     * @return {void}
     */
    _storeUserInfo(response) {
        this.userInfo = response.data && !isEmptyObject(response.data) ? response.data : null;
    }

    /**
     * Check if the Axios's response is an invalid token response.
     *
     * @private
     * @param {Object} response The Axios's response instance received from an http call.
     * @return {boolean}
     */
    _isInvalidToken(response) {
        if (!response)
            return false;

        if (!response.headers)
            return false;

        const status = response.status
        const wwwAuthenticateHeader = response.headers['www-authenticate']

        return (status === 401 && wwwAuthenticateHeader && (wwwAuthenticateHeader.includes('invalid_token') || wwwAuthenticateHeader.includes('expired_token')));
    }

    /**
     * Ensures data integrity.
     *
     * @private
     * @return {boolean}
     */
    _ensureDataIntegrity() {
        //if there is no token data then session marker should not exist
        if (!this.tokenData && Cookies.get('userSessionMarker'))
            return false;

        //if there is token data then session marker should exist as well
        if (this.tokenData && !Cookies.get('userSessionMarker'))
            return false;

        return true;
    }

    /**
     * Checks if token data exists.
     *
     * @private
     * @return {boolean}
     */
    _tokenExists() {
        return this._ensureDataIntegrity() && this.tokenData;
    }

    /**
    * Client-side authorization checks for navigation.
    *
    * @private
    * @return {boolean}
    */
    _initNavigationGuards(forceRedirectTo) {

        //user tries to open an direct URL or refreshes a browser
        this.router.onReady(() => {

            if (!this.isRouteAccessible(this.router.currentRoute)) {
                if (!this.isAuthenticated)
                    this.router.push({ name: this.options.unauthorizedRedirectTo, query: { redirect: this.router.currentRoute.path } });
                else
                    this.router.replace({ name: this.options.forbiddenRedirectTo });

                return;
            }

            if (forceRedirectTo) {
                let redirectTo = forceRedirectTo(this.router.currentRoute);

                if (redirectTo) {
                    let resolved = this.router.resolve(redirectTo);

                    if (resolved.route)
                        this.router.push(resolved.route.path);
                }
            }
        });

        //user performs a regular navigation
        this.router.beforeEach((to, from, next) => {

            if (!this.isRouteAccessible(to)) {
                next({ name: this.options.forbiddenRedirectTo });
                return;
            }

            if (forceRedirectTo) {
                let redirectTo = forceRedirectTo(to);

                if (redirectTo) {
                    let resolved = this.router.resolve(redirectTo);

                    if (resolved.route && resolved.route.path !== to.path) {
                        next(resolved.route.path);
                        return;
                    }
                }
            }

            next();
        })
    }
}