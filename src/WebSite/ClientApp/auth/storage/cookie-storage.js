import Cookies from 'js-cookie';

class CookieStorage {
    constructor(namespace, options) {
        this.namespace = namespace || null;
        this.options = options;
    }

    setItem(key, value) {
        Cookies.set(this._getStorageKey(key), value, this.options);
    }

    getItem(key) {
        return Cookies.get(this._getStorageKey(key));
    }

    removeItem(key) {
        Cookies.remove(this._getStorageKey(key));
    }

    _getStorageKey(key) {
        if (this.namespace) {
            return [this.namespace, key].join('.')
        }
        return key;
    }
}

export default CookieStorage