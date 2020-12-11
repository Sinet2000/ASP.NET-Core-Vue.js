<template>
    <b-modal ref="modal" title="Create New User">
        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.email" label="Email">
                <b-form-input v-model="email" />
            </v-form-group>
            <v-form-group :validator="$v.selectedCompany" label="Company">
                <b-form-select v-model="selectedCompany" :options="companies" />
            </v-form-group>
            <v-form-group :validator="$v.fullName" label="Full Name" label-for="fullName">
                <b-form-input id="fullName" v-model="fullName" />
            </v-form-group>
            <v-form-group :validator="$v.password" label="Password">
                <b-form-input v-model="password" type="password" />
            </v-form-group>
            <v-form-group :validator="$v.confirmPassword" label="Confirm Password">
                <b-form-input v-model="confirmPassword" type="password" />
            </v-form-group>
            <b-form-group>
                <b-form-checkbox v-model="disabled">Disabled</b-form-checkbox>
            </b-form-group>
        </b-form>
        <div slot="modal-footer">
            <b-btn @click="cancel" variant="secondary">Cancel</b-btn>
            <btn-loading :loading="loading" @click="submit">OK</btn-loading>
        </div>
    </b-modal>
</template>

<script>
    import Vue from 'vue';
    import { required, email, sameAs } from 'vuelidate/lib/validators';

    import adminService from 'services/admin-service';
    import modalFormMixin from 'mixins/modal-form-mixin';

    export default {
        mixins: [modalFormMixin],

        data() {
            return {
                email: '',
                selectedCompany: null,
                companies: [],
                fullName: '',
                password: '',
                confirmPassword: '',
                disabled: false
            };
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
            async beforeShow() {

                let data = await adminService.createUserData();

                this.companies = data.companies;
            },

            async onSubmit() {

                let data = {
                    email: this.email,
                    companyId: this.selectedCompany,
                    fullName: this.fullName,
                    disabled: this.disabled,
                    password: this.password
                };

                await adminService.createUser(data);
            }
        }
    }
</script>