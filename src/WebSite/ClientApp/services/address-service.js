import Vue from 'vue';
import ApiError from 'utils/api-error';

const addressService = {
    async getUserAddresses(userId) {
        try {
            return (await Vue.axios.get('/address/getUserAddresses', { params: { userId } })).data;
        }
        catch (error) {
            throw new ApiError(error);
        }
    }
}