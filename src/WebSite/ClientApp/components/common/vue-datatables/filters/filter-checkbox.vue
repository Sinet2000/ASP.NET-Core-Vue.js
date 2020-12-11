<template>
    <b-form-checkbox v-model="checked" :size="size"><slot></slot></b-form-checkbox>
</template>
<script>
    import Vue from 'vue';
    import { VueDataTablesFiltersMixin } from 'vue-datatables.net';

    export default Vue.extend({
        mixins: [VueDataTablesFiltersMixin],

        props: {
            size: {
                type: String
            }
        },

        data: () => ({
            checked: false
        }),

        created() {
            this.checked = this.query === 'true';
        },

        watch: {
            checked() {
                this.query = this.checked.toString();
            }
        }

    });
</script>
