//import * as types from '../mutation-types';
//import jwtDecode from 'jwt-decode';

//import accountService from 'services/account-service';

// state
const state = {
    //user: null,
    //accessToken: null,
    //refreshToken: null
};

// getters
const getters = {

};

// mutations
const mutations = {

    //[types.SET_USER](state, data) {
    //    state.user = data;
    //},

    //[types.SET_ACCESS_TOKEN](state, accessToken) {
    //    state.accessToken = accessToken;
    //},

    //[types.SET_REFRESH_TOKEN](state, refreshToken) {
    //    state.refreshToken = refreshToken;
    //},

    //[types.LOGOUT_USER](state) {
    //    state.user = null;
    //    state.accessToken = null;
    //    state.refreshToken = null;
    //}
};

// actions
const actions = {

    //async setUserAndTokens({ dispatch, commit, getters, rootGetters }, data) {

    //    let token = jwtDecode(data.accessToken);

    //    commit(types.SET_USER, { id: token.sub, email: token.name, role: token.role });
    //    commit(types.SET_ACCESS_TOKEN, data.accessToken);
    //    commit(types.SET_REFRESH_TOKEN, data.refreshToken);
    //},

    //async login({ dispatch, commit, getters, rootGetters }, data) {

    //    let res = await accountService.login(data.auth, data.email, data.password);

    //    await dispatch('setUserAndTokens', { accessToken: res.access_token, refreshToken: res.refresh_token });
    //},

    //async logout({ dispatch, commit, getters, rootGetters }, data) {

    //    await accountService.logout(data.auth);

    //    commit(types.LOGOUT_USER);
    //}
};

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
};