<template>
    <div class="login-wrap">
        <div class="login-logo">
            <router-link to="/"><img src="http://placehold.it/100x100/efefef/?text=LOGO" alt="" /></router-link>
        </div>
        <div class="card card-login">
            <div class="card-header">
                <h1 class="card-title">Email Confirmation</h1>
            </div>
            <div class="card-body text-center">
                <p v-if="!confirmed">Confirming your email, please wait...</p>
                <template v-else>
                    <template v-if="!errorMessage">
                        <p>Thank you for confirming your email. Please Sign In.</p>
                        <b-btn class="btn-block" variant="primary" to="/login">Sign In</b-btn>
                        </template>
                    <p v-else class="text-danger">{{ errorMessage }}</p>
                </template>
            </div>
        </div>
        <div class="bottom-text">
            <router-link to="/login">Back to Sign In</router-link>
        </div>
    </div>
</template>

<script>
    import Vue from 'vue';
    import accountService from 'services/account-service';

    export default {
        async beforeRouteEnter (to, from, next) {
            next(async (c) => await c.confirm());
        },

        props: {
            userId: {
                type: String,
                required: true
            },
            token: {
                type: String,
                required: true
            }
        },

        data() {
            return {
                confirmed: false,
                errorMessage: ''
            }
        },

        methods: {
            async confirm() {
                let data = { userId: this.userId, token: this.token };

                try {
                    await accountService.confirmEmail(data);
                }
                catch (error) {
                    this.errorMessage = error.message;
                }
                finally {
                    this.confirmed = true;
                }
            }
        }
    }
</script>