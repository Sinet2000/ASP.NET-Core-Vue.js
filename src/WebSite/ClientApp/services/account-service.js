import Vue from 'vue';
import ApiError from 'utils/api-error';

const accountService = {
    async register(data) {

        try {
            return (await Vue.axios.post('/auth/register', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async getBaseInfo() {
        try {
            return (await Vue.axios.get('/profile/GetBaseInfo')).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async changeBaseInfo(data) {
        try {
            return (await Vue.axios.post('/profile/ChangeBaseInfo', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async changeUserPassword(data) {
        try {
            return (await Vue.axios.post('/profile/ChangePassword', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async setUserPassword(data) {
        try {
            return (await Vue.axios.post('/profile/SetPassword', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async resetPassword(data) {
        try {
            return (await Vue.axios.post('/Auth/ResetPassword', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async forgotPassword(data) {
        try {
            return (await Vue.axios.post('/Auth/ForgotPassword', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async confirmEmail(data) {
        try {
            return (await Vue.axios.post('/Auth/ConfirmEmail', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async deleteLoginProvider(data) {

        try {
            return (await Vue.axios.post('/profile/deleteLoginProvider', data)).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },
    async getAvailableLoginProviders() {
        try {
            return (await Vue.axios.get('/profile/GetAvailableLoginProviders')).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },
}

export default accountService;