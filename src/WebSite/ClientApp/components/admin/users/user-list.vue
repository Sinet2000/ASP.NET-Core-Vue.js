<template>
    <div>
        <div class="page-header with-actions">
            <h1>Users</h1>
            <div class="actions">
                <b-btn @click="createUser" variant="primary"><i class="oi oi-plus"></i> New User</b-btn>
            </div>
        </div>

        <dt-filters-group :tableId="tableId">
            <div class="form-group">
                <label for="search-filter">Search</label>
                <dt-filter-input id="search-filter" size="sm" />
            </div>

            <div class="row">
                <div class="col-3">
                    <div class="form-group">
                        <label for="id-filter">Id</label>
                        <dt-filter-input id="id-filter" column-name="Id" placeholder="Id" size="sm" />
                    </div>
                </div>
                <div class="col-3">
                    <div class="form-group">
                        <label for="enabled-filter">Enabled</label>
                        <dt-filter-select id="enabled-filter" column-name="Disabled" :options="enabledFilterOptions" add-empty="true" size="sm" />
                    </div>
                </div>
                <div class="col-3">
                    <div class="form-group">
                        <label for="role-filter">Role</label>
                        <dt-filter-select id="role-filter" column-name="Role" :options="roleFilterOptions" size="sm" />
                    </div>
                </div>
                <div class="col-3">
                    <div class="form-group">
                        <label for="create-date-filter">Create Date</label>
                        <dt-filter-daterange column-name="CreateDate" placeholder="Create Date" size="sm" />
                    </div>
                </div>
            </div>
        </dt-filters-group>

        <dt-datatable ref="userList" :id="tableId" url="/Admin/UserListAjax" :columns="columns" :dt-options="dtOptions" @selection-changed="onSelectionChanged">
            <template #disabled="{ data }">
                <span v-if="data.disabled" class="badge badge-secondary">No</span>
                <span v-else class="badge badge-success">Yes</span>
            </template>
            <template #actions="{ data: user }">
                <b-dropdown text="Edit" @click="editUser(user)" split right size="sm">
                    <b-dropdown-item-button v-if="user.role !== 'Admin'" @click="viewAs(user)"><i class="oi oi-eye"></i> View as User</b-dropdown-item-button>
                    <b-dropdown-item-button @click="disableUser(user)"><i :class="['oi', user.disabled ?  'oi-circle-check' : 'oi-circle-x']"></i> {{ user.disabled ? 'Enable' : 'Disable' }}</b-dropdown-item-button>
                    <b-dropdown-item-button @click="deleteUser(user)"><i class="oi oi-trash"></i> Delete</b-dropdown-item-button>
                </b-dropdown>
            </template>
        </dt-datatable>

        <p class="mt-2">Selected Ids: {{ selected }}</p>

        <create-user-modal ref="createUserModal" @success="userCreated"></create-user-modal>
        <edit-user-modal ref="editUserModal" @success="userUpdated"></edit-user-modal>
    </div>
</template>

<script>
    import Vue from 'vue';
    import { required, email, sameAs } from 'vuelidate/lib/validators';

    import adminService from 'services/admin-service';

    import CreateUserModal from './create-user-modal';
    import EditUserModal from './edit-user-modal';

    export default {

        components: {
            'create-user-modal': CreateUserModal,
            'edit-user-modal': EditUserModal
        },

        data() {
            return {
                tableId: 'user-list',

                selected: [],

                enabledFilterOptions: [
                    { text: 'Yes', value: false },
                    { text: 'No', value: true }
                ],

                roleFilterOptions: [],

                columns: [
                    { name: 'Select', type: 'select', data: 'id' },
                    'Id',
                    { name: 'CompanyName', title: 'Company' },
                    'Email',
                    'FullName',
                    { name: 'Role', sortable: false },
                    { name: 'Disabled', title: 'Enabled' },
                    'CreateDate',
                    { name: 'Actions', data: false }
                ],

                dtOptions: {
                    order: [[1, 'asc']],
                    columnDefs: [
                        { width: '30px', targets: 0 },
                        { width: '15%', targets: 2 },
                    ]
                },

                form: {
                    createUser: {
                        email: '',
                        fullName: '',
                        password: '',
                        confirmPassword: '',
                        errors: [],
                        loading: false
                    }
                }
            }
        },

        validations: {
            form: {
                createUser: {
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
                }
            }
        },

        async beforeRouteEnter(to, from, next) {
            next(async (c) => await c.initData());
        },

        methods: {
            async initData() {
                var data = await adminService.getUserListData();

                this.roleFilterOptions = data.roles;
            },

            createUser(data) {
                this.$refs.createUserModal.show();
            },

            viewAs(data) {
                data.disabled ? this.$root.errorToast('Please enable user first') : this.$auth.loginAsUser(data.email);
            },

            editUser(data) {
                this.$refs.editUserModal.show(data.id);
            },

            async disableUser(data) {
                await adminService.disableUser(data.id);

                this.redrawTable();

                this.$root.successToast(data.disabled ? 'User enabled' : 'User disabled');
            },

            async deleteUser(data) {
                await adminService.deleteUser(data.id);

                this.redrawTable();

                this.$root.successToast('User deleted');
            },

            userCreated() {
                this.redrawTable();

                this.$root.successToast('User created');
            },

            userUpdated() {
                this.redrawTable();

                this.$root.successToast('User updated');
            },

            onSelectionChanged(val) {
                this.selected = val;
            },

            redrawTable() {
                this.$refs.userList.draw();
            }
        }
    }
</script>