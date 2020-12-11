import Vue from 'vue';
import ApiError from 'utils/api-error';

const service = {
    async getUserListData() {

        try {
            return (await Vue.axios.get('/admin/userListData')).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async createUserData() {

        try {
            return (await Vue.axios.get('/admin/createUser')).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async createUser(data) {

        try {
            await Vue.axios.post('/admin/createUser', data);
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async editUserData(id) {

        try {
            return (await Vue.axios.get('/admin/editUser', { params: { id: id } })).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async editUser(data) {

        try {
            await Vue.axios.post('/admin/editUser', data);
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async disableUser(id) {

        try {
            await Vue.axios.get('/admin/disableOrEnableUser', { params: { id: id } });
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async deleteUser(id) {

        try {
            await Vue.axios.get('/admin/deleteUser', { params: { id: id } });
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async createCompany(data) {

        try {
            await Vue.axios.post('/admin/createCompany', data);
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async editCompanyData(id) {

        try {
            return (await Vue.axios.get('/admin/editCompany', { params: { id: id } })).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async editCompany(data) {

        try {
            await Vue.axios.post('/admin/editCompany', data);
        }
        catch (error) {
            throw new ApiError(error);
        }
    },

    async deleteCompany(id) {

        try {
            await Vue.axios.get('/admin/deleteCompany', { params: { id: id } });
        }
        catch (error) {
            throw new ApiError(error);
        }
    }
}

export default service;