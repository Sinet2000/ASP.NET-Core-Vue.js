<template>
    <div class="login-wrap">
        <div class="login-logo">
            <router-link to="/"><img src="http://placehold.it/100x100/efefef/?text=LOGO" alt="" /></router-link>
        </div>
        <div class="card card-login">
            <div class="card-header">
                <h1 class="card-title">Reset Password</h1>
            </div>                        
            <div class="card-body">
                <template v-if="!showSuccessConfirmation">
                    <b-form @submit.prevent="submit">
                        <validation-summary :errors="errors"></validation-summary>

                        <v-form-group :validator="$v.email" label="Email">
                            <b-form-input v-model="email" />
                        </v-form-group>
                        <v-form-group :validator="$v.password" label="Password">
                            <b-form-input type="password" v-model="password" />
                        </v-form-group>
                        <v-form-group :validator="$v.confirmPassword" label="Confirm Password">
                            <b-form-input type="password" v-model="confirmPassword" />
                        </v-form-group>

                        <div class="form-group">
                            <btn-loading :loading="loading" class="btn-block" type="submit">Reset</btn-loading>
                        </div>
                    </b-form>
                </template>
                <template v-else>
                    <p class="text-center">Your password has been reset successfully. Please Sign In.</p>
                    <b-btn class="btn-block" variant="primary" to="/login">Sign In</b-btn>
                </template>
            </div>
        </div>
        <div class="bottom-text">
            <router-link to="/login">Back to Sign In</router-link>
        </div>
    </div>
</template>

<script>
    import { required, email, minLength, sameAs } from 'vuelidate/lib/validators';
    import Vue from 'vue';
    import ApiError from 'utils/api-error';
    import accountService from 'services/account-service';
    import formMixin from 'mixins/form-mixin';

    export default {
        mixins: [formMixin],

        created: function () {
            if (!this.code)
                this.$router.push('/error');
        },

        props: {
            code: {
                type: String,
                required: true
            }
        },

        data() {
            return {
                email: '',
                password: '',
                confirmPassword: '',
                showSuccessConfirmation: false,
            }
        },

        validations: {
            email: {
                required,
                email
            },
            password: {
                required,
                minLength: minLength(6)
            },
            confirmPassword: {
                required,
                confirmPassword: sameAs('password')
            }
        },

        methods: {
            async onSubmit() {
                let data = { code: this.code, email: this.email, password: this.password, confirmPassword: this.confirmPassword };

                await accountService.resetPassword(data);

                this.showSuccessConfirmation = true;
            }
        }
    }
</script>