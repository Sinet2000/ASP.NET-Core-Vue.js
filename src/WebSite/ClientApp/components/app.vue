<template>
    <div>
        <vue-progress-bar></vue-progress-bar>
        
        <nav-menu></nav-menu>

        <div class="container body-content">
            <router-view></router-view>
        </div>
    </div>
</template>

<script>
    import Vue from 'vue';
    import NavMenu from './nav-menu';

    Vue.component('nav-menu', NavMenu);

    export default {
        data() {
            return {
            }
        },

        mounted() {
            this.$Progress.finish();
        },

        created() {
            this.$Progress.start();
            this.initRouter();
            this.initAjax();

            this.$auth.init(this.forceRedirectTo);
        },

        methods: {
            initRouter() {

                this.$router.beforeEach((to, from, next) => {
                    if (to.meta.progress)
                        this.$Progress.parseMeta(to.meta.progress);

                    this.$Progress.start();

                    next();
                })

                this.$router.afterEach((to, from) => {
                    this.$Progress.finish();
                });
            },

            initAjax() {

                this.axios.interceptors.request.use(config => {

                    if (this.canStartProgress() && this.canShowGlobalLoader(config))
                        this.$Progress.start();

                    return config;

                }, error => {

                    if (error.request) {
                        if (this.canShowGlobalLoader(error.request.config))
                            this.$Progress.fail();
                    }
                    else {
                        Vue.toasted.error('Oops! ' + error.message);
                        this.$Progress.fail();
                    }

                    return Promise.reject(error);
                });

                this.axios.interceptors.response.use(response => {

                    if (this.canShowGlobalLoader(response.config))
                        this.$Progress.finish();

                    return response;

                }, error => {
                    if (error.response) {
                        if (this.canShowGlobalLoader(error.response.config))
                            this.$Progress.fail();

                        if (!this.errorHasCustomErrors(error)) {
                            if (error.response.status === 404)
                                this.$router.replace({ name: 'not-found'});
                            else
                                Vue.toasted.error('Oops! ' + error.message);
                        }
                    }
                    else {
                        Vue.toasted.error('Oops! ' + error.message);
                        this.$Progress.fail();
                    }

                    return Promise.reject(error);
                });
            },

            canStartProgress()
            {
                //check if already started
                return !this.$Progress.$vm.RADON_LOADING_BAR.options.show;
            },

            canShowGlobalLoader(config) {

                if (!config)
                    return true;

                return config.globalLoader !== false;
            },

            successToast(message) {
                Vue.toasted.success(message, { icon: 'check' });
            },

            errorToast(message) {
                Vue.toasted.error(message, { icon: 'exclamation' });
            },

            errorHasCustomErrors(error) {
                return error.response.data.error_description || error.response.data.error;
            },

            forceRedirectTo(toRoute) {
                //skipping anonymous users
                if (!this.$auth.isAuthenticated)
                    return false;

                //navigation to routes that doesn't require authentication is always allowed
                if (toRoute.meta && !toRoute.meta.auth)
                    return false;

                //example
                //if (this.$auth.isInRole(UserRole.Parent) && !this.isParentHasChildren)
                //    return { name: 'parent-my-students' };

                return false;
            }
        }
    }
</script>