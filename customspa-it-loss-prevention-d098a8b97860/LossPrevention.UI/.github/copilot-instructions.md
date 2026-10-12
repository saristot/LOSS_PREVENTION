# LossPrevention UI - AI Coding Agent Instructions

## Project Overview
Vue 3 + Vite + TypeScript application for retail loss prevention analytics, featuring dynamic query building, fraud detection, dashboards, and Euclidean distance analysis. Backend communicates via MongoDB aggregation pipelines.

## Architecture

### Core Stack
- **Frontend**: Vue 3 Composition API (`<script setup>`), Vuetify 3, TypeScript
- **State**: Pinia stores (composition API style with `defineStore`)
- **Router**: Vue Router 4 with auth guards
- **Data Grid**: Tabulator Tables 6.x (virtual scrolling for 1000+ records)
- **API**: Axios with JWT bearer auth via interceptors

### Key Data Flow
1. User builds queries in `queryBuilderTabs.vue` → generates MongoDB aggregation pipeline
2. Pipeline sent to backend via `reportDataStore.fetchReportData(pipeline, pageSize, skip)`
3. Results displayed in `resultsGrid.vue` with Tabulator + virtual DOM
4. AI fraud analysis processes paginated results, stores metadata in `fraudDetectionStore`

## Critical Patterns

### MongoDB Pipeline Construction
**Location**: `src/components/reports/resultsGrid.vue` (submitQuery), `src/helpers/queryUtils.ts`

Pipelines follow this order for performance:
```javascript
1. $match (pre-unwind) - filter early using indexes
2. $unwind - expand arrays (e.g., LineItem, TENDER)
3. $match (post-unwind) - filter on unwound fields
4. $sort - sort filtered results
5. $project - map fields to aliases for frontend
```

**Array Unwinding Logic**:
- Check `IsArray`/`isArray` flag on mappings
- Unwind root fields (e.g., `LineItem`) before accessing nested properties
- After unwind, reference fields with full dotted paths: `LineItem.Barcode`
- Prefix naming convention: `LineItemBarcode` → converted to `LineItem.Barcode` in projection

**Type Coercion** (see `queryUtils.ts`):
- `Decimal128`: Use `{$numberDecimal: "123.45"}` for eq/ne, plain numbers for ranges
- Dates: Convert to `{$date: "2024-01-01T00:00:00Z"}`
- Boolean fields: Stored as `true`/`false`, not strings

### Tabulator Performance Optimization
**Location**: `src/components/reports/resultsGrid.vue` (initializeEmptyTable)

For 1000 records:
```javascript
{
  virtualDom: true,
  virtualDomBuffer: 500, // Increased for smooth scrolling
  renderVertical: 'virtual',
  renderHorizontal: 'virtual',
  progressiveLoad: 'scroll',
  progressiveLoadDelay: 20,
  height: 500 // Fixed height required for virtual scrolling
}
```

**Large Dataset Handling** (>500 rows):
- Show loading overlay during render (`showLoadingOverlay`)
- Use `requestAnimationFrame` batching for columns/data/highlighting
- Apply fraud highlights in separate frame to avoid blocking

### Fraud Detection Modal Pattern
**Location**: `src/components/reports/resultsGrid.vue` (analyzeFraud)

**Critical**: Close modal BEFORE reloading page to prevent UI overlap:
```javascript
showFraudModal.value = false;
shouldUpdateLoadTime.value = false; // Don't show load time during fraud reload
await loadPage(currentPage.value);
```

**Load Time Display**: Only show `{{ duration }} ms` for user-initiated actions:
- Set `shouldUpdateLoadTime = true` in: submitQuery, previousPage, nextPage, changePageSize
- Set `shouldUpdateLoadTime = false` in: fraud analysis reloads, clearFraudAnalysis, filterFraudRows

### Field Mapping & Aliases
**Location**: `src/stores/mappingStore.ts`, used throughout query builders

Mappings define:
- `name`/`Name`: MongoDB field path (e.g., `LineItem.Barcode`)
- `alias`/`Alias`: Display name for UI (e.g., "Item Code")
- `DataType`/`dataType`: Type for coercion (Decimal128, Int32, Date, Boolean, etc.)
- `IsArray`/`isArray`: Determines if unwinding needed

**Always check both Pascal and camelCase** (`m.Name ?? m.name`) due to backend inconsistencies.

### Pinia Store Conventions
**Setup-style stores** (composition API):
- `workspaceStore`, `loginStore`, `passwordResetStore`
- Use `ref()` for state, `computed()` for getters, regular functions for actions

**Options-style stores**:
- `reportDataStore`, `mappingStore`, `userStore`, etc.
- Use `state()`, `getters`, `actions` properties

All stores accessed via `useStoreNameStore()` in components.

## Component Structure

### Query Builder (`src/components/reports/queryBuilderTabs.vue`)
- Manages workspace tabs with queries, selected fields, conditions, formatting rules
- Child components: `selectFields.vue`, `conditionGroup.vue`, `toolbar.vue`, `resultsGrid.vue`
- Tabs stored in `workspaceStore`, persisted to backend
- Virtual scrolling in `selectFields.vue` for 100+ fields

### Results Grid (`src/components/reports/resultsGrid.vue`)
1982 lines - central data display component
- Context menu: Add conditions, formatting rules, Euclidean distance, export (CSV/PDF/Excel)
- Fraud detection: Multi-page analysis with progress modal, highlighting by severity
- Pagination: Server-side via skip/take, client-side fraud filtering

### Dashboard System (`src/components/dashboard/`)
- Grid layout using `vue-grid-layout-v3`
- Block types: `textblock`, `imageblock`, `chartblock`, `tabularBlock`
- `tabularBlock.vue` reuses query pipeline logic from results grid
- Blocks reference workspace tabs by `tabId`

## Development Commands

```bash
npm run dev      # Start dev server (Vite)
npm run build    # Production build
npm run preview  # Preview production build
```

**Environment**: Set `VITE_API_BASE_URL` in `.env` (e.g., `http://localhost:5000`)

## Common Tasks

### Adding a New Field Type
1. Update backend mapping with correct `DataType`
2. Add type check in `queryUtils.ts` (e.g., `isDecimalType`)
3. Add coercion in `buildTypeAwareCondition` and projection logic
4. Update `createFormattedColumns` for display formatting

### Creating New Store
```typescript
// src/stores/myStore.ts
import { defineStore } from 'pinia'
export const useMyStore = defineStore('myStore', {
  state: () => ({ items: [] }),
  actions: {
    async fetchItems() { /* ... */ }
  }
})
```

### Adding Context Menu Action
Update `contextMenu` in `resultsGrid.vue`:
```javascript
menu.push({
  label: 'My Action',
  action: (e, cell) => handleMyAction(cell)
});
```

## Known Patterns to Avoid

❌ **Don't** use `&&` in PowerShell - use `;` for command chaining  
❌ **Don't** mutate Tabulator data directly - use `replaceData()`  
❌ **Don't** assume field names match casing - always check both  
❌ **Don't** show load time during fraud analysis internal reloads  
❌ **Don't** forget to close modals before triggering page reloads  

## Testing Fraud Detection
1. Run query with 100+ records
2. Click "Detect Potential Fraud" - modal should display progress
3. Modal closes automatically when complete
4. Grid reloads with colored highlights (red=high, orange=medium, yellow=low)
5. Hover over highlighted rows for fraud details tooltip

## Key Files Reference

- **Query building**: `src/components/reports/queryBuilderTabs.vue`, `src/helpers/queryUtils.ts`
- **Results display**: `src/components/reports/resultsGrid.vue`
- **Pipeline generation**: `submitQuery()` in resultsGrid, tabularBlock
- **Type mapping**: `src/stores/mappingStore.ts`
- **API config**: `src/api/api.ts` (JWT interceptor)
- **Auth**: `src/router/index.ts` (beforeEach guard), `src/stores/loginStore.ts`
- **Distance analysis**: `src/components/reports/distanceDialog.vue`, `src/components/reports/distanceTable.vue`

## Documentation

See `docs/` for feature-specific guides:
- `DISTANCE_ANALYSIS_QUICK_START.md` - Euclidean distance feature
- `FRAUD_DETECTION_API.md` - Fraud detection endpoints and thresholds
- `roadmap.txt` - Planned features
