<template>
    <div>
        <h1 class="page-header">Set Password</h1>

        <div class="alert alert-warning" role="alert">
            You do not have a local username/password for this site. Add a local
            account so you can log in without an external login.
        </div>

        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.newPassword" label="New password" label-for="newPassword">
                <b-form-input id="newPassword" v-model="newPassword" type="password" />
            </v-form-group>

            <v-form-group :validator="$v.confirmPassword" label="Confirm password" label-for="confirmPassword">
                <b-form-input id="confirmPassword" v-model="confirmPassword" type="password" />
            </v-form-group>
        </b-form>
        <div class="float-right">
            <btn-loading :loading="loading" @click="submit">Set Password</btn-loading>
        </div>
    </div>
</template>

<script>
    import Vue from 'vue';
    import { required, sameAs } from 'vuelidate/lib/validators';
    import accountService from 'services/account-service';
    import formMixin from 'mixins/form-mixin';

    export default {
        mixins: [formMixin],

        data() {
            return {
                confirmPassword: '',
                newPassword: ''
            }
        },

        validations: {
            newPassword: {
                required
            },
            confirmPassword: {
                required,
                confirmPassword: sameAs('newPassword')
            }
        },

        beforeRouteEnter(to, from, next) {
            next((c) => c.checkAccount());
        },

        methods: {
            checkAccount() {
                if (this.$auth.hasLocalAccount)
                    this.$router.push('/profile/change-password');
            },
            async onSubmit() {
                let requestData = {
                    newPassword: this.newPassword,
                    confirmPassword: this.confirmPassword
                }

                let res = await accountService.setUserPassword(requestData);

                this.$root.successToast(res.message);

                await this.$auth.updateUserInfo();
                this.$router.push('/profile/change-password');   
                this.$parent.$children[0].loadProfileNodes();
            }
        }

    }
</script>
