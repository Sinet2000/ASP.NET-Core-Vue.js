<template>
    <b-btn :type="type" :class="['ld-ext-right', { running: loading }]" @click="handleClick" :variant="variant" :disabled="loading"><slot></slot><div class="ld ld-ring ld-spin"></div></b-btn>
</template>

<style src="./loading.css"></style>
<style src="./loading-btn.css"></style>

<script>
    module.exports = {
        props: {
            loading: {
                type: Boolean,
                required: true
            },
            variant: {
                type: String,
                default: 'primary'
            },
            type: {
                type: String,
                default: 'button'
            }
        },

        methods: {
            handleClick(e) {
                this.$emit('click', e);
            }
        }
    };
</script>