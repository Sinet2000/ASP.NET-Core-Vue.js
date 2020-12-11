<template>
    <div v-if="errorsArr.length > 0">
        <div class="validation-summary-errors">
            <p v-for="err in errorsArr">{{ err }}</p>
        </div>
    </div>
</template>

<script>
    module.exports = {
        props: {
            errors: {
                type: [String, Array],
                required: true,
                default: function () {
                    return [];
                }
            },
            success: null
        },

        data: () => ({
            errorsArr: []
        }),

        watch: {
            errors() {
                this.errorsArr = Array.isArray(this.errors) ? this.errors : [this.errors];
            }
        }
    };
</script>