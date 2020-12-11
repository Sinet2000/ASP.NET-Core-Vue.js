import Vue from 'vue';
import Vuex from 'vuex';
//import createPersistedState from 'vuex-persistedstate';

import * as types from './mutation-types';
import user from './modules/user';

Vue.use(Vuex);

// STATE
const state = {
    counter: 0
};

// MUTATIONS
const mutations = {
    [types.MAIN_SET_COUNTER](state, obj) {
        state.counter = obj.counter;
    }
};

// ACTIONS
const actions = ({
    setCounter({ commit }, obj) {
        commit(types.MAIN_SET_COUNTER, obj);
    }
});

const store = new Vuex.Store({
    state,
    mutations,
    actions,
    modules: {
        user
    },
    //plugins: [createPersistedState()]
});

export default store;