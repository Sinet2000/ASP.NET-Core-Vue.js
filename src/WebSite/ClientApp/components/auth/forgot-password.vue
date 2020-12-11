<template>
    <div class="login-wrap">
        <div class="login-logo">
            <router-link to="/"><img src="http://placehold.it/100x100/efefef/?text=LOGO" alt="" /></router-link>
        </div>
        <div class="card card-login">
            <div class="card-header">
                <h1 class="card-title">Password Reminder</h1>
            </div>
            <div class="card-body">
                <b-form @submit.prevent="submit" v-if="!showSuccessConfirmation">
                    <p class="text-center">Enter your email address and we will send you a link to reset your password.</p>

                    <validation-summary :errors="errors"></validation-summary>

                    <v-form-group :validator="$v.email" label="Email" hide-label>
                        <b-form-input v-model="email" placeholder="Email" />
                    </v-form-group>

                    <div class="form-group">
                        <btn-loading :loading="loading" class="btn-block" type="submit">Send Link</btn-loading>
                    </div>
                </b-form>
                <p class="text-center" v-else>
                    Please check your email to reset your password.
                </p>
            </div>
            </div>
        <div class="bottom-text">
            <router-link to="/login">Back to Sign In</router-link>
        </div>
    </div>
</template>

<script>
    import { required, email } from 'vuelidate/lib/validators';
    import Vue from 'vue';
    import ApiError from 'utils/api-error';
    import accountService from 'services/account-service';
    import formMixin from 'mixins/form-mixin';

    export default {
        mixins: [formMixin],

        data() {
            return {
                email: '',
                showSuccessConfirmation: false,
            }
        },

        validations: {
            email: {
                required,
                email
            }
        },

        methods: {
            async onSubmit() {
                let data = { email: this.email };

                await accountService.forgotPassword(data);

                this.showSuccessConfirmation = true;
            }
        }
    }
</script>