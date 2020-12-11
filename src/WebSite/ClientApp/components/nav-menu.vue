<template>
    <div class="navbar navbar-expand-md navbar-dark">
        <div class="container">
            <button type="button" class="navbar-toggler" @click="toggleCollapsed">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" :class="{ show: showCollapsed }" @click="toggleCollapsedOnClick">
                <ul class="navbar-nav mr-auto">
                    <template v-for="route in visibleRoutes">
                        <router-link tag="li" class="nav-item" :to="route.path" exact>
                            <a class="nav-link">{{ route.display }}</a>
                        </router-link>
                    </template>
                </ul>
                <ul class="navbar-nav" v-if="user">
                    <li is="b-dropdown" variant="link" right class="nav-item">
                        <span slot="button-content" class="nav-link"><i class="oi oi-person"></i> {{ user.email }}</span>
                        <b-dropdown-item to="/profile">
                            <i class="oi oi-cog"></i> My Profile
                        </b-dropdown-item>
                        <b-dropdown-item @click.prevent="loginBackAsAdmin()" v-if="isLoggedInAsAnotherUser">
                            <i class="oi oi-action-undo"></i> Back to Admin
                        </b-dropdown-item>
                        <b-dropdown-item @click.prevent="logout()">
                            <i class="oi oi-account-logout"></i> Log Out
                        </b-dropdown-item>
                    </li>
                </ul>
                <ul class="navbar-nav ml-auto" v-else-if="!isAuthenticated">
                    <router-link tag="li" class="nav-item" to="/register"><a class="nav-link">Register</a></router-link>
                    <router-link tag="li" class="nav-item" to="/login"><a class="nav-link">Sign In</a></router-link>
                </ul>
            </div>
        </div>
    </div>
</template>

<script>
    import { mapState } from 'vuex';
    import { routes } from '../routes';

    export default {
        data() {
            return {
                showCollapsed: false
            }
        },

        computed: {
            //...mapState('user', ['accessToken', 'user']),

            isAuthenticated() {
                return this.$auth.isAuthenticated;
            },

            user() {
                return this.$auth.user;
            },

            isLoggedInAsAnotherUser() {
                return this.$auth.isLoggedInAsAnotherUser;
            },

            visibleRoutes() {
                return routes.filter(route => {
                    if (!(route.meta && route.meta.showInMenu))
                        return false;

                    if (!route.meta.auth)
                        return true;

                    return this.$auth.isRouteAccessible(route);
                });
            }
        },

        methods: {
            toggleCollapsed() {
                this.showCollapsed = !this.showCollapsed;
            },

            toggleCollapsedOnClick() {
                if (this.showCollapsed)
                    this.showCollapsed = false;
            },

            async loginBackAsAdmin() {
                await this.$auth.loginBackAsAdmin();
            },

            async logout() {
                await this.$auth.logout('/');
            }
        }
    }
</script>