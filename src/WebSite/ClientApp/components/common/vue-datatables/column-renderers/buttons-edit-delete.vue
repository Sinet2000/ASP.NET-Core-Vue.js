<template>
    <div class="btn-group">
        <a v-if="editAction" class="btn btn-secondary btn-sm dt-action" title="Edit" @click="editAction(data)"><i class="oi oi-pencil"></i> Edit</a>
        <a v-if="deleteAction" class="btn btn-secondary btn-sm dt-action" title="Delete" @click="deleteAction(data)"><i class="oi oi-trash"></i></a>
    </div>
</template>

<script>
    export default {
        props: {
            data: {
                type: Object,
                required: true
            },
        
            editAction: {
                type: Function
            },

            deleteAction: {
                type: Function,
            },
      },
    }
</script>
