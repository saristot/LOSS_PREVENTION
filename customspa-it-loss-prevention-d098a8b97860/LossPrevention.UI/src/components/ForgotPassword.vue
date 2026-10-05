<template>
    <v-container class="d-flex justify-center align-center" style="height: 40vh;">
        <v-card width="400" elevation="4">
            <v-card-title class="text-h6 text-center">Forgot Password</v-card-title>
            <v-card-text>
                <v-form @submit.prevent="handleForgotPassword" ref="formRef">
                    <v-text-field v-model="usernameOrEmail"
                                  label="Username or Email"
                                  prepend-icon="mdi-account"
                                  :rules="[rules.required]"
                                  required></v-text-field>

                    <v-btn :loading="passwordResetStore.loading"
                           type="submit"
                           color="primary"
                           class="mt-3"
                           block>
                        Send Reset Email
                    </v-btn>

                    <v-btn @click="goToLogin"
                           variant="text"
                           class="mt-2"
                           block>
                        Back to Login
                    </v-btn>

                    <v-alert v-if="passwordResetStore.success"
                             type="success"
                             class="mt-3"
                             dense>
                        {{ passwordResetStore.successMessage }}
                    </v-alert>

                    <v-alert v-if="passwordResetStore.error"
                             type="error"
                             class="mt-3"
                             dense>
                        {{ passwordResetStore.error }}
                    </v-alert>
                </v-form>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script lang="ts" setup>import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { usePasswordResetStore } from '../stores/passwordResetStore';

const usernameOrEmail = ref('');
const passwordResetStore = usePasswordResetStore();
const router = useRouter();
const formRef = ref();

const rules = {
  required: (value: string) => !!value || 'This field is required',
};

const handleForgotPassword = async () => {
  const result = await formRef.value?.validate();
  if (!result?.valid) return;

  await passwordResetStore.requestPasswordReset(usernameOrEmail.value);
};

const goToLogin = () => {
  passwordResetStore.clearMessages();
  router.push('/');
};</script>