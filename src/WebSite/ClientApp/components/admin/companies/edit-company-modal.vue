<template>
    <b-modal ref="modal" :title="'Edit Company ' + name" lazy>
        <b-form @submit.prevent="submit">
            <validation-summary :errors="errors"></validation-summary>

            <v-form-group :validator="$v.name" label="Name">
                <b-form-input v-model="name" />
            </v-form-group>
        </b-form>
        <div slot="modal-footer">
            <b-btn @click="cancel" variant="secondary">Cancel</b-btn>
            <btn-loading :loading="loading" @click="submit">OK</btn-loading>
        </div>
    </b-modal>
</template>

<script>
    import Vue from 'vue';
    import { required } from 'vuelidate/lib/validators';

    import adminService from 'services/admin-service';
    import modalFormMixin from 'mixins/modal-form-mixin';

    export default {
        mixins: [modalFormMixin],

        data() {
            return {
                id: null,
                name: ''
            };
        },

        validations: {
            name: {
                required
            }
        },

        methods: {
            async beforeShow(id) {

                let data = await adminService.editCompanyData(id);

                this.id = id;
                this.name = data.name;
            },

            async onSubmit() {

                let data = {
                    id: this.id,
                    name: this.name
                };

                await adminService.editCompany(data);
            }
        }
    }
</script>