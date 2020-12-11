<template>
    <daterange-picker v-model="query" :placeholder="placeholder" class="form-control" :class="sizeClass"></daterange-picker>
</template>

<script>
    import Vue from 'vue';
    import DateRangePicker from 'vue-daterangepicker/daterangepicker';
    import { VueDataTablesFiltersMixin } from 'vue-datatables.net';

    export default Vue.extend({
        mixins: [VueDataTablesFiltersMixin],

        props: {
            placeholder: {
                type: String
            },

            size: {
                type: String
            }
        },

        computed: {
            sizeClass() {
                if (this.size === 'sm')
                    return 'form-control-sm';

                if (this.size === 'lg')
                    return 'form-control-lg';

                return '';
            }
        },

        components: {
            'daterange-picker': DateRangePicker
        }
    });
</script>
