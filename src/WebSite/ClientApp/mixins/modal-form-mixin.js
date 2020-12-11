export default {
    data() {
        return {
            errors: [],
            loading: false,
            wasSubmitted: false
        };
    },

    created() {
        this.resetData();
    },

    mounted() {
        this.$refs.modal.$on('hidden', this.modalHidden);
    },

    methods: {

        resetData() {
            Object.assign(this.$data, this.$options.data.call(this));
            this.$v.$reset();
        },

        async show(args) {

            this.resetData();

            await this.beforeShow(args);

            this.$refs.modal.show();
        },

        cancel() {
            this.$refs.modal.hide();
        },

        async submit() {
            if (this.$v.$invalid) {
                this.$v.$touch();
                return;
            }

            this.errors = [];
            this.loading = true;

            try {
                await this.onSubmit();

                this.wasSubmitted = true;

                this.$refs.modal.hide();
            }
            catch (error) {
                this.errors = error.customErrors;
            }
            finally {
                this.loading = false;
            }
        },

        modalHidden() {
            if (this.wasSubmitted)
                this.$emit('success');
        },

        async beforeShow(args) {},

        async onSubmit() {}
    }
}