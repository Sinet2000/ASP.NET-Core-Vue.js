<template>
    <input ref="input" :placeholder="placeholder" type="text" />
</template>

<script>
    import 'bootstrap-daterangepicker';
    import 'bootstrap-daterangepicker/daterangepicker.css';
    import moment from 'moment';

    import { default as defaults, FORMAT } from './defaults';

    export default {
        props: {
            // more options see http://www.daterangepicker.com/#options
            config: {
                type: Object,
                default: () => {}
            },

            value: {
                type: String,
                default: ''
            },

            placeholder: {
                type: String,
                default: 'Select a date range...'
            }
        },
        mounted() {

            let that = this;

            this.$nextTick(() => {
                $(this.$refs.input).daterangepicker({

                    locale: { format: FORMAT, cancelLabel: 'Clear' },
                    cancelClass: "btn-secondary",
                    autoUpdateInput: false,
                    showDropdowns: true,
                    opens: 'right',

                    ranges: {
                        'Today': [moment(), moment()],
                        'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                        'This Month': [moment().startOf('month'), moment().endOf('month')],
                        'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
                    },

                    //startDate: defaults.startDate,
                    //endDate: defaults.endDate,

                    ...this.config
                })
                .on('apply.daterangepicker', function (ev, picker) {

                    this.value = picker.startDate.format(FORMAT) + ' - ' + picker.endDate.format(FORMAT);
                    that.$emit('input', this.value);

                }).on('cancel.daterangepicker', function (ev, picker) {

                    this.value = '';
                    that.$emit('input', this.value);
                });
            });
        }
    }
</script>