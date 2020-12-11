<template>
    <b-form-select v-model="query" :size="size">
        <option v-for="option in internalOptions" v-bind:value="option.value">
            {{ option.text }}
        </option>
    </b-form-select>
</template>
<script>
    import Vue from 'vue';
    import { VueDataTablesFiltersMixin } from 'vue-datatables.net';

    export default Vue.extend({
        mixins: [VueDataTablesFiltersMixin],

        props: {
            options: {
                type: Array,
                required: true
            },

            size: {
                type: String
            },

            addEmpty: {}
        },

        data: () => ({
        }),

        computed: {
            internalOptions() {
                let options = this.options;

                if (this.addEmpty === 'true')
                    options.splice(0, 0, { text: '', value: '' });

                return options;
            }
        }
    });
</script>
