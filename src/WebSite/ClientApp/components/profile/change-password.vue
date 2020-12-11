<template>
    <div>
        <h1 class="page-header">Change Password</h1>

        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.oldPassword" label="Old password" label-for="oldPassword">
                <b-form-input id="oldPassword" v-model="oldPassword" type="password"/>
            </v-form-group>

            <v-form-group :validator="$v.newPassword" label="New password" label-for="newPassword">
                <b-form-input id="newPassword" v-model="newPassword" type="password"/>
            </v-form-group>

            <v-form-group :validator="$v.confirmPassword" label="Confirm password" label-for="confirmPassword">
                <b-form-input id="confirmPassword" v-model="confirmPassword" type="password"/>
            </v-form-group>
        </b-form>
        <div class="float-right">
            <btn-loading :loading="loading" @click="submit">Change Password</btn-loading>
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
                oldPassword: '',
                confirmPassword: '',
                newPassword: ''
            }
        },

        validations: {
            oldPassword: {
                required
            },
            newPassword: {
                required
            },
            confirmPassword: {
                required,
                confirmPassword: sameAs('newPassword')
            }
        },

        methods: {
            async onSubmit() {
                let requestData = {
                    oldPassword: this.oldPassword,
                    newPassword: this.newPassword,
                    confirmPassword: this.confirmPassword
                }

                let res = await accountService.changeUserPassword(requestData);

                this.$root.successToast(res.message);
            }
        }

    }
</script>
