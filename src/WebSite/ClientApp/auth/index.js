import AuthManager from './auth-manager';

/**
 * AuthManager plugin
 * @param {Object} Vue
 * @param {Object} options
 */
function install(Vue, options) {

    if (!Vue.axios) {
        throw new Error('Axios instance is not found');
    }

    let client = new AuthManager(Vue.axios, options)

    Vue.prototype.$auth = Vue.auth = client;
}

export default install;