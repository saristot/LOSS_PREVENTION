import { createApp } from "vue";
import { createPinia } from "pinia";
import "@mdi/font/css/materialdesignicons.css";
import App from "./App.vue";
import router from './router';
import vuetify from "./plugins/vuetify"; // Import the Vuetify plugin you will create
import ToastPlugin from 'vue-toast-notification';
import 'vue-toast-notification/dist/theme-bootstrap.css'
import './style.css'; // Global styles

// AG Grid module registration
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
ModuleRegistry.registerModules([AllCommunityModule]);

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(vuetify); // Use Vuetify
app.use(ToastPlugin); // Use Toast notifications
app.mount("#app");
