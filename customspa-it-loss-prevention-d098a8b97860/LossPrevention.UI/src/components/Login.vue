<template>
    <v-container class="d-flex justify-center align-center" style="height: 40vh;">
        <v-card width="400" elevation="4">
            <v-card-title class="text-h6 text-center">Login</v-card-title>
            <v-card-text>
                <v-form @submit.prevent="handleLogin" ref="formRef">
                    <v-text-field v-model="username"
                                  label="Username"
                                  prepend-icon="mdi-account"
                                  required></v-text-field>

                    <v-text-field v-model="password"
                                  label="Password"
                                  type="password"
                                  prepend-icon="mdi-lock"
                                  required></v-text-field>

                    <v-btn :loading="loginStore.loading"
                           type="submit"
                           color="primary"
                           class="mt-3"
                           block>
                        Login
                    </v-btn>
                    <v-btn @click="handleForgotPassword" class="mt-5" block>Forgot Password?</v-btn>
                    <v-alert v-if="loginStore.error"
                             type="error"
                             class="mt-3"
                             dense>
                        {{ loginStore.error }}
                    </v-alert>
                </v-form>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script lang="ts" setup>
    import { ref } from 'vue';
    import { useRouter } from 'vue-router';
    import { useLoginStore } from '../stores/loginStore';

    const username = ref('');
    const password = ref('');
    const loginStore = useLoginStore();
    const router = useRouter();

    const handleLogin = async () => {
        await loginStore.login(username.value, password.value);

        if (loginStore.isLoggedIn) {
            router.push('/home'); // Navigate on successful login
        }
    };

    const handleForgotPassword = () => {
        router.push('/forgot-password');
    };
</script>