<template>
    <div class="login-wrap">
        <div class="login-logo">
            <router-link to="/"><img src="http://placehold.it/100x100/efefef/?text=LOGO" alt="" /></router-link>
        </div>
        <div class="card card-login">
            <template v-if="!pleaseConfirmEmail">
                <div class="card-header">
                    <h1 class="card-title">Create an Account</h1>
                </div>
                <div class="card-body">
                    <b-form @submit.prevent="submit">
                        <validation-summary :errors="errors"></validation-summary>

                        <v-form-group :validator="$v.email" label="Email">
                            <b-form-input v-model="email" />
                        </v-form-group>
                        <v-form-group :validator="$v.fullName" label="Full Name">
                            <b-form-input v-model="fullName" />
                        </v-form-group>
                        <v-form-group :validator="$v.password" label="Password">
                            <b-form-input v-model="password" type="password" />
                        </v-form-group>
                        <v-form-group :validator="$v.confirmPassword" label="Confirm Password">
                            <b-form-input v-model="confirmPassword" type="password" />
                        </v-form-group>
                        <div class="form-group">
                            <btn-loading :loading="loading" class="btn-block" type="submit">Create</btn-loading>
                        </div>
                    </b-form>
                </div>
            </template>
            <template v-else>
                <div class="card-header">
                    <h1 class="card-title">Confirm Email Address</h1>
                </div>
                <div class="card-body text-center">
                    <p>We've just sent an email to {{ email }}.</p>
                    <p>Please follow the link in that email to verify your account and complete registration.</p>
                </div>
            </template>
        </div>
        <div class="bottom-text">
            Already have an account? <router-link to="/login">Sign In</router-link>
        </div>
    </div>
</template>

<script>
    import { mapState } from 'vuex';
    import { required, email, sameAs } from 'vuelidate/lib/validators';
    import formMixin from 'mixins/form-mixin';
    import accountService from 'services/account-service';

    export default {
        mixins: [formMixin],

        data() {
            return {
                email: '',
                fullName: '',
                password: '',
                confirmPassword: '',
                pleaseConfirmEmail: false
            }
        },

        validations: {
            email: {
                required,
                email
            },
            fullName: {
                required
            },
            password: {
                required
            },
            confirmPassword: {
                required,
                confirmPassword: sameAs('password')
            }
        },

        methods: {
            async onSubmit() {
                let res = await accountService.register({ email: this.email, fullName: this.fullName, password: this.password });

                if (res.canLogin) {
                    await this.$auth.login(this.email, this.password, false, '/');
                    return;
                }

                if (res.requireConfirmedEmail)
                    this.pleaseConfirmEmail = true;
            }
        }
    }
</script>