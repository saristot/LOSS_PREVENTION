<template>
  <v-app>
    <v-app-bar app color="primary" dark v-if="loginStore.isLoggedIn">
      <v-toolbar-title style="max-width: none;">Welcome {{ username }}</v-toolbar-title>
      <v-spacer />

      <!-- Nav -->
      <v-btn icon class="mx-1" color="white" @click="home">
        <v-icon>mdi-home</v-icon>
      </v-btn>

      <v-btn class="mx-1" color="white" variant="outlined" @click="workspaces">Workspaces</v-btn>
      <v-btn class="mx-1" color="white" variant="outlined" @click="dashboardsList">Dashboards</v-btn>
      <v-btn class="mx-1" color="white" variant="outlined" @click="logout">Logout</v-btn>

      <!-- Notifications -->
      <v-menu offset-y :close-on-content-click="false" v-model="notifMenu">
        <template #activator="{ props }">
          <v-btn icon v-bind="props" :aria-label="'Notifications'" class="mx-2">
            <v-badge :content="unreadCount" :value="unreadCount > 0" color="red" overlap>
              <v-icon>mdi-bell</v-icon>
            </v-badge>
          </v-btn>
        </template>

        <v-card style="min-width: 360px" elevation="6">
          <v-card-title class="d-flex align-center justify-space-between">
            <span class="text-subtitle-1">Notifications</span>
            <div class="d-flex align-center" style="gap:.25rem">
              <v-btn size="small" variant="text" :disabled="unreadCount === 0" @click="markAllRead">
                Mark all read
              </v-btn>
              <v-btn size="small" variant="text" @click="goManageNotifications">
                Manage Notifications
              </v-btn>
            </div>
          </v-card-title>

          <v-divider />

          <v-list density="compact" v-if="unreadNotificationsWithTimeLabels.length">
            <v-list-item v-for="n in unreadNotificationsWithTimeLabels" :key="n.id" :title="n.title" :subtitle="n.timeLabel"
              :class="[{ 'opacity-70': n.read }]">
              <template #prepend>
                <v-icon :color="n.read ? 'grey' : 'yellow-darken-2'">
                  {{ n.icon || 'mdi-bell-ring' }}
                </v-icon>
              </template>

              <template #append>
                <div class="d-flex align-center" style="gap:.25rem">
                  <v-btn size="x-small" variant="text" @click="openNotification(n)">Open</v-btn>
                  <v-btn size="x-small" variant="text" @click="markRead(n.id)">Read</v-btn>
                  <v-btn size="x-small" variant="text" color="red" @click="dismiss(n.id)">Dismiss</v-btn>
                </div>
              </template>

              <div class="text-body-2 mt-1">{{ n.message }}</div>
            </v-list-item>
          </v-list>

          <div v-else class="pa-6 text-medium-emphasis text-center">
            <p class="mb-3">No unread notifications</p>
            <v-btn size="small" variant="outlined" @click="goManageNotifications(); notifMenu = false">
              View All Notifications
            </v-btn>
          </div>
        </v-card>
      </v-menu>

      <!-- Settings -->
      <v-menu offset-y>
        <template #activator="{ props }">
          <v-btn icon v-bind="props"><v-icon>mdi-cog</v-icon></v-btn>
        </template>
        <v-list>
          <v-list-item v-if="canManageUsers" @click="manageUsers">
            <v-list-item-title>Manage Users</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canViewPermissions" @click="managePermissions">
            <v-list-item-title>Manage Permissions</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canViewRoles" @click="manageRoles">
            <v-list-item-title>Manage Roles</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canManageGroups" @click="manageGroups">
            <v-list-item-title>Manage Groups</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canManageNotifications" @click="goManageNotifications">
            <v-list-item-title>Manage Notifications</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canManageMappings" @click="manageMappings">
            <v-list-item-title>Manage Field Mappings</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canManageRules" @click="manageRules">
            <v-list-item-title>Manage Rules</v-list-item-title>
          </v-list-item>
          <v-list-item v-if="canManageDataIngestion" @click="manageDataIngestion">
            <v-list-item-title>Manage Data Ingestion</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>


    </v-app-bar>

    <v-main>
      <v-container fluid class="pa-4" style="max-width: 100%;">
        <router-view />
      </v-container>
    </v-main>

    <!-- Global snackbar for toasts -->
    <v-snackbar v-model="snackbar.show" :timeout="snackbar.timeout" :color="snackbar.color" location="bottom right">
      {{ snackbar.text }}
      <template #actions>
        <v-btn variant="text" @click="snackbar.show = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<script lang="ts" setup>
import { ref, computed, provide, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useLoginStore } from './stores/loginStore';
import { useNotificationStore } from './stores/notificationStore';

const router = useRouter();
const loginStore = useLoginStore();
const notificationStore = useNotificationStore();

// Track if this is an initial load from storage (no welcome toast)
const isInitialLoad = ref(true);
loginStore.initializeFromStorage();
isInitialLoad.value = false;

const username = ref(localStorage.getItem('username') ?? '');

const logout = () => {
  loginStore.logout();
  router.push('/');
};

const home = () => router.push('/home');
const workspaces = () => router.push('/workspaces');
const dashboardsList = () => router.push('/dashboardsList');
//const queryBuilder = () => router.push('/query');

function manageUsers() { router.push('/manage/users'); }
function managePermissions() { router.push('/manage/permissions'); }
function manageRoles() { router.push('/manage/roles'); }
function manageGroups() { router.push('/manage/groups'); }
function manageMappings() { router.push('/manage/mappings'); }
function manageRules() { router.push('/manage/rules'); }
function goManageNotifications() { router.push('/manage/notifications'); }
function manageDataIngestion() { router.push('/manage/dataingestion'); }

/* ---------------------- Notifications / Alerts ---------------------- */
// Use Pinia notification store
const notifMenu = ref(false);

// Load notifications from backend
onMounted(async () => {
  if (loginStore.isLoggedIn && notificationStore.canManageNotifications) {
    await notificationStore.fetchNotifications();
  }
});

// Watch for login state changes and load notifications
watch(() => loginStore.isLoggedIn, async (newVal) => {
  if (newVal && notificationStore.canManageNotifications) {
    await notificationStore.fetchNotifications();
  }
});

const unreadCount = computed(() => notificationStore.unreadCount);

// Map store notifications to local format for display
const notifications = computed(() => 
  notificationStore.notifications.map(n => ({
    id: n.id,
    title: n.title || 'Notification',
    message: n.message,
    route: undefined,
    read: n.isRead,
    createdAt: new Date(n.createdAt).getTime(),
    icon: n.type === 'alert' ? 'mdi-alert' : n.type === 'reply' ? 'mdi-reply' : 'mdi-bell-ring'
  }))
);

// Add computed property for notifications with time labels
const notificationsWithTimeLabels = computed(() => 
  notifications.value.map(n => ({
    ...n,
    timeLabel: fmtTime(n.createdAt)
  }))
);

// Add computed property for unread notifications with time labels
const unreadNotificationsWithTimeLabels = computed(() => 
  notifications.value
    .filter(n => !n.read)
    .map(n => ({
      ...n,
      timeLabel: fmtTime(n.createdAt)
    }))
);

function fmtTime(ts: number) {
  const d = new Date(ts);
  return d.toLocaleString();
}

function pushNotification(n: { title: string; message: string; route?: string; icon?: string }) {
  // For local notifications (like welcome message), just show toast
  toast(`${n.title}: ${n.message}`);
}

async function markRead(id: string) {
  await notificationStore.markAsRead(id);
}

async function dismiss(id: string) {
  await notificationStore.deleteNotification(id);
}

async function markAllRead() {
  await notificationStore.markAllAsRead();
}

function openNotification(n: any) {
  notificationStore.markAsRead(n.id);
  notifMenu.value = false;
  router.push('/manage/notifications');
}

// computed helper to expose a label without mutating
// const timeLabelFor = (n: Notif) => fmtTime(n.createdAt);
// expose on objects for template
// Object.defineProperty(Object.prototype, 'timeLabel', { get() { return '' }, configurable: true }); // noop to satisfy TS in template
// safer: map in template via computed; but for simplicity, add below:
const _ = new Proxy({}, { get: () => undefined }); // no-op

/* attach timeLabel at render */
/* ------------------------ Global Snackbar -------------------------- */
const snackbar = ref({ show: false, text: '', color: 'primary', timeout: 3500 });
function toast(text: string, color = 'primary', timeout = 3500) {
  snackbar.value.text = text;
  snackbar.value.color = color;
  snackbar.value.timeout = timeout;
  snackbar.value.show = true;
}

// Provide toast function to all child components
provide('toast', toast);

// Watch notification store's snackbar and sync to local snackbar
watch(() => notificationStore.snackbar, (storeSnackbar) => {
  if (storeSnackbar.show) {
    toast(storeSnackbar.message, storeSnackbar.color, 3500);
    notificationStore.hideNotification();
  }
}, { deep: true });

/* -------------- Show welcome alert only on actual login ------------- */
watch(() => loginStore.isLoggedIn, (newVal, oldVal) => {
  // Only show welcome toast when transitioning from logged out to logged in
  // AND it's not the initial page load from storage
  if (newVal && !oldVal && !isInitialLoad.value) {
    username.value = localStorage.getItem('username') ?? '';
    pushNotification({
      title: 'Welcome',
      message: `Hello ${username.value}!`,
      route: '/dashboard',
      icon: 'mdi-hand-wave'
    });
  }
});

/* ---------------------- Permission Checking ---------------------- */
function hasPermission(permission: string): boolean {
  try {
    const token = localStorage.getItem('token');
    if (!token) {
      console.log('No token found');
      return false;
    }
    
    // Decode JWT token (simple base64 decode for payload)
    const payload = JSON.parse(atob(token.split('.')[1]));
    console.log('JWT Payload:', payload);
    
    // Check different possible permission property names
    const permissions = payload.permissions || payload.Permission || payload.Permissions || payload.scope || payload.scopes || [];
    console.log('Permissions found:', permissions);
    console.log('Checking for permission:', permission);
    
    // Handle both array and string formats
    if (Array.isArray(permissions)) {
      return permissions.includes(permission);
    } else if (typeof permissions === 'string') {
      return permissions.split(' ').includes(permission);
    }
    
    return false;
  } catch (error) {
    console.error('Error checking permissions:', error);
    return false;
  }
}

// Computed properties for menu item visibility
const canManageUsers = computed(() => {
  const result = hasPermission('CAN_ADD_USERS') || 
    hasPermission('CAN_DELETE_USERS') || 
    hasPermission('CAN_UPDATE_USERS') || 
    hasPermission('CAN_VIEW_USER') || 
    hasPermission('CAN_CREATE_USER') || 
    hasPermission('CAN_UPDATE_USER');
  console.log('canManageUsers:', result);
  return result;
});

const canViewPermissions = computed(() => {
  const result = hasPermission('CAN_ADD_USERS') || hasPermission('CAN_UPDATE_USERS');
  console.log('canViewPermissions:', result);
  return result;
});

const canViewRoles = computed(() => {
  const result = hasPermission('CAN_ADD_USERS') || hasPermission('CAN_UPDATE_USERS');
  console.log('canViewRoles:', result);
  return result;
});

const canManageGroups = computed(() => {
  const result = hasPermission('CAN_ADD_USERS') || 
    hasPermission('CAN_UPDATE_USERS') || 
    hasPermission('CAN_MANAGE_GROUPS');
  console.log('canManageGroups:', result);
  return result;
});

const canManageNotifications = computed(() => {
  const result = hasPermission('CAN_MANAGE_NOTIFICATIONS');
  console.log('canManageNotifications:', result);
  return result;
});

const canManageMappings = computed(() => {
  const result = hasPermission('CAN_UPDATE_MAPPINGS') || 
    hasPermission('CAN_CREATE_MAPPINGS') || 
    hasPermission('CAN_DELETE_MAPPINGS');
  console.log('canManageMappings:', result);
  return result;
});

const canManageRules = computed(() => {
  const result = hasPermission('CAN_UPDATE_MAPPINGS');
  console.log('canManageRules:', result);
  return result;
});

const canManageDataIngestion = computed(() => {
  const result = hasPermission('CAN_CREATE_TRANSACTION');
  console.log('canManageDataIngestion:', result);
  return result;
});

const canManageAlerts = computed(() => {
  const result = hasPermission('CAN_VIEW_DASHBOARD');
  console.log('canManageAlerts:', result);
  return result;
});

const canManageDashboards = computed(() => 
  hasPermission('CAN_MANAGE_DASHBOARDS')
);

// Helper to show divider only if there are management items above it
const showAnyManagementItem = computed(() => 
  canManageUsers.value || 
  canViewPermissions.value || 
  canViewRoles.value || 
  canManageMappings.value || 
  canManageRules.value || 
  canManageDataIngestion.value
);
</script>

<style scoped>
/* keep spacing tight on the app bar buttons */
.v-app-bar .v-btn {
  text-transform: none;
}
</style>
