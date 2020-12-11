<template>
    <div class="col-lg-3">
        <div class="my-profile-sidebar">
            <ul class="list-unstyled">
                <li v-for="route in nodes">
                    <router-link :to="route.fullPath">{{ route.display }}</router-link>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
    import Vue from 'vue';
    import { routes } from 'routes';

    export default {
        data() {
            return {
                nodes: []
            }
        },
        async created() {
            if (!this.$auth.user)
                await this.$auth.updateUserInfo();

            this.loadProfileNodes();
        },
        methods: {
            loadProfileNodes() {
                let node = routes.filter(route => route.path === '/profile').pop();
                let hasLocalAccount = this.$auth.hasLocalAccount;
                this.nodes = [];

                node.children.forEach((route, i) => {
                    route['fullPath'] = node.path + '/' + route.path;

                    if ((!hasLocalAccount && route.path != 'change-password') || (hasLocalAccount && route.path != 'set-password')) {                        
                        this.nodes.push(route);
                    }
                });
            }
        },

    }
</script>
