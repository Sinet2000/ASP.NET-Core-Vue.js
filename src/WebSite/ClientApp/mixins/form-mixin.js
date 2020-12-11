export default {
    data() {
        return {
            errors: [],
            loading: false,
            wasSubmitted: false,
        };
    },
    methods: {
        async submit() {
            this.errors = [];

            if (this.$v.$invalid) {
                this.$v.$touch();
                return;
            }

            this.loading = true;

            try {
                await this.onSubmit();
            }
            catch (error) {
                this.errors = error.customErrors;
            }
            finally {
                this.loading = false;
            }
        },

        async onSubmit() { }
    }
}