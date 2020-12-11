import Vue from 'vue';

//bootstrap plug-in
import BootstrapVue from 'bootstrap-vue/esm/index.js';
import 'bootstrap-vue/dist/bootstrap-vue.css';

//common plug-ins

//axios
import axios from 'axios';
import VueAxios from 'vue-axios';

//Auth
import AuthManager from './auth/index';

//vuelidate
import Vuelidate from 'vuelidate';
import VuelidateErrorExtractor from 'vuelidate-error-extractor';

//other
import { sync } from 'vuex-router-sync';
import VueProgressBar from 'vue-progressbar';
import Toasted from 'vue-toasted';

//DataTables
import dt from 'datatables.net-bs4';
dt(window, $);
import 'datatables.net-bs4/css/dataTables.bootstrap4.css';
import VueDataTables from 'vue-datatables.net';

//router and store
import router from './router';
import store from './store';

//components
import app from 'components/app';
import loadingButton from 'components/common/loading/loading-btn';
import validationSummary from 'components/common/validation-summary';
import vuelidateErrorExtractorTemplate from 'components/common/vuelidate/bootstrap4-template';
import VueDataTablesFilterInput from 'components/common/vue-datatables/filters/filter-input';
import VueDataTablesFilterDateRange from 'components/common/vue-datatables/filters/filter-daterange';
import VueDataTablesFilterCheckbox from 'components/common/vue-datatables/filters/filter-checkbox';
import VueDataTablesFilterSelect from 'components/common/vue-datatables/filters/filter-select';
import VueDataTableDefaults from 'components/common/vue-datatables/defaults';

Vue.use(router);
sync(store, router);

Vue.use(BootstrapVue);

Vue.use(VueAxios, axios);
Vue.axios.defaults.baseURL = '/api/';

Vue.use(AuthManager, { router: router });

Vue.use(Vuelidate);

Vue.use(VuelidateErrorExtractor, {
    name: 'v-form-group',
    template: vuelidateErrorExtractorTemplate,
    messages: {
        required: 'The {label} field is required.',
        email: 'The {label} field should be a valid email.',
        confirmPassword: 'The passwords should match.',
        minLength: 'The {label} must be at least {min} characters long.'
    }
});

Vue.use(VueProgressBar, {
    color: 'rgb(143, 255, 199)'
});

Vue.use(VueDataTables, VueDataTableDefaults);

Vue.use(Toasted, {
    theme: "bubble",
    position: "top-center",
    iconPack: 'fontawesome',
    duration: 1500
});

Vue.component('btn-loading', loadingButton);
Vue.component('validation-summary', validationSummary);

Vue.component('dt-filter-input', VueDataTablesFilterInput);
Vue.component('dt-filter-daterange', VueDataTablesFilterDateRange);
Vue.component('dt-filter-checkbox', VueDataTablesFilterCheckbox);
Vue.component('dt-filter-select', VueDataTablesFilterSelect);

new Vue({
    el: '#app',
    store,
    router,
    ...app
});