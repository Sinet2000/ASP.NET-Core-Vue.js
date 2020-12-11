<template>
    <div>
        <div v-if="header" class="page-header">
            <h1>{{ header }}</h1>
        </div>
        <b-alert :variant="alertType" show>
            <template v-if="message">
                {{ message }}
            </template>
            <template v-else>
                <slot></slot>
            </template>
        </b-alert>
    </div>
</template>

<script>
    export default {
        props: {
            header: {
                type: String,
                required: false
            },
            alertType: {
                default: 'success',
                type: String,
                required: false
            },
            message: {
                type: String,
                required: false
            }
        }
    };
</script>