<template>
    <div class="login-wrap">
        <div class="login-logo">
            <router-link to="/"><img src="http://placehold.it/100x100/efefef/?text=LOGO" alt="" /></router-link>
        </div>
        <div class="card card-login">
            <div class="card-header">
                <h1 class="card-title">Sign In</h1>
            </div>
            <div class="card-body">
                <b-form @submit.prevent="submit">
                    <validation-summary :errors="errors"></validation-summary>

                    <v-form-group :validator="$v.email" label="Email" hide-label>
                        <b-form-input v-model="email" placeholder="Email" />
                    </v-form-group>
                    <v-form-group :validator="$v.password" label="Password" hide-label>
                        <b-form-input v-model="password" placeholder="Password" type="password" />
                    </v-form-group>

                    <b-form-group>
                        <b-form-checkbox v-model="rememberMe">Keep me signed in</b-form-checkbox>

                        <router-link class="pull-right" to="/auth/forgot-password">Forgot password?</router-link>
                    </b-form-group>

                    <div class="form-group">
                        <btn-loading :loading="loading" class="btn-block" type="submit">Sign In</btn-loading>
                    </div>
                </b-form>

                <template v-if="loginProviders && loginProviders.length > 0">
                    <span class="or"></span>

                    <button v-for="provider in loginProviders" :key="provider.key" typeof="button" :class="'ld-ext-right btn btn-block btn-sm btn-social btn-'+provider.key" type="button" @click="providerLogin(provider.key)">
                        <span :class="'fa fa-'+provider.key"></span>{{ provider.name }}
                    </button>
                </template>
            </div>
        </div>
        <div class="bottom-text">
            <router-link to="/register">Create an Account</router-link>
        </div>
    </div>
</template>

<script>
    import { mapState } from 'vuex';
    import { required, email } from 'vuelidate/lib/validators';
    import formMixin from 'mixins/form-mixin';

    export default {
        mixins: [formMixin],
        data() {
            return {
                email: '',
                password: '',
                rememberMe: false
            }
        },

        validations: {
            email: {
                required,
                email
            },
            password: {
                required
            }
        },

        computed: {
            loginProviders() {
                return this.$auth.loginProviders;
            }
        },

        methods: {
            getAfterLoginRedirect() {
                return this.$route.query.redirect || '/';
            },

            async onSubmit() {
                let redirect = this.$route.query.redirect || '/';

                await this.$auth.login(this.email, this.password, this.rememberMe, this.getAfterLoginRedirect());
            },

            async providerLogin(provider) {
                try {
                    await this.$auth.providerLogin(provider, this.getAfterLoginRedirect());
                }
                catch (error) {
                    this.errors = error.customErrors;
                }
            }
        }
    }
</script>