<template>
  <select v-model="perPage" class="form-control">
    <option v-for="val in options" :value="val">{{ val }}</option>
  </select>
</template>

<script>
    import defaults from '../defaults';

    export default {
        props: {
            value: {
                type: Number,
                required: true
            },

            options: {
                type: Array,
                default: () => defaults.perPageOptions
            }
        },

        data: () => ({ perPage: defaults.perPage }),

        created () {
            this.perPage = this.value;
        },

        watch: {
            perPage (val) {
                this.$emit('input', val);
            }
        }
    }
</script>
