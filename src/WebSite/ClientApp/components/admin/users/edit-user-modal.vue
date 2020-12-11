<template>
    <b-modal ref="modal" :title="'Edit User ' + fullName">
        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.email" label="Email">
                <b-form-input v-model="email" />
            </v-form-group>
            <v-form-group :validator="$v.selectedCompany" label="Company">
                <b-form-select v-model="selectedCompany" :options="companies" />
            </v-form-group>
            <v-form-group :validator="$v.fullName" label="Full Name">
                <b-form-input v-model="fullName" />
            </v-form-group>
            <v-form-group :validator="$v.password" label="New Password">
                <b-form-input v-model="password" type="password" />
            </v-form-group>
            <v-form-group :validator="$v.confirmPassword" label="Confirm New Password">
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
                id: null,
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
            confirmPassword: {
                confirmPassword: sameAs('password')
            }
        },

        methods: {

            async beforeShow(id) {

                let data = await adminService.editUserData(id);

                this.companies = data.companies;

                this.id = id;
                this.email = data.email;
                this.selectedCompany = data.companyId;
                this.fullName = data.fullName;
                this.disabled = data.disabled;
            },

            async onSubmit() {

                let data = {
                    id: this.id,
                    email: this.email,
                    companyId: this.selectedCompany,
                    fullName: this.fullName,
                    disabled: this.disabled,
                    password: this.password
                };

                await adminService.editUser(data);
            }
        }
    }
</script>