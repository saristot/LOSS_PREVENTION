import { reactive, toRefs } from 'vue';
import { defineStore } from 'pinia';
import api from '../api/api';

export const usePasswordResetStore = defineStore('passwordReset', () => {
    const state = reactive({
        loading: false,
        error: null as string | null,
        success: false,
        successMessage: null as string | null,
    });

    async function requestPasswordReset(usernameOrEmail: string) {
        state.loading = true;
        state.error = null;
        state.success = false;
        state.successMessage = null;

        try {
            const response = await api.post('/users/forgot-password', {
                usernameOrEmail,
            });

            state.success = true;
            state.successMessage = response.data || 'Password reset email sent successfully. Please check your email.';
        } catch (error: any) {
            state.error = error?.response?.data || 'Failed to send password reset email. Please try again.';
            state.success = false;
        } finally {
            state.loading = false;
        }
    }

    async function validateResetToken(token: string): Promise<boolean> {
        state.loading = true;
        state.error = null;

        try {
            const response = await api.post('/users/validate-reset-token', {
                token,
            });

            return response.data === true;
        } catch (error: any) {
            state.error = 'Invalid or expired reset token.';
            return false;
        } finally {
            state.loading = false;
        }
    }

    async function resetPassword(token: string, newPassword: string) {
        state.loading = true;
        state.error = null;
        state.success = false;
        state.successMessage = null;

        try {
            const response = await api.post('/users/reset-password', {
                token,
                newPassword,
            });

            state.success = true;
            state.successMessage = response.data || 'Password reset successfully. You can now login with your new password.';
        } catch (error: any) {
            state.error = error?.response?.data || 'Failed to reset password. Please try again.';
            state.success = false;
        } finally {
            state.loading = false;
        }
    }

    function clearMessages() {
        state.error = null;
        state.success = false;
        state.successMessage = null;
    }

    return {
        ...toRefs(state),
        requestPasswordReset,
        validateResetToken,
        resetPassword,
        clearMessages,
    };
});