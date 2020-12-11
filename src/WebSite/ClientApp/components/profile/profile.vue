<template>
    <div class="row">
        <profile-sidebar></profile-sidebar>
        <router-view class="col-lg-9"></router-view>
    </div>
</template>

<script>
    import Vue from 'vue';
    import ProfileSidebar from './profile-sidebar'

    Vue.component('profile-sidebar', ProfileSidebar);

    export default {
        data() {
            return {
            }
        },
    }
</script>
