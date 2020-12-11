const defaults = {
    tokenUri: '/auth/login',
    logoutUri: '/auth/logout',
    logoutRedirectTo: 'default',
    unauthorizedRedirectTo: 'login',
    forbiddenRedirectTo: 'forbidden',
    userInfoUri: '/auth/userinfo',
    scope: 'openid offline_access email profile roles',
    rememberMeDuration: 14, //in days
    tokenStorageType: 'localStorage',
    tokenStorageNamespace: 'oidc-client',
    loginProviders: {
        configuredProvidersUri: '/auth/loginproviders',
        redirectUri: '/auth/login',
        addLoginUri: '/profile/addlogin'
    }
};

export default defaults;