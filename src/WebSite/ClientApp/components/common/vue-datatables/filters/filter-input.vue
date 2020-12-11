<template>
    <b-form-input :value="query" @input="debounceQuery" :size="size" :placeholder="placeholder" autocomplete="off" />
</template>

<script>
    import _ from 'lodash';
    import Vue from 'vue';
    import { defaults, VueDataTablesFiltersMixin } from 'vue-datatables.net';

    export default Vue.extend({
        mixins: [VueDataTablesFiltersMixin],

        props: {
            placeholder: {
                type: String,
                default: defaults.dtOptions.language.searchPlaceholder
            },
            debounce: {
                type: Number,
                default: 300
            },
            size: {
                type: String
            }
        },

        created() {
            this.debounceQuery = _.debounce(this.debounceQuery, this.debounce);
        },

        methods: {
            debounceQuery(value) {
                this.query = value;
            }
        }
    })
</script>