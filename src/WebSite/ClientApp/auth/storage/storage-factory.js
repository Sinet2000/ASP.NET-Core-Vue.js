import CookieStorage from './cookie-storage.js';
import LocalStorage from './local-storage.js'

export default function StorageFactory(options) {
    switch (options.tokenStorageType) {

        case 'cookieStorage':
            return new CookieStorage(options.tokenStorageNamespace, {
                domain: window.location.hostname,
                expires: null,
                path: '/',
                secure: false
            });

        case 'localStorage':
        default:
            window.localStorage.setItem('testKey', 'test')
            window.localStorage.removeItem('testKey')

            return new LocalStorage(options.tokenStorageNamespace)
    }
}