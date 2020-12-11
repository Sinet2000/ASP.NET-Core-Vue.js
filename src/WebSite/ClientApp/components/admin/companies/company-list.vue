<template>
    <div>
        <div class="page-header with-actions">
            <h1>Companies</h1>
            <div class="actions">
                <b-btn @click="createCompany" variant="primary"><i class="oi oi-plus"></i> New Company</b-btn>
            </div>
        </div>

        <dt-filters-group :tableId="tableId">
            <div class="row">
                <div class="col-3">
                    <div class="form-group">
                        <label for="name-filter">Company Name</label>
                        <dt-filter-input id="name-filter" column-name="Name" size="sm" placeholder="Company Name" />
                    </div>
                </div>
            </div>
        </dt-filters-group>

        <dt-datatable ref="companyList" :id="tableId" url="/Admin/CompanyListAjax" :columns="columns" :dt-options="dtOptions">
            <template #actions="{ data: company }">
                <buttons-edit-delete :data="company" :editAction="editCompany" :deleteAction="deleteCompany" />
            </template>
        </dt-datatable>

        <create-company-modal ref="createCompanyModal" @success="companyCreated"></create-company-modal>
        <edit-company-modal ref="editCompanyModal" @success="companyUpdated"></edit-company-modal>
    </div>
</template>

<script>
    import Vue from 'vue';

    import adminService from 'services/admin-service';

    import ButtonsEditDelete from 'components/common/vue-datatables/column-renderers/buttons-edit-delete';
    import CreateCompanyModal from './create-company-modal';
    import EditCompanyModal from './edit-company-modal';

    export default {

        components: {
            'buttons-edit-delete': ButtonsEditDelete,
            'create-company-modal': CreateCompanyModal,
            'edit-company-modal': EditCompanyModal
        },

        data() {
            return {
                tableId: 'company-list',

                sampleFilterOptions: {
                    1 : 'First',
                    2 : 'Second'
                },

                columns: [
                    'Id',
                    'Name',
                    { name: 'Actions', data: false }
                ],

                dtOptions: {
                    columnDefs: [
                        { width: '10%', targets: 0 },
                        { width: '15%', targets: 2 },
                    ]
                }
            }
        },

        methods: {
            async createCompany(data) {
                this.$refs.createCompanyModal.show();
            },

            async editCompany(data) {
                this.$refs.editCompanyModal.show(data.id);
            },

            async deleteCompany(data) {
                try {
                    await adminService.deleteCompany(data.id);

                    this.redrawTable();

                    this.$root.successToast('Company deleted');
                }
                catch(error) {
                    this.$root.errorToast(error.customErrors[0]);
                }
            },

            companyCreated() {
                this.redrawTable();

                this.$root.successToast('Company created');
            },

            companyUpdated() {
                this.redrawTable();

                this.$root.successToast('Company updated');
            },

            redrawTable() {
                this.$refs.companyList.draw();
            },
        }
    }
</script>