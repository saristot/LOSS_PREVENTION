<template>
    <div class="home-container">
        <div class="welcome-section">
            <h1>Welcome to Loss Prevention</h1>
            <p class="date">{{ currentDate }}</p>
        </div>

        <div class="content-grid">
            <div class="panel">
                <div class="panel-header">
                    <h2>Recent Dashboards</h2>
                    <button class="view-all-btn" @click="goToDashboardsList">View All</button>
                </div>
                <div v-if="dashboardsLoading" class="loading-indicator">Loading dashboards...</div>
                <ul v-else class="item-list">
                    <li v-for="dashboard in dashboards" :key="dashboard.id" 
                        class="list-item clickable" 
                        @click="navigateToDashboard(dashboard.id)">
                        <span class="item-name">{{ dashboard.name }}</span>
                        <span class="item-date">{{ formatDate(dashboard.lastModified || new Date()) }}</span>
                    </li>
                    <li v-if="dashboards.length === 0" class="empty-list">
                        No dashboards found.
                    </li>
                </ul>
            </div>

            <div class="panel">
                <div class="panel-header">
                    <h2>Recent Workspaces</h2>
                    <button class="view-all-btn" @click="goToWorkspaces">View All</button>
                </div>
                <div v-if="workspacesLoading" class="loading-indicator">Loading workspaces...</div>
                <ul v-else class="item-list">
                    <li v-for="workspace in workspaces" :key="workspace.id" 
                        class="list-item clickable"
                        @click="navigateToWorkspace(workspace)">
                        <span class="item-name">{{ workspace.name }}</span>
                        <span class="item-date">{{ formatDate(new Date()) }}</span>
                    </li>
                    <li v-if="workspaces.length === 0" class="empty-list">
                        No workspaces found.
                    </li>
                </ul>
            </div>

            <div class="panel notifications-panel">
                <div class="panel-header">
                    <h2>Alerts & Notifications</h2>
                    <button class="view-all-btn" @click="goToNotifications">View All</button>
                </div>
                <div v-if="notificationsLoading" class="loading-indicator">Loading notifications...</div>
                <ul v-else class="item-list">
                    <li v-for="notification in latestNotifications" :key="notification.id" 
                        class="list-item notification-item"
                        :class="{ 'unread': !notification.isRead }">
                        <div class="notification-content">
                            <span class="notification-title">{{ notification.title }}</span>
                            <span class="notification-message">{{ notification.message }}</span>
                        </div>
                        <div class="notification-actions">
                            <span class="item-date">{{ formatDate(new Date(notification.createdAt)) }}</span>
                            <button v-if="!notification.isRead" 
                                    class="mark-read-btn" 
                                    @click.stop="markAsRead(notification.id)"
                                    title="Mark as read">
                                ✓
                            </button>
                            <button class="delete-btn" 
                                    @click.stop="deleteNotification(notification.id)"
                                    title="Delete">
                                ✕
                            </button>
                        </div>
                    </li>
                    <li v-if="latestNotifications.length === 0" class="empty-list">
                        No notifications at this time.
                    </li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useDashboardStore } from '@/stores/dashboardStore';
import { useWorkspaceStore } from '@/stores/workspaceStore';
import { useNotificationStore } from '@/stores/notificationStore';
import type { Workspace } from '@/interfaces/workspace';

const router = useRouter();
const dashboardStore = useDashboardStore();
const workspaceStore = useWorkspaceStore();
const notificationStore = useNotificationStore();

// State
const dashboardsLoading = ref(false);
const workspacesLoading = ref(false);
const notificationsLoading = ref(false);
const dashboards = ref<any[]>([]);
const workspaces = computed(() => workspaceStore.workspaces);
const latestNotifications = computed(() => notificationStore.latestNotifications);

// Computed properties
const currentDate = computed(() => {
    return new Date().toLocaleDateString('en-GB', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
});

// Methods
const formatDate = (date: Date) => {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

const navigateToDashboard = (id: string) => {
    router.push({ name: 'dashboard-detail', params: { id } });
};

const navigateToWorkspace = (workspace: Workspace) => {
    workspaceStore.setActiveWorkspace(workspace);
    router.push({ name: 'query' });
};

const navigateToWorkspaces = () => {
    router.push({ name: 'workspaces' });
};

// Navigation
const goToDashboardsList = () => {
    router.push({ name: 'dashboards-list' });
};

const goToWorkspaces = () => {
    router.push({ name: 'workspaces' });
};

const goToNotifications = () => {
    router.push({ name: 'manage-notifications' });
};

// Notification actions
const markAsRead = async (notificationId: string) => {
    try {
        await notificationStore.markAsRead(notificationId);
    } catch (error) {
        console.error('Error marking notification as read:', error);
    }
};

const deleteNotification = async (notificationId: string) => {
    try {
        await notificationStore.deleteNotification(notificationId);
    } catch (error) {
        console.error('Error deleting notification:', error);
    }
};

// Fetch data
const fetchDashboards = async () => {
    dashboardsLoading.value = true;
    try {
        dashboards.value = await dashboardStore.list();
        // Sort by most recently created/modified first
        dashboards.value.sort((a, b) => {
            const dateA = a.lastModified ? new Date(a.lastModified) : new Date();
            const dateB = b.lastModified ? new Date(b.lastModified) : new Date();
            return dateB.getTime() - dateA.getTime();
        });
        // Limit to 5 most recent
        if (dashboards.value.length > 5) {
            dashboards.value = dashboards.value.slice(0, 5);
        }
    } catch (error) {
        console.error('Error fetching dashboards:', error);
    } finally {
        dashboardsLoading.value = false;
    }
};

const fetchWorkspaces = async () => {
    workspacesLoading.value = true;
    try {
        await workspaceStore.fetchWorkspaces();
        // Sort workspaces by name (since we don't have last accessed timestamp)
        workspaceStore.workspaces.sort((a, b) => a.name.localeCompare(b.name));
    } catch (error) {
        console.error('Error fetching workspaces:', error);
    } finally {
        workspacesLoading.value = false;
    }
};

const fetchNotifications = async () => {
    notificationsLoading.value = true;
    try {
        await notificationStore.fetchNotifications();
    } catch (error) {
        console.error('Error fetching notifications:', error);
    } finally {
        notificationsLoading.value = false;
    }
};

// Lifecycle hooks
onMounted(async () => {
    await Promise.all([fetchDashboards(), fetchWorkspaces(), fetchNotifications()]);
});
</script>

<style scoped>
.home-container {
    padding: 2rem;
    max-width: 1200px;
    margin: 0 auto;
}

.welcome-section {
    text-align: center;
    margin-bottom: 2rem;
}

.welcome-section h1 {
    color: #333;
    font-size: 2.5rem;
    margin-bottom: 0.5rem;
}

.date {
    color: #666;
    font-size: 1.1rem;
}

.content-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
}

.notifications-panel {
    grid-column: 1 / -1;
}

.panel {
    background: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    border: 1px solid #e1e5e9;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
    border-bottom: 2px solid #007bff;
    padding-bottom: 0.5rem;
}

.panel h2 {
    color: #333;
    margin: 0;
    font-size: 1.25rem;
}

.view-all-btn {
    background: transparent;
    color: #007bff;
    border: 1px solid #007bff;
    border-radius: 4px;
    padding: 0.25rem 0.75rem;
    font-size: 0.85rem;
    cursor: pointer;
    transition: all 0.2s;
}

.view-all-btn:hover {
    background: #007bff;
    color: white;
}

.item-list {
    list-style: none;
    padding: 0;
    margin: 0;
}

.list-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 0;
    border-bottom: 1px solid #f0f0f0;
}

.list-item:last-child {
    border-bottom: none;
}

.item-name {
    font-weight: 500;
    color: #333;
}

.item-date {
    color: #666;
    font-size: 0.875rem;
}

.notification-item {
    align-items: flex-start;
}

.notification-item.unread {
    background-color: #f0f8ff;
    border-left: 3px solid #007bff;
    padding-left: 0.5rem;
}

.notification-content {
    display: flex;
    flex-direction: column;
    flex: 1;
}

.notification-title {
    font-weight: 600;
    color: #333;
    margin-bottom: 0.25rem;
}

.notification-message {
    color: #666;
    font-size: 0.875rem;
}

.notification-actions {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.mark-read-btn,
.delete-btn {
    background: transparent;
    border: 1px solid #ccc;
    border-radius: 4px;
    width: 24px;
    height: 24px;
    cursor: pointer;
    font-size: 0.875rem;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.mark-read-btn {
    color: #28a745;
    border-color: #28a745;
}

.mark-read-btn:hover {
    background: #28a745;
    color: white;
}

.delete-btn {
    color: #dc3545;
    border-color: #dc3545;
}

.delete-btn:hover {
    background: #dc3545;
    color: white;
}

.clickable {
    cursor: pointer;
    transition: background-color 0.2s;
}

.clickable:hover {
    background-color: #f5f9ff;
}

.loading-indicator {
    text-align: center;
    padding: 1rem;
    color: #666;
    font-style: italic;
}

.empty-list {
    text-align: center;
    padding: 1rem;
    color: #666;
    font-style: italic;
}

.create-link {
    color: #007bff;
    cursor: pointer;
    text-decoration: underline;
}

.create-link:hover {
    color: #0056b3;
}

@media (max-width: 768px) {
    .content-grid {
        grid-template-columns: 1fr;
    }
    
    .notifications-panel {
        grid-column: 1;
    }
}
</style>