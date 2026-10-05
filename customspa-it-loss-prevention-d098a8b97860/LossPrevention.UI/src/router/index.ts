import { createRouter, createWebHistory, NavigationGuardNext, RouteLocationNormalized } from 'vue-router';
import LoginForm from '../components/Login.vue';
import Home from '../components/home.vue';
import Dashboard from '../components/dashboard/dashboard.vue';
import DashboardsList from '../components/dashboard/dashboardsList.vue';
import QueryBuilderTabs from '../components/reports/queryBuilderTabs.vue';
import WorkSpace from '../components/workspace/workspace.vue';
import Roles from '../components/manage/roles.vue';
import Permissions from '../components/manage/permissions.vue';
import Users from '../components/manage/users.vue';
import Groups from '../components/manage/groups.vue';
import Notifications from '../components/manage/notifications.vue';
import Mappings from '../components/manage/mappings.vue';
import Rules from '@/components/manage/rules.vue';
import DataIngestion from '@/components/manage/dataingestion.vue';
import ForgotPassword from '../components/ForgotPassword.vue';
import ResetPassword from '../components/ResetPassword.vue';

import { useLoginStore } from '../stores/loginStore';

const routes = [
    { path: '/', name: 'login', component: LoginForm },
    { path: '/forgot-password', name: 'forgot-password', component: ForgotPassword },
    { path: '/reset-password', name: 'reset-password', component: ResetPassword },
    { path: '/home', name: 'home', component: Home, meta: { requiresAuth: true } },

    // Dashboards
    { path: '/dashboards', name: 'dashboards-list', component: DashboardsList, meta: { requiresAuth: true } },
    // keep old path as alias so existing links keep working
    { path: '/dashboardsList', redirect: { name: 'dashboards-list' }, meta: { requiresAuth: true } },

    // Dashboard detail (by id)
    { path: '/dashboard/:id', name: 'dashboard-detail', component: Dashboard, props: true, meta: { requiresAuth: true } },

    // Optional: dashboard create
    { path: '/dashboard/new', name: 'dashboard-new', component: Dashboard, meta: { requiresAuth: true } },

    // Other routes
    { path: '/query', name: 'query', component: QueryBuilderTabs, meta: { requiresAuth: true } },
    { path: '/workspaces', name: 'workspaces', component: WorkSpace, meta: { requiresAuth: true } },
    { path: '/manage/roles', name: 'manage-roles', component: Roles, meta: { requiresAuth: true } },
    { path: '/manage/permissions', name: 'manage-permissions', component: Permissions, meta: { requiresAuth: true } },
    { path: '/manage/users', name: 'manage-users', component: Users, meta: { requiresAuth: true } },
    { path: '/manage/groups', name: 'manage-groups', component: Groups, meta: { requiresAuth: true } },
    { path: '/manage/notifications', name: 'manage-notifications', component: Notifications, meta: { requiresAuth: true } },
    { path: '/manage/mappings', name: 'manage-mappings', component: Mappings, meta: { requiresAuth: true } },
    { path: '/manage/rules', name: 'manage-rules', component: Rules, meta: { requiresAuth: true } },
    { path: '/manage/dataingestion', name: 'manage-dataingestion', component: DataIngestion, meta: { requiresAuth: true } }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

router.beforeEach((to: RouteLocationNormalized, _from: RouteLocationNormalized, next: NavigationGuardNext) => {
    const loginStore = useLoginStore();

    // Hard check on every nav; auto-logout if expired
    loginStore.validateToken();

    if (to.name === 'login' && loginStore.isLoggedIn) {
        // send logged-in users to the home page
        next({ name: 'home' });
    } else if (to.meta.requiresAuth && !loginStore.isLoggedIn) {
        next({ name: 'login' });
    } else {
        next();
    }
});

export default router;