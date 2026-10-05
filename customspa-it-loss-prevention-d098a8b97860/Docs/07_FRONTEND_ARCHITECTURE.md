# Frontend Architecture Documentation

## Table of Contents
1. [Technology Stack](#technology-stack)
2. [Project Structure](#project-structure)
3. [Component Architecture](#component-architecture)
4. [Complete Component List](#complete-component-list)
5. [State Management](#state-management)
6. [Routing Configuration](#routing-configuration)
7. [API Integration](#api-integration)
8. [Authentication Flow](#authentication-flow)
9. [Key UI Features](#key-ui-features)
10. [Build and Deployment](#build-and-deployment)
11. [Styling and Theming](#styling-and-theming)
12. [Performance Optimizations](#performance-optimizations)

---

## Technology Stack

### Core Framework
- **Vue.js 3.5.12** - Progressive JavaScript framework with Composition API
- **Nuxt 3** - Meta-framework for server-side rendering and modern web applications
- **Vite 6.3.5** - Next-generation frontend build tool for fast development

### UI Components & Styling
- **Vuetify 3.7.3** - Material Design component framework
- **Material Design Icons (@mdi/font 7.4.47)** - Icon library
- **Custom CSS** - Application-specific styling

### State Management & Routing
- **Pinia 2.2.4** - Vue 3 official state management library
- **Vue Router 4.6.3** - Official router for Vue.js

### Data Visualization & Tables
- **Chart.js 4.5.0** - JavaScript charting library
- **chartjs-chart-matrix 3.0.0** - Heatmap/matrix chart plugin
- **AG Grid Community 35.0.0** - Enterprise-grade data grid
- **Tabulator Tables 6.3.1** - Interactive table library

### Data Processing & Export
- **Axios 1.9.0** - HTTP client for API requests
- **PapaParse 5.5.3** - CSV parser
- **XLSX 0.18.5** - Excel file processing
- **jsPDF 3.0.1** - PDF generation
- **jsPDF-autotable 5.0.2** - Table support for PDF
- **pdfmake 0.2.20** - Alternative PDF creation library
- **JSON5 2.2.3** - JSON with comments and more lenient syntax

### Layout & Interactions
- **grid-layout-plus 1.1.0** - Draggable grid layout system
- **vue-grid-layout-v3 3.1.2** - Vue 3 grid layout component
- **vuedraggable 4.1.0** - Drag and drop functionality

### Notifications
- **vue-toast-notification 3.1.3** - Toast notification system

### Development Tools
- **TypeScript 5.0.0** - Type-safe JavaScript
- **@nuxt/devtools 1.6.0** - Development tools for Nuxt
- **Vue TSC 1.2.0** - TypeScript support for Vue

---

## Project Structure

```
LossPrevention.UI/
├── public/                      # Static assets
│   └── vite.svg                # Favicon
├── src/
│   ├── api/                    # API configuration
│   │   └── api.ts             # Axios instance with interceptors
│   ├── components/             # Vue components
│   │   ├── ai/                # AI/NLP components
│   │   ├── dashboard/         # Dashboard system components
│   │   ├── manage/            # Administration components
│   │   ├── reports/           # Query builder & reporting
│   │   ├── workspace/         # Workspace management
│   │   ├── Login.vue          # Authentication
│   │   ├── ForgotPassword.vue
│   │   ├── ResetPassword.vue
│   │   └── home.vue           # Landing page
│   ├── helpers/               # Utility functions
│   │   ├── fieldLock.ts      # Field-level security
│   │   ├── queryUtils.ts     # Query building utilities
│   │   └── tabUtils.ts       # Tab management helpers
│   ├── interfaces/            # TypeScript interfaces
│   │   ├── distance.ts       # Distance analysis types
│   │   ├── ruletypes.ts      # Rule configuration types
│   │   ├── tab.ts            # Query tab interfaces
│   │   ├── user.ts           # User model
│   │   └── workspace.ts      # Workspace model
│   ├── plugins/               # Vue plugins
│   │   └── vuetify.ts        # Vuetify configuration
│   ├── router/                # Vue Router
│   │   └── index.ts          # Route definitions & guards
│   ├── stores/                # Pinia stores (state management)
│   │   ├── aiStore.ts        # AI query generation
│   │   ├── dashboardStore.ts # Dashboard state
│   │   ├── dataIngestionStore.ts
│   │   ├── fraudDetectionStore.ts
│   │   ├── groupStore.ts
│   │   ├── loginStore.ts     # Authentication
│   │   ├── mappingStore.ts   # Field mappings
│   │   ├── notificationStore.ts
│   │   ├── PasswordResetStore.ts
│   │   ├── permissionStore.ts
│   │   ├── reportDataStore.ts # Query results
│   │   ├── rolestore.ts
│   │   ├── ruleStore.ts
│   │   ├── userStore.ts
│   │   └── workspacestore.ts
│   ├── App.vue                # Root component
│   ├── main.ts                # Application entry point
│   ├── shims-vue.d.ts         # TypeScript declarations
│   └── style.css              # Global styles
├── index.html                  # HTML entry point
├── nuxt.config.ts             # Nuxt configuration
├── package.json               # Dependencies
├── tsconfig.json              # TypeScript configuration
└── vite.config.js             # Vite build configuration
```

---

## Component Architecture

### Component Hierarchy

The application follows a hierarchical component structure:

```
App.vue (Root)
├── Navigation Bar (conditional on auth)
├── Notification Menu
├── Settings Menu
└── Router View
    ├── Login/Auth Pages
    ├── Home Dashboard
    ├── Workspace Management
    ├── Query Builder
    ├── Dashboard System
    └── Admin Management Pages
```

### Component Design Principles

1. **Single Responsibility** - Each component has a focused purpose
2. **Composition API** - Modern Vue 3 `<script setup>` syntax
3. **Type Safety** - TypeScript interfaces for props and emits
4. **Reusability** - Shared components across features
5. **Props Down, Events Up** - Unidirectional data flow

---

## Complete Component List

### Authentication Components

| Component | Path | Purpose |
|-----------|------|---------|
| Login | `src/components/Login.vue` | User authentication form |
| ForgotPassword | `src/components/ForgotPassword.vue` | Password reset request |
| ResetPassword | `src/components/ResetPassword.vue` | Password reset confirmation |

### Core Navigation Components

| Component | Path | Purpose |
|-----------|------|---------|
| App | `src/App.vue` | Root component with navigation bar, notification bell, settings menu |
| Home | `src/components/home.vue` | Landing page with recent dashboards, workspaces, and notifications |

### Dashboard System Components

| Component | Path | Purpose |
|-----------|------|---------|
| Dashboard | `src/components/dashboard/dashboard.vue` | Main dashboard container with edit/view modes |
| DashboardsList | `src/components/dashboard/dashboardsList.vue` | List all available dashboards |
| DashboardHeader | `src/components/dashboard/dashboardheader.vue` | Dashboard header with workspace/tab selector |
| DashboardEditorBar | `src/components/dashboard/dashboardEditorBar.vue` | Toolbar for design mode (save, revert, delete) |
| DashboardGrid | `src/components/dashboard/dashboardGrid.vue` | Draggable grid layout for dashboard blocks |
| ChartBlock | `src/components/dashboard/chartblock.vue` | Chart visualization block |
| ChartConfigDialog | `src/components/dashboard/chartConfigDialog.vue` | Configure chart type, data, colors |
| TextBlock | `src/components/dashboard/textblock.vue` | Rich text content block |
| TextConfigDialog | `src/components/dashboard/textConfigDialog.vue` | Edit text block content |
| ImageBlock | `src/components/dashboard/imageblock.vue` | Image display block |
| ImageConfigDialog | `src/components/dashboard/imageConfigDialog.vue` | Upload/configure images |
| TabularBlock | `src/components/dashboard/tabularBlock.vue` | Data table block |
| TabularDialog | `src/components/dashboard/tabularDialog.vue` | Configure table columns and data |

### Query Builder & Report Components

| Component | Path | Purpose |
|-----------|------|---------|
| QueryBuilderTabs | `src/components/reports/queryBuilderTabs.vue` | Multi-tab query builder interface |
| SelectFields | `src/components/reports/selectFields.vue` | Field selection with aggregations |
| ConditionGroup | `src/components/reports/conditionGroup.vue` | Nested query conditions (AND/OR/NOR) |
| ConditionRow | `src/components/reports/conditionRow.vue` | Individual query condition |
| FormattingRow | `src/components/reports/formattingRow.vue` | Conditional formatting rules |
| Toolbar | `src/components/reports/toolbar.vue` | Query actions (save, copy, export, heatmap) |
| ResultsGrid | `src/components/reports/resultsGrid.vue` | AG Grid for query results with pagination |
| DistanceDialog | `src/components/reports/distanceDialog.vue` | Configure distance/similarity analysis |
| DistanceTable | `src/components/reports/distanceTable.vue` | Display distance analysis results |
| RadarChart | `src/components/reports/radarChart.vue` | Radar chart for multi-dimensional comparison |
| HeatmapPreview | `src/components/reports/heatmapPreview.vue` | Heatmap visualization |
| FraudSettingsDialog | `src/components/reports/FraudSettingsDialog.vue` | Configure fraud detection thresholds |

### AI & Natural Language Components

| Component | Path | Purpose |
|-----------|------|---------|
| NaturalLanguageQuery | `src/components/ai/naturalLanguageQuery.vue` | AI-powered query generation from natural language |

### Workspace Management Components

| Component | Path | Purpose |
|-----------|------|---------|
| Workspace | `src/components/workspace/workspace.vue` | List, create, edit workspaces and their reports |

### Administration Components

| Component | Path | Purpose |
|-----------|------|---------|
| Users | `src/components/manage/users.vue` | User management (CRUD operations) |
| Roles | `src/components/manage/roles.vue` | Role management with permissions |
| Permissions | `src/components/manage/permissions.vue` | Permission catalog |
| Groups | `src/components/manage/groups.vue` | Group management for access control |
| Notifications | `src/components/manage/notifications.vue` | Notification configuration and history |
| Mappings | `src/components/manage/mappings.vue` | Field mapping configuration |
| Rules | `src/components/manage/rules.vue` | Business rule configuration |
| DataIngestion | `src/components/manage/dataingestion.vue` | Data import scheduling and configuration |
| ScheduleForm | `src/components/manage/ScheduleForm.vue` | Cron schedule configuration component |

---

## State Management

### Pinia Stores Overview

The application uses Pinia for centralized state management with 15 specialized stores:

| Store | File | Responsibility |
|-------|------|----------------|
| **loginStore** | `stores/loginStore.ts` | Authentication state, token management, auto-logout |
| **dashboardStore** | `stores/dashboardStore.ts` | Dashboard CRUD, blocks, dirty state tracking |
| **workspaceStore** | `stores/workspacestore.ts` | Workspace management, active workspace |
| **reportDataStore** | `stores/reportDataStore.ts` | Query execution, results caching, distance analysis |
| **aiStore** | `stores/aiStore.ts` | AI query generation with LLM integration (Qwen 2.5) |
| **fraudDetectionStore** | `stores/fraudDetectionStore.ts` | Fraud detection thresholds, analytics configuration |
| **userStore** | `stores/userStore.ts` | User CRUD operations |
| **roleStore** | `stores/rolestore.ts` | Role management |
| **permissionStore** | `stores/permissionStore.ts` | Permission catalog |
| **groupStore** | `stores/groupStore.ts` | Group management |
| **notificationStore** | `stores/notificationStore.ts` | Notifications, read/unread state |
| **mappingStore** | `stores/mappingStore.ts` | Field mappings, schema metadata |
| **ruleStore** | `stores/ruleStore.ts` | Business rules engine |
| **dataIngestionStore** | `stores/dataIngestionStore.ts` | Data import jobs, scheduling |
| **PasswordResetStore** | `stores/PasswordResetStore.ts` | Password reset flow |

### Store Architecture Pattern

All stores follow consistent patterns:

```typescript
// Example: loginStore.ts
export const useLoginStore = defineStore('login', () => {
  // State
  const state = reactive({
    username: '',
    token: '',
    loading: false,
    error: null,
    isLoggedIn: false
  })

  // Actions
  async function login(credentials) {
    state.loading = true
    // API call
    state.loading = false
  }

  // Getters (computed)
  const isAdmin = computed(() => {
    // derive from state
  })

  return {
    ...toRefs(state),
    login,
    isAdmin
  }
})
```

### Key Store Features

#### Authentication Store (`loginStore`)
- **JWT Token Management** - Parse, validate, and store JWT tokens
- **Auto-Logout** - Scheduled logout when token expires
- **Token Validation** - Check token on every route navigation
- **Permission Checks** - Role-based access control helpers

#### Dashboard Store (`dashboardStore`)
- **Dirty State Tracking** - Monitor unsaved changes
- **Auto-save** - Optional automatic saving
- **Block Management** - Add, edit, delete, reorder blocks
- **Snapshot System** - Revert to last saved state

#### Report Data Store (`reportDataStore`)
- **Query Execution** - Send MongoDB aggregation pipelines to API
- **Result Caching** - Store query results with metadata
- **Distance Analysis** - Similarity calculations between records
- **Performance Tracking** - Record query execution times

#### AI Store (`aiStore`)
- **LLM Integration** - Connect to local Ollama instance (Qwen 2.5 14B model)
- **Query Generation** - Natural language to MongoDB query pipeline
- **JSON Parsing** - Extract and clean LLM JSON responses
- **Fast Mode** - Optimized parameters for quick responses

#### Fraud Detection Store (`fraudDetectionStore`)
- **Threshold Configuration** - Manage detection sensitivity
- **Multiple Detection Types** - High value, low value, refunds, voids
- **Statistical Calculations** - Percentile, standard deviation, mean multipliers
- **Threshold Descriptions** - Document what each threshold detects

---

## Routing Configuration

### Route Structure

```typescript
// src/router/index.ts
const routes = [
  // Public routes
  { path: '/', name: 'login', component: LoginForm },
  { path: '/forgot-password', name: 'forgot-password', component: ForgotPassword },
  { path: '/reset-password', name: 'reset-password', component: ResetPassword },
  
  // Protected routes (requiresAuth: true)
  { path: '/home', name: 'home', component: Home, meta: { requiresAuth: true } },
  
  // Dashboards
  { path: '/dashboards', name: 'dashboards-list', component: DashboardsList, meta: { requiresAuth: true } },
  { path: '/dashboard/:id', name: 'dashboard-detail', component: Dashboard, props: true, meta: { requiresAuth: true } },
  { path: '/dashboard/new', name: 'dashboard-new', component: Dashboard, meta: { requiresAuth: true } },
  
  // Query Builder & Reports
  { path: '/query', name: 'query', component: QueryBuilderTabs, meta: { requiresAuth: true } },
  
  // Workspaces
  { path: '/workspaces', name: 'workspaces', component: WorkSpace, meta: { requiresAuth: true } },
  
  // Administration
  { path: '/manage/roles', name: 'manage-roles', component: Roles, meta: { requiresAuth: true } },
  { path: '/manage/permissions', name: 'manage-permissions', component: Permissions, meta: { requiresAuth: true } },
  { path: '/manage/users', name: 'manage-users', component: Users, meta: { requiresAuth: true } },
  { path: '/manage/groups', name: 'manage-groups', component: Groups, meta: { requiresAuth: true } },
  { path: '/manage/notifications', name: 'manage-notifications', component: Notifications, meta: { requiresAuth: true } },
  { path: '/manage/mappings', name: 'manage-mappings', component: Mappings, meta: { requiresAuth: true } },
  { path: '/manage/rules', name: 'manage-rules', component: Rules, meta: { requiresAuth: true } },
  { path: '/manage/dataingestion', name: 'manage-dataingestion', component: DataIngestion, meta: { requiresAuth: true } }
]
```

### Navigation Guards

```typescript
router.beforeEach((to, from, next) => {
  const loginStore = useLoginStore()
  
  // Validate token on every navigation
  loginStore.validateToken()
  
  // Redirect logged-in users away from login page
  if (to.name === 'login' && loginStore.isLoggedIn) {
    next({ name: 'home' })
  } 
  // Require authentication for protected routes
  else if (to.meta.requiresAuth && !loginStore.isLoggedIn) {
    next({ name: 'login' })
  } 
  else {
    next()
  }
})
```

### Route Features

- **Route-level Guards** - Authentication checks before navigation
- **Meta Properties** - `requiresAuth` flag for protected routes
- **Dynamic Routes** - Dashboard detail with `:id` parameter
- **Props Passing** - Automatic prop conversion for route params
- **Redirects** - Legacy route aliases for backwards compatibility

---

## API Integration

### API Client Configuration

```typescript
// src/api/api.ts
import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL
})

// Request Interceptor - Add JWT token to all requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}, (error) => {
  return Promise.reject(error)
})

export default api
```

### API Integration Patterns

#### Store-based API Calls
All API interactions go through Pinia stores:

```typescript
// Example from userStore
async fetchUsers() {
  this.loading = true
  this.error = null
  try {
    const res = await api.get('/users')
    this.users = res.data
  } catch (err) {
    this.error = err.message
  } finally {
    this.loading = false
  }
}
```

#### Common API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/auth/login` | POST | User authentication |
| `/auth/refresh` | POST | Refresh JWT token |
| `/auth/forgot-password` | POST | Request password reset |
| `/auth/reset-password` | POST | Confirm password reset |
| `/users` | GET | List all users |
| `/users/:id` | GET/PUT/DELETE | User CRUD operations |
| `/workspaces` | GET/POST | Workspace management |
| `/workspaces/:id` | GET/PUT/DELETE | Workspace operations |
| `/dashboards` | GET/POST | Dashboard list/create |
| `/dashboards/:id` | GET/PUT/DELETE | Dashboard CRUD |
| `/data/report/query` | POST | Execute MongoDB aggregation query |
| `/distance` | POST | Distance/similarity analysis |
| `/rules` | GET/POST | Business rules |
| `/mappings` | GET/POST | Field mappings |
| `/notifications` | GET/POST/PUT | Notification management |
| `/groups` | GET/POST/PUT/DELETE | Group management |
| `/roles` | GET/POST/PUT/DELETE | Role management |
| `/permissions` | GET | Permission catalog |
| `/dataingestion` | GET/POST/PUT/DELETE | Data import jobs |

#### Error Handling Pattern

```typescript
try {
  const res = await api.post('/endpoint', data)
  return res.data
} catch (err) {
  if (axios.isAxiosError(err)) {
    this.error = err.response?.data?.message || err.message
  } else {
    this.error = 'Operation failed'
  }
  throw err
}
```

#### Environment Configuration

API base URL is configured via Vite environment variables:

```
VITE_API_BASE_URL=http://localhost:5000/api
```

---

## Authentication Flow

### Login Process

```
┌─────────────┐
│   User      │
│ enters      │
│ credentials │
└──────┬──────┘
       │
       ▼
┌─────────────────────────┐
│  loginStore.login()     │
│  - POST /auth/login     │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  Receive JWT token      │
│  - Parse token          │
│  - Extract username     │
│  - Extract permissions  │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  Store token            │
│  - localStorage         │
│  - Store state          │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  Schedule auto-logout   │
│  - Calculate expiry     │
│  - Set timeout timer    │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│  Redirect to /home      │
└─────────────────────────┘
```

### Token Management

#### JWT Token Structure
```json
{
  "sub": "user123",
  "username": "john.doe",
  "permissions": ["read:data", "write:dashboard"],
  "roles": ["analyst"],
  "exp": 1708899600,
  "iat": 1708896000
}
```

#### Token Validation
- **On Route Navigation** - Check token validity before each route change
- **On App Load** - Restore session from localStorage
- **Expiry Check** - Validate token hasn't expired (with 30s skew buffer)
- **Auto-Logout** - Automatic logout when token expires

#### Permission Checks

```typescript
// In App.vue
const canManageUsers = computed(() => 
  loginStore.permissions.includes('user:write')
)

const canViewPermissions = computed(() => 
  loginStore.permissions.includes('permission:read')
)
```

### Password Reset Flow

```
User → Forgot Password Page
  ↓
Enter Email → POST /auth/forgot-password
  ↓
Email with reset link sent
  ↓
User clicks link → Reset Password Page
  ↓
Enter new password → POST /auth/reset-password
  ↓
Redirect to Login
```

### Logout Process

```typescript
function logout() {
  loginStore.logout()
  // Clears:
  // - localStorage token
  // - Store state
  // - Cancels expiry timer
  router.push({ name: 'login' })
}
```

---

## Key UI Features

### 1. Dashboard System

#### Design Mode vs. View Mode
- **Design Mode** - Drag & drop blocks, edit configurations, save changes
- **View Mode** - Display-only mode for end users

#### Block Types
1. **Chart Block** - Bar, line, pie, doughnut, heatmap, matrix charts
2. **Text Block** - Rich text content, markdown support
3. **Image Block** - Upload or URL-based images
4. **Tabular Block** - Data tables with filtering and sorting

#### Features
- **Grid Layout** - Responsive draggable grid with resizing
- **Workspace Integration** - Load data from workspace reports
- **Tab Support** - Multiple tabs per dashboard
- **Auto-save** - Optional automatic saving of changes
- **Dirty State Tracking** - Visual indicator for unsaved changes
- **Block Configuration** - Dialog-based editors for each block type
- **Real-time Updates** - Immediate preview of changes

### 2. Report Builder (Query Builder)

#### Multi-Tab Interface
- **Tab Management** - Add, edit, rename, delete, duplicate tabs
- **Tab State** - Each tab maintains its own query configuration

#### Field Selection
- **Dynamic Field List** - Auto-populated from field mappings
- **Aggregations** - Sum, Avg, Min, Max, Count, CountDistinct
- **Aliases** - Custom field names in results
- **Sorting** - Multiple field sort with ASC/DESC

#### Query Conditions
- **Nested Groups** - Hierarchical AND/OR/NOR logic
- **Multiple Conditions** - Field comparisons with operators:
  - Equals, Not Equals
  - Greater Than, Less Than, Between
  - Contains, Not Contains, Starts With, Ends With
  - In, Not In (list matching)
  - Is Null, Is Not Null
- **Dynamic Types** - Auto-detect field types (string, number, date, boolean)

#### Conditional Formatting
- **Color Rules** - Highlight cells based on conditions
- **Multiple Rules** - Stack rules with priority

#### Results Grid
- **AG Grid** - Enterprise data grid with:
  - Column sorting and filtering
  - Column reordering
  - Column resizing
  - Row selection
  - Pagination (client-side)
- **Export Options** - CSV, Excel, PDF
- **Copy to Clipboard** - Quick data sharing

#### Distance Analysis
- **Similarity Calculation** - Find similar records based on field values
- **Multi-field Comparison** - Compare across 3+ fields
- **Radar Chart Visualization** - Multi-dimensional similarity view
- **Date Range Filtering** - Limit search to specific time periods

#### Heatmap Visualization
- **Matrix View** - Color-coded grid of data
- **Drill-down** - Click cells to filter data

### 3. Workspace Management

#### Workspace Features
- **Create/Edit/Delete** - Full CRUD operations
- **Report Organization** - Group related queries into workspaces
- **Search** - Filter workspaces by name
- **Active Workspace** - Set current working context

#### Workspace Structure
```typescript
interface Workspace {
  id: string
  name: string
  description?: string
  tabs: Tab[]  // Array of report configurations
}
```

#### Tab/Report Structure
```typescript
interface Tab {
  id: string
  title: string
  description?: string
  selectedFields: FieldDefinition[]
  query: ConditionGroup
  formattingRules: FormattingRule[]
  designMode: boolean
}
```

### 4. Fraud Detection UI

#### Threshold Configuration
- **High Value Transactions** - Detect unusually large transactions
  - Percentile threshold
  - Standard deviation multiplier
  - Minimum value override
- **Extreme High Value** - Detect outlier transactions
  - Percentile + multiplier
- **Low Value Padding** - Detect unusual low-value patterns
  - Mean multiplier
  - Minimum multiplier
- **Refund Analysis** - Detect excessive refunds
  - Amount percentile
  - Count percentile
- **Void Analysis** - Detect void patterns

#### Fraud Detection Process
1. Configure thresholds in `FraudSettingsDialog`
2. Run analysis on query results
3. View flagged transactions in results grid
4. Export fraud reports

#### Fraud Indicators
- Color-coded cells in results grid
- Fraud flags in data
- Summary statistics

### 5. AI Query Interface

#### Natural Language Query Generation
- **Input** - Plain English question
  - Example: "Show me all transactions over $1000 from last month"
- **LLM Processing** - Local Ollama instance (Qwen 2.5 14B model)
  - Convert natural language to MongoDB aggregation pipeline
  - Extract field names from mappings
  - Generate appropriate operators and conditions
- **Query Preview** - Display generated query structure
- **Add to Builder** - Import generated query into Query Builder tab

#### AI Features
- **Fast Mode** - Optimized for quick responses
- **Strict Mode** - More accurate, slower processing
- **JSON Mode** - Deterministic JSON output from LLM
- **Error Handling** - Graceful fallback for invalid queries

#### AI Store Architecture
- **Multi-step Generation** - Break complex queries into steps
- **Field Validation** - Verify fields exist in mappings
- **Query Optimization** - Simplify generated pipelines
- **JSON Cleanup** - Extract and parse LLM responses

---

## Build and Deployment

### Development Mode

```bash
# Install dependencies
npm install

# Start development server
npm run dev
# Vite dev server runs on http://localhost:5173
```

Development features:
- **Hot Module Replacement (HMR)** - Instant updates without full reload
- **Source Maps** - Debug original TypeScript code
- **Vite Dev Server** - Fast startup and incremental compilation

### Production Build

```bash
# Build for production
npm run build

# Output directory: dist/
# - Minified JavaScript
# - Optimized CSS
# - Code splitting
# - Tree-shaking
# - Asset optimization
```

### Preview Production Build

```bash
npm run preview
# Preview production build locally
```

### Build Configuration

#### Vite Configuration (`vite.config.js`)
```javascript
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  }
})
```

#### TypeScript Configuration (`tsconfig.json`)
```json
{
  "compilerOptions": {
    "target": "esnext",
    "module": "esnext",
    "strict": true,
    "jsx": "preserve",
    "resolveJsonModule": true,
    "esModuleInterop": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    }
  }
}
```

#### Nuxt Configuration (`nuxt.config.ts`)
```typescript
export default defineNuxtConfig({
  ssr: true,  // Enable Server-Side Rendering
  buildModules: ['@pinia/nuxt', '@nuxtjs/vuetify'],
  css: ['vuetify/styles'],
  vuetify: {
    theme: {
      themes: {
        light: {
          primary: '#1976D2',
          secondary: '#424242',
          accent: '#82B1FF',
          error: '#FF5252',
          info: '#2196F3',
          success: '#4CAF50',
          warning: '#FB8C00'
        }
      }
    }
  }
})
```

### Environment Variables

Create `.env` file in root directory:

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5000/api

# Ollama AI Configuration (for AI query generation)
VITE_OLLAMA_URL=http://localhost:11434
VITE_OLLAMA_MODEL=qwen2.5:14b
```

### Deployment Options

#### Static Hosting (Netlify, Vercel)
```bash
npm run build
# Deploy dist/ folder
```

#### Docker Container
```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build
EXPOSE 5173
CMD ["npm", "run", "preview"]
```

#### Nginx Reverse Proxy
```nginx
server {
  listen 80;
  server_name app.example.com;
  
  root /var/www/app/dist;
  index index.html;
  
  location / {
    try_files $uri $uri/ /index.html;
  }
  
  location /api {
    proxy_pass http://localhost:5000;
  }
}
```

---

## Styling and Theming

### Vuetify Theme

```typescript
// src/plugins/vuetify.ts
export default createVuetify({
  theme: {
    themes: {
      light: {
        primary: '#1976D2',    // Blue
        secondary: '#424242',  // Dark Gray
        accent: '#82B1FF',     // Light Blue
        error: '#FF5252',      // Red
        info: '#2196F3',       // Blue
        success: '#4CAF50',    // Green
        warning: '#FB8C00'     // Orange
      }
    }
  }
})
```

### Global Styles

```css
/* src/style.css */
:root {
  font-family: Inter, system-ui, Avenir, Helvetica, Arial, sans-serif;
  line-height: 1.5;
  font-weight: 400;
}

body {
  margin: 0;
  min-width: 320px;
  min-height: 100vh;
}
```

### Component-level Styling

Most components use:
- **Scoped CSS** - `<style scoped>` to prevent style leakage
- **Vuetify Classes** - Utility classes for spacing, layout, typography
- **Material Design Icons** - `mdi-*` icon classes

### Vuetify Component Defaults

```typescript
// Compact density for all select/list components
defaults: {
  VSelect: {
    density: 'compact',
    menuProps: {
      contentClass: 'compact-menu'
    }
  },
  VList: {
    density: 'compact'
  },
  VListItem: {
    density: 'compact'
  }
}
```

### Responsive Design

- **Vuetify Grid System** - 12-column responsive grid with breakpoints
- **Breakpoints** - xs, sm, md, lg, xl
- **Mobile-first** - Base styles for mobile, enhanced for desktop

### Custom CSS Variables

Components can define local CSS variables for theming:

```css
.query-builder-page {
  --primary-color: #1976D2;
  --border-color: #e0e0e0;
  --spacing-unit: 8px;
}
```

---

## Performance Optimizations

### Code Splitting

Vite automatically splits code by:
- **Route-based** - Each route component is a separate chunk
- **Vendor Splitting** - Third-party libraries in separate bundle
- **Dynamic Imports** - Components loaded on demand

### Lazy Loading

```typescript
// Router with lazy-loaded components
const routes = [
  {
    path: '/dashboard/:id',
    component: () => import('./components/dashboard/dashboard.vue')
  }
]
```

### State Management Optimizations

#### Computed Properties
```typescript
// Efficient reactive computations
const filteredUsers = computed(() => {
  return userStore.users.filter(u => u.isActive)
})
```

#### Selective Reactivity
```typescript
// Use markRaw for non-reactive data
import { markRaw } from 'vue'
const chartInstance = markRaw(new Chart(...))
```

### Data Grid Performance

#### AG Grid Optimizations
- **Virtual Scrolling** - Render only visible rows
- **Row Pagination** - Client-side pagination for large datasets
- **Column Virtualization** - Render only visible columns
- **Immutable Data** - Use `getRowId` for efficient updates

```typescript
// AG Grid configuration
gridOptions = {
  rowModelType: 'clientSide',
  pagination: true,
  paginationPageSize: 50,
  animateRows: true,
  getRowId: (params) => params.data._id
}
```

### API Call Optimization

#### Debounced Search
```typescript
// Debounce search input
import { debounce } from 'lodash'

const onSearch = debounce(() => {
  workspaceStore.fetchWorkspaces(searchQuery.value)
}, 300)
```

#### Request Caching
```typescript
// Cache API responses in store
const cachedData = computed(() => reportDataStore.data)
```

#### Abort Controllers
```typescript
// Cancel pending requests
let abortController = new AbortController()

async function fetchData() {
  abortController.abort()
  abortController = new AbortController()
  
  await api.get('/data', {
    signal: abortController.signal
  })
}
```

### Asset Optimization

- **Image Compression** - Optimize images before upload
- **SVG Icons** - Use Material Design Icons (SVG) instead of image files
- **Font Subsetting** - Load only required font characters
- **Tree Shaking** - Remove unused Vuetify components

### Bundle Size Analysis

```bash
# Analyze bundle size
npm run build -- --report
```

### Rendering Performance

#### Virtual Scrolling
- Used in notification lists
- Used in AG Grid
- Used in large dropdown menus

#### Memo Optimization
```typescript
// Prevent unnecessary re-renders
import { toRaw } from 'vue'

const rawData = toRaw(complexObject)
```

#### Conditional Rendering
```typescript
// Use v-show for frequent toggles
<div v-show="isVisible">Content</div>

// Use v-if for rare toggles
<div v-if="isLoggedIn">Content</div>
```

### Network Performance

- **HTTP/2** - Multiplexed connections
- **Compression** - Gzip/Brotli compression on server
- **CDN** - Serve static assets from CDN
- **Preconnect** - DNS prefetch for API domain

```html
<link rel="preconnect" href="https://api.example.com">
```

### Monitoring & Profiling

```typescript
// Performance tracking in stores
const start = performance.now()
await api.post('/data', query)
const duration = performance.now() - start
console.log(`Query executed in ${duration}ms`)
```

---

## Additional Considerations

### Accessibility
- Semantic HTML elements
- ARIA labels on interactive elements
- Keyboard navigation support
- Color contrast compliance

### Security
- JWT token in localStorage (consider httpOnly cookies for production)
- CSRF protection via tokens
- Input validation on forms
- XSS prevention (Vue auto-escapes)
- Content Security Policy headers

### Browser Support
- Modern browsers (Chrome, Firefox, Safari, Edge)
- ES2020+ JavaScript features
- No IE11 support

### Internationalization (i18n)
- Currently English only
- Framework ready for vue-i18n integration

### Testing
- Component testing framework: Vitest (recommended)
- E2E testing: Cypress or Playwright (recommended)

---

## Related Documentation

- [02_SYSTEM_ARCHITECTURE.md](./02_SYSTEM_ARCHITECTURE.md) - Overall system design
- [03_SETUP_INSTALLATION.md](./03_SETUP_INSTALLATION.md) - Development environment setup
- [05_API_DOCUMENTATION.md](./05_API_DOCUMENTATION.md) - Backend API reference
- [DISTANCE_ANALYSIS_QUICK_START.md](./DISTANCE_ANALYSIS_QUICK_START.md) - Distance analysis feature

---

**Document Version:** 1.0  
**Last Updated:** February 12, 2026  
**Maintained By:** Loss Prevention Development Team
