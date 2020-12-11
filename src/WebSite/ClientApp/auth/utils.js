function isObject(val) {
    if (val !== null && typeof val === 'object' && val.constructor !== Array)
        return true;

    return false;
}

function isEmptyObject(val) {
    return isObject(val) && Object.keys(val).length === 0;
}

function toArray(val) {
    return (typeof val) === 'string' || (typeof val) === 'number' ? [val] : val;
}

function objectExtend(a, b) {

    // Don't touch 'null' or 'undefined' objects.
    if (a == null || b == null) {
        return a;
    }

    Object.keys(b).forEach(function (key) {
        if (Object.prototype.toString.call(b[key]) === '[object Object]') {
            if (Object.prototype.toString.call(a[key]) !== '[object Object]') {
                a[key] = b[key];
            } else {
                a[key] = objectExtend(a[key], b[key]);
            }
        } else {
            a[key] = b[key];
        }
    });

    return a;
}

function compare(one, two) {
    var i, ii, key;

    if (Object.prototype.toString.call(one) === '[object Object]' && Object.prototype.toString.call(two) === '[object Object]') {
        for (key in one) {
            if (compare(one[key], two[key])) {
                return true;
            }
        }

        return false;
    }

    one = toArray(one);
    two = toArray(two);

    if (!one || !two || one.constructor !== Array || two.constructor !== Array) {
        return false;
    }

    for (i = 0, ii = one.length; i < ii; i++) {
        if (two.indexOf(one[i]) >= 0) {
            return true;
        }
    }

    return false;
}

export { isObject, isEmptyObject, objectExtend, compare }