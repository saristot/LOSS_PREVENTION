// /src/stores/loginStore.ts
import { reactive, toRefs } from 'vue';
import { defineStore } from 'pinia';
import api from '../api/api';

// --- helpers (no external deps) ---
function parseJwt(token: string): any | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

function getExpiryMs(token: string): number | null {
  const payload = parseJwt(token);
  const exp = payload?.exp;
  return typeof exp === 'number' ? exp * 1000 : null;
}

function isExpired(token: string, skewSeconds = 30): boolean {
  const expMs = getExpiryMs(token);
  if (!expMs) return true;
  return expMs <= Date.now() + skewSeconds * 1000;
}

export const useLoginStore = defineStore('login', () => {
  const state = reactive({
    username: '',
    token: '',
    loading: false,
    error: null as string | null,
    isLoggedIn: false,
  });

  // internal: manage a scheduled logout when token expires
  let expiryTimer: number | undefined;

  function clearExpiryTimer() {
    if (expiryTimer) {
      clearTimeout(expiryTimer);
      expiryTimer = undefined;
    }
  }

  function scheduleExpiryLogout(token: string) {
    clearExpiryTimer();
    const expMs = getExpiryMs(token);
    if (!expMs) return; // malformed token, let validateToken handle it elsewhere
    const skewMs = 20_000; // 20s buffer
    const delay = Math.max(0, expMs - Date.now() - skewMs);

    // Window.setTimeout returns number in browsers
    expiryTimer = window.setTimeout(() => {
      // token considered expired -> logout
      logout();
    }, delay);
  }

  async function login(username: string, password: string) {
    state.loading = true;
    state.error = null;

    try {
      const response = await api.post('/users/login', {
        username,
        password,
      });

      const token = response.data?.token?.result as string | undefined;

      if (token && typeof token === 'string') {
        if (isExpired(token)) {
          state.error = 'Your session token is already expired or invalid.';
          // ensure clean state
          logout();
          return;
        }

        state.token = token;
        state.username = username;
        state.isLoggedIn = true;

        localStorage.setItem('token', token);
        localStorage.setItem('username', username);

        scheduleExpiryLogout(token); // <— auto logout when it expires
      } else {
        state.error = 'Invalid login response. Token missing.';
      }
    } catch (error: any) {
      state.error = error?.response?.data?.message || 'Login error';
    } finally {
      state.loading = false;
    }
  }

  function logout() {
    state.username = '';
    state.token = '';
    state.isLoggedIn = false;
    state.error = null;

    clearExpiryTimer();

    localStorage.removeItem('token');
    localStorage.removeItem('username');
  }

  function initializeFromStorage() {
    const token = localStorage.getItem('token') || '';
    const username = localStorage.getItem('username') || '';

    if (token && username && !isExpired(token)) {
      state.token = token;
      state.username = username;
      state.isLoggedIn = true;
      scheduleExpiryLogout(token);
    } else {
      // stale/expired storage -> clear it
      logout();
    }
  }

  /** Optional helper for router guards: returns true if token is present & valid, else logs out and returns false */
  function validateToken(): boolean {
    if (!state.token || isExpired(state.token)) {
      logout();
      return false;
    }
    return true;
  }

  return {
    ...toRefs(state),
    login,
    logout,
    initializeFromStorage,
    // new helper you can call from router guard if you want
    validateToken,
  };
});
