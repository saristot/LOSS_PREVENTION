<template>
  <v-container class="d-flex justify-center align-center" style="height: 50vh;">
    <v-card width="400" elevation="4">
      <v-card-title class="text-h6 text-center">Reset Password</v-card-title>
      <v-card-text>
        <v-progress-circular
          v-if="validatingToken"
          indeterminate
          color="primary"
          class="d-flex mx-auto"
        ></v-progress-circular>

        <div v-else-if="!tokenValid">
          <v-alert type="error" class="mb-3">
            Invalid or expired reset token. Please request a new password reset.
          </v-alert>
          <v-btn @click="goToForgotPassword" color="primary" block>
            Request New Reset
          </v-btn>
        </div>

        <v-form v-else @submit.prevent="handleResetPassword" ref="formRef">
          <v-text-field
            v-model="newPassword"
            label="New Password"
            type="password"
            prepend-icon="mdi-lock"
            :rules="[rules.required, rules.minLength]"
            required
          ></v-text-field>

          <v-text-field
            v-model="confirmPassword"
            label="Confirm Password"
            type="password"
            prepend-icon="mdi-lock-check"
            :rules="[rules.required, rules.passwordMatch]"
            required
          ></v-text-field>

          <v-btn
            :loading="passwordResetStore.loading"
            type="submit"
            color="primary"
            class="mt-3"
            block
          >
            Reset Password
          </v-btn>

          <v-btn
            @click="goToLogin"
            variant="text"
            class="mt-2"
            block
          >
            Back to Login
          </v-btn>

          <v-alert
            v-if="passwordResetStore.success"
            type="success"
            class="mt-3"
            dense
          >
            {{ passwordResetStore.successMessage }}
          </v-alert>

          <v-alert
            v-if="passwordResetStore.error"
            type="error"
            class="mt-3"
            dense
          >
            {{ passwordResetStore.error }}
          </v-alert>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { usePasswordResetStore } from '../stores/passwordResetStore';

const newPassword = ref('');
const confirmPassword = ref('');
const passwordResetStore = usePasswordResetStore();
const router = useRouter();
const route = useRoute();
const formRef = ref();

const token = ref('');
const tokenValid = ref(false);
const validatingToken = ref(true);

const rules = {
  required: (value: string) => !!value || 'This field is required',
  minLength: (value: string) => value.length >= 1 || 'Password must be at least 1 character',
  passwordMatch: (value: string) => value === newPassword.value || 'Passwords do not match',
};

onMounted(async () => {
  token.value = (route.query.token as string) || '';
  
  if (!token.value) {
    tokenValid.value = false;
    validatingToken.value = false;
    return;
  }

  tokenValid.value = await passwordResetStore.validateResetToken(token.value);
  validatingToken.value = false;
});

const handleResetPassword = async () => {
  const result = await formRef.value?.validate();
  if (!result?.valid) return;

  await passwordResetStore.resetPassword(token.value, newPassword.value);

  if (passwordResetStore.success) {
    // Redirect to login after 2 seconds
    setTimeout(() => {
      router.push('/');
    }, 2000);
  }
};

const goToLogin = () => {
  passwordResetStore.clearMessages();
  router.push('/');
};

const goToForgotPassword = () => {
  passwordResetStore.clearMessages();
  router.push('/forgot-password');
};
</script>