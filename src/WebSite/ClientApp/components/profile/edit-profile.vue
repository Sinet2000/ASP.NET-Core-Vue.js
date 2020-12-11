<template>
    <div>
        <h1 class="page-header">My Profile</h1>

        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.email" label="Email" label-for="email">
                <b-form-input id="email" v-model="email" />
            </v-form-group>
            <v-form-group :validator="$v.fullName" label="Full Name" label-for="fullName">
                <b-form-input id="fullName" v-model="fullName" />
            </v-form-group>
        </b-form>
        <div class="float-right">
            <btn-loading :loading="loading" @click="submit">Save</btn-loading>
        </div>
    </div>
</template>

<script>
    import Vue from 'vue';
    import { required, email } from 'vuelidate/lib/validators';
    import accountService from 'services/account-service';
    import formMixin from 'mixins/form-mixin';

    export default {
        mixins: [formMixin],

        data() {
            return {
                email: null,
                fullName: null,
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
        },

        async beforeRouteEnter(to, from, next) {
            next(async (c) => await c.initData());
        },

        methods: {
            async initData() {
                let data = await accountService.getBaseInfo();

                this.email = data.email;
                this.fullName = data.fullName;
            },

            async onSubmit() {
                let requestData = {
                    email: this.email,
                    fullName: this.fullName
                }

                let res = await accountService.changeBaseInfo(requestData);

                this.$root.successToast(res.message);
                this.$auth.updateUserInfo();
            }
        }
    }
</script>
