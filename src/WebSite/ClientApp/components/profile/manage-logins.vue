<template>
    <div>
        <div class="page-header with-actions">
            <h1>Manage Logins</h1>
            <div class="actions">
                <b-dropdown text="Add Log In" v-if="availableLoginProviders.length > 0">
                    <b-dropdown-item v-for="provider in availableLoginProviders" @click="linkLoginProvider(provider.key)" :key="provider.key">{{ provider.name }}</b-dropdown-item>
                </b-dropdown>
            </div>
        </div>

        <dt-datatable ref="loginProvidersList" :id="tableId" url="/Profile/LoginProvidersListAjax" :columns="columns" :dt-options="dtOptions">
            <template #actions="{ data: provider }">
                <buttons-delete :data="provider" :deleteAction="deleteLoginProvider" v-if="deleteAllowed()" />
            </template>
        </dt-datatable>
    </div>
</template>

<script>
    import Vue from 'vue';
    import ButtonsEditDelete from 'components/common/vue-datatables/column-renderers/buttons-edit-delete';
    import accountService from '../../services/account-service';

    export default {

        components: {
            'buttons-delete': ButtonsEditDelete,
        },

        data() {
            return {
                tableId: 'external-logins',
                columns: [
                    { name: 'ProviderDisplayName', title: 'Provider' },
                    { name: 'actions', data: false }
                ],

                dtOptions: {
                    columnDefs: [
                        { width: '80%', targets: 0 }
                    ]
                },
                availableLoginProviders: []
            }
        },

        beforeRouteEnter(to, from, next) {
            next(async (c) => await c.initData());
        },

        methods: {
            deleteAllowed() {
                let totalRecords = this.$refs.loginProvidersList.getDataTableApi().settings()[0].fnRecordsTotal();

                return this.$auth.hasLocalAccount || totalRecords > 1;
            },

            async initData() {
                await this.loadAvailableLoginProviders();
            },

            async linkLoginProvider(provider) {
                try {
                    let res = await this.$auth.providerLogin(provider, null, true);

                    await this.redrawTable();

                    this.$root.successToast(res.message);
                }
                catch (error) {
                    this.$root.errorToast(error.customErrors[0]);
                }
            },

            async loadAvailableLoginProviders() {
                this.availableLoginProviders = await accountService.getAvailableLoginProviders();
            },

            async deleteLoginProvider(data) {
                try {
                    let res = await accountService.deleteLoginProvider({ LoginProvider: data.loginProvider, ProviderKey: data.providerKey });                  

                    await this.redrawTable();

                    this.$root.successToast(res.message);
                }
                catch (error) {
                    this.$root.errorToast(error.customErrors[0]);
                }
            },

            async redrawTable() {
                await this.loadAvailableLoginProviders();
                this.$refs.loginProvidersList.draw();
            }
        }
    }
</script>
