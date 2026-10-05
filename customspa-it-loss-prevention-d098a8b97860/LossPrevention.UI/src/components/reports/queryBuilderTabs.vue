<template>
  <div class="query-builder-page">
    <!-- Compact Header Row with Toolbar -->
    <div class="page-header">
      <h3 class="page-title">Query Builder</h3>
      <div v-if="tabs.length === 0" class="empty-hint">No reports yet. Click Add Tab to create one.</div>
      <v-spacer />
      
      <!-- Inline Toolbar -->
      <div class="inline-toolbar">
        <Toolbar
          :tabs="tabs"
          :tabCounter="tabCounter"
          :isSaved="isSaved"
          :canShowHeatmap="canShowHeatmap"
          @save="saveWorkspace"
          @copy-tab="({ index }: { index: number }) => duplicateSelectedTabFromIndex(index)"
          @heatmap-drill="handleHeatmapDrill"
          @switch-tab="handleSwitchTab"
        />
      </div>
      
      <v-switch 
        v-if="currentTab" 
        v-model="currentTab.designMode" 
        :label="designModeLabel" 
        hide-details
        density="compact"
        color="success"
        class="mode-switch compact-switch"
      />
      <v-btn size="small" @click="promptNewTab" color="primary" variant="tonal">
        <v-icon size="small" class="mr-1">mdi-plus</v-icon>
        Add Tab
      </v-btn>
    </div>

    <v-tabs v-model="selectedTab" show-arrows class="custom-tabs">
      <v-tab
        v-for="tab in tabs"
        :key="tab.id"
        :value="tab.id"
        class="tab-item"
      >
        <div class="tab-label">{{ tab.title }}</div>
        <div class="tab-actions">
          <v-btn icon size="x-small" @click.stop="promptEditTab(tab)">
            <v-icon icon="mdi-pencil" />
          </v-btn>
          <v-btn icon size="x-small" @click.stop="removeTab(tab.id)">
            <v-icon icon="mdi-close" />
          </v-btn>
        </div>
      </v-tab>
    </v-tabs>

    <v-window v-model="selectedTab">
      <v-window-item v-for="tab in tabs" :key="tab.id" :value="tab.id">
        <div class="tab-content">
          <div class="section-card" v-if="tab.designMode">
            <selectFields :tab="tab" :field-options="fieldOptions" :reset-field-props="resetFieldProps" :get-aggregation-options="getAggregationOptions" :loading="mappingsStore.loading" />
          </div>

          <div class="section-card" v-if="tab.designMode">
            <ConditionGroup :group="tab.query" @update="val => updateTabQuery(tab.id, val)" />
          </div>

          <div class="section-card" v-if="tab.designMode">
            <FormattingRow v-model:group="tab.query.formattingRules" />
          </div>

          <div class="section-card section-card--results">
            <ResultsGrid :tab="tab" />
          </div>
        </div>
      </v-window-item>
    </v-window>

    <v-dialog v-model="dialog" persistent max-width="400px">
      <v-card>
        <v-card-title class="dialog-title">{{ editMode ? 'Edit Report Details' : 'Enter Report Details' }}</v-card-title>
        <v-card-text>
          <v-text-field v-model="newTabName" label="Report Name" required />
          <v-text-field v-model="newTabDescription" label="Report Description" />
        </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="editMode ? applyTabRename() : createNewTab()" :disabled="!newTabName">
          {{ editMode ? 'Save' : 'Create' }}
        </v-btn>
        <v-btn text="true" @click="dialog = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- AI Report Generator Dialog -->
  <v-dialog v-model="reportGeneratorDialog" max-width="600px">
    <v-card>
      <v-card-title>AI Report Generator</v-card-title>
      <v-card-text>
        <v-textarea
          v-model="fraudQuestion"
          label="Fraud Detection Question"
          rows="3"
          outlined
        ></v-textarea>
        <v-textarea
          v-model="nqlQuestion"
          label="NQL Query (Optional)"
          rows="3"
          outlined
        ></v-textarea>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="executeNQLQuery">Generate Report</v-btn>
        <v-btn text="true" @click="reportGeneratorDialog = false">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import selectFields from './selectFields.vue'
import ConditionGroup from './conditionGroup.vue'
import FormattingRow from './formattingRow.vue'
import ResultsGrid from './resultsGrid.vue'
import { useMappingStore } from '@/stores/mappingStore'
import { useWorkspaceStore } from '@/stores/workspaceStore'
import { Tab } from '@/interfaces/tab'
import Toolbar from './toolbar.vue'
import { useToast } from 'vue-toast-notification'
import { onBeforeRouteLeave } from 'vue-router'

const initializing = ref(true)
const mappingsStore = useMappingStore()
const workspaceStore = useWorkspaceStore()
const activeWorkspace = computed(() => workspaceStore.activeWorkspace)
const isSaved = ref(true)
const toast = useToast()

const selectedTab = ref('tab-1')
const tabCounter = ref(1)
const tabs = ref<Tab[]>([{
  id: 'tab-1',
  title: 'Query 1',
  description: '',
  selectedFields: [],
  groupByField: '',
  query: { type: 'AND', conditions: [], id: '' },
  designMode: true
}])

const currentTab = computed(() => tabs.value.find((t: Tab) => t.id === selectedTab.value))
const designModeLabel = computed(() =>
  currentTab.value?.designMode ? 'Design View' : 'Report View'
)

// Optimized: Create a Set for numeric data types for faster lookup
const NUMERIC_TYPES = new Set(['integer', 'double', 'decimal', 'float', 'long'])

const canShowHeatmap = computed(() => {
  const selectedFields = currentTab.value?.selectedFields || []
  if (!selectedFields.length) return false

  let hasTime = false
  let hasNumeric = false

  // Single pass through selected fields
  for (const field of selectedFields) {
    const mapping = mappingsStore.mappings.find(m => m.name === field.name || m.alias === field.alias)
    if (!mapping) continue

    const dataType = mapping.dataType?.toLowerCase()
    if (dataType?.includes('date')) hasTime = true
    if (NUMERIC_TYPES.has(dataType)) hasNumeric = true

    if (hasTime && hasNumeric) return true // Early exit
  }

  return false
})

const dialog = ref(false)
const newTabName = ref('')
const newTabDescription = ref('')
const editMode = ref(false)
const tabBeingEdited = ref<Tab | null>(null)

const fieldOptions = computed(() => {
  return mappingsStore.mappings.map((m: any) => ({
    Id: m._id || m.id,
    Name: m.name,
    Alias: m.alias,
    DataType: m.dataType,
    IsCalculated: m.isCalculated,
    Expression: m.expression,
    GroupBy: false,
    Aggregation: '',
    IsArray: m.isArray || false,
    IsLookup: m.isLookup || false,
    CollectionName: m.collectionName || 'Mappings',
    LongestLength: m.longestLength || null
  }))
})

// Cached numeric field names for faster aggregation lookup
const numericFieldNames = computed(() =>
  new Set(mappingsStore.mappings
    .filter(m => m.dataType === 'Integer' || m.dataType === 'Decimal')
    .map(m => m.name))
)

onMounted(async () => {
  await Promise.all([
    workspaceStore.fetchWorkspaceFromLocalStorageById(),
    mappingsStore.fetchMappings()
  ])

  if (activeWorkspace.value?.tabs) {
    // Create mapping lookup for faster access
    const mappingLookup = new Map(
      mappingsStore.mappings.map(m => [m.name, m])
    )

    tabs.value = activeWorkspace.value.tabs.map((tab: any, index: number) => {
      const selectedFields = (tab.selectedFields ?? []).map((field: any) => {
        const mapping = mappingLookup.get(field.name) || 
                       mappingsStore.mappings.find(m => m.alias === field.alias) || 
                       {}
        
        return {
          name: field.name || (mapping as any).name || '',
          alias: field.alias || (mapping as any).alias || '',
          dataType: field.dataType || (mapping as any).dataType || '',
          groupBy: field.groupBy ?? false,
          aggregation: field.aggregation || '',
          isCalculated: field.isCalculated ?? false,
          expression: field.expression || '',
          prefix: field.prefix || '',
          suffix: field.suffix || '',
          visible: field.visible ?? true // Default to visible if not specified
        }
      })

      return {
        id: `tab-${index + 1}`,
        title: tab.title ?? `Query ${index + 1}`,
        description: tab.description ?? '',
        selectedFields,
        groupByField: tab.groupByField ?? '',
        query: tab.query ?? { type: 'AND', conditions: [], id: '' },
        designMode: tab.designMode ?? true
      }
    })

    tabCounter.value = tabs.value.length
    selectedTab.value = tabs.value.length ? tabs.value[0].id : ''
  }

  await nextTick()
  isSaved.value = true
  initializing.value = false
})

onBeforeRouteLeave((_to, _from, next) => {
  if (!isSaved.value) {
    const confirmed = confirm("You have unsaved changes. Are you sure you want to leave?")
    next(confirmed)
  } else {
    next()
  }
})

watch(
  tabs,
  () => {
    if (initializing.value) return
    isSaved.value = false
  },
  { deep: true }
)

function promptNewTab() {
  editMode.value = false
  tabBeingEdited.value = null
  newTabName.value = ''
  newTabDescription.value = ''
  dialog.value = true
}

function promptEditTab(tab: Tab) {
  editMode.value = true
  tabBeingEdited.value = tab
  newTabName.value = tab.title
  newTabDescription.value = tab.description || ''
  dialog.value = true
}

function createNewTab() {
  tabCounter.value++
  const newId = `tab-${tabCounter.value}`
  tabs.value.push({
    id: newId,
    title: newTabName.value,
    description: newTabDescription.value,
    selectedFields: [],
    groupByField: '',
    query: { type: 'AND', conditions: [], id: '' },
    designMode: true
  })
  selectedTab.value = newId
  dialog.value = false
  isSaved.value = false
}

function applyTabRename() {
  if (tabBeingEdited.value) {
    tabBeingEdited.value.title = newTabName.value
    tabBeingEdited.value.description = newTabDescription.value
    isSaved.value = false
  }
  dialog.value = false
  tabBeingEdited.value = null
}

function removeTab(id: string) {
  const index = tabs.value.findIndex(t => t.id === id)
  if (index === -1) return

  const isCurrentTab = selectedTab.value === id
  tabs.value.splice(index, 1)
  
  if (isCurrentTab && tabs.value.length) {
    selectedTab.value = tabs.value[Math.max(0, index - 1)].id
  }
}

function updateTabQuery(id: string, newGroup: any) {
  const tab = tabs.value.find(t => t.id === id)
  if (tab) tab.query = JSON.parse(JSON.stringify(newGroup))
}

function duplicateSelectedTabFromIndex(index: number) {
  if (index < 0 || index >= tabs.value.length) return
  
  const src = tabs.value[index]
  const clone = JSON.parse(JSON.stringify(src))
  
  tabCounter.value++
  clone.id = `tab-${tabCounter.value}`
  clone.title = src.title ? `${src.title} (Copy)` : `Query ${tabCounter.value}`
  
  tabs.value.splice(index + 1, 0, clone)
  selectedTab.value = clone.id
  isSaved.value = false
}

/* ---------- Drillthrough support ---------- */
const makeId = (prefix = 'id') => `${prefix}-${Math.random().toString(36).slice(2, 9)}`

function appendConditionsToRoot(group: any, conds: any[]) {
  if (!group || typeof group !== 'object') {
    return { id: makeId('group'), type: 'AND', conditions: [...conds] }
  }
  
  if (!Array.isArray(group.conditions)) group.conditions = []
  group.conditions.push(...conds)
  group.type = group.type || 'AND'
  group.id = group.id || makeId('group')
  
  return group
}

/** Duplicate current tab and add filters from heatmap drill (already ISO-minute formatted) */
function handleHeatmapDrill({ index, payload }: { index: number, payload: any }) {
  if (index < 0 || index >= tabs.value.length) return
  
  const src = tabs.value[index]
  const clone = JSON.parse(JSON.stringify(src))

  tabCounter.value++
  clone.id = `tab-${tabCounter.value}`
  clone.title = src.title ? `${src.title} (Drilldown)` : `Query ${tabCounter.value}`

  // payload.filters contains values already normalized to "YYYY-MM-DDTHH:mm" when date-like
  const conds = (payload?.filters ?? []).map((f: any) => ({ id: makeId('cond'), ...f }))
  clone.query = appendConditionsToRoot(clone.query, conds)

  tabs.value.splice(index + 1, 0, clone)
  selectedTab.value = clone.id
  isSaved.value = false
}

/* ---------- Misc helpers ---------- */
function getAggregationOptions(fieldName: string) {
  if (!fieldName) return []
  return numericFieldNames.value.has(fieldName)
    ? [
        { title: 'Sum', value: 'sum' },
        { title: 'Avg', value: 'avg' },
        { title: 'Min', value: 'min' },
        { title: 'Max', value: 'max' }
      ]
    : [{ title: 'Count', value: 'count' }]
}

function resetFieldProps(field: { groupBy: boolean; aggregation: string }) {
  field.groupBy = false
  field.aggregation = ''
}

const normalize = (str: string) => str?.toLowerCase()

function saveWorkspace() {
  if (!activeWorkspace.value) return

  const updatedTabs = tabs.value.map(tab => {
    // Create mapping lookup for this iteration
    const mappingByName = new Map(
      mappingsStore.mappings.map(m => [normalize(m.name), m])
    )

    const selectedFields = tab.selectedFields.map(field => {
      const mapping = mappingByName.get(normalize(field.name)) || {}
      
      return {
        Name: field.name || '',
        Alias: field.alias || (mapping as any).alias || '',
        DataType: field.dataType || (mapping as any).dataType || '',
        GroupBy: field.groupBy ?? false,
        Aggregation: field.aggregation || '',
        IsCalculated: field.isCalculated ?? false,
        Expression: field.expression || '',
        Prefix: field.prefix || '',
        Suffix: field.suffix || '',
        Visible: field.visible ?? true // Save visibility state
      }
    })

    return {
      id: tab.id,
      title: tab.title,
      description: tab.description,
      groupByField: tab.groupByField,
      query: tab.query,
      designMode: tab.designMode,
      selectedFields
    }
  })

  workspaceStore.updateWorkspace(activeWorkspace.value.id, {
    id: activeWorkspace.value.id,
    name: activeWorkspace.value.name,
    description: activeWorkspace.value.description,
    tabs: updatedTabs
  })

  toast.success('Workspace Saved Successfully', { position: 'top-right', duration: 3000 })
  isSaved.value = true
}

// AI functionality refs
const nqlQuestion = ref('')
const fraudQuestion = ref('')
const reportGeneratorDialog = ref(false)

// NQL Query execution
const executeNQLQuery = async () => {
  if (!nqlQuestion.value.trim()) {
    toast.warning('Please enter a question', { position: 'top-right', duration: 3000 })
    return
  }

  // TODO: Implement AI store integration and query generation
  toast.info('AI Report Generator coming soon', { position: 'top-right', duration: 3000 })
}

function handleSwitchTab(tabId: string) {
  selectedTab.value = tabId
  isSaved.value = false
}
</script>

<style scoped>
/* Full width page container */
.query-builder-page {
  width: 100%;
  max-width: 100%;
}

.custom-tabs {
  display: flex;
  flex-wrap: nowrap;
  overflow-x: auto;
  white-space: nowrap;
  min-height: 32px;
}

.custom-tabs :deep(.v-slide-group__content) {
  gap: 2px;
}

/* Page Header */
.page-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 0;
  margin-bottom: 4px;
  border-bottom: 1px solid #eee;
}

/* Inline Toolbar */
.inline-toolbar {
  display: flex;
  align-items: center;
  gap: 2px;
  margin-right: 8px;
}

.inline-toolbar :deep(.v-btn) {
  width: 28px;
  height: 28px;
  min-width: 28px;
}

.inline-toolbar :deep(.v-btn .v-icon) {
  font-size: 16px;
}

.inline-toolbar :deep(.mr-2) {
  margin-right: 2px !important;
}

.page-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0;
}

.empty-hint {
  font-size: 12px;
  color: #888;
}

.mode-switch {
  margin-right: 8px;
}

.mode-switch :deep(.v-label) {
  font-size: 11px;
}

/* Compact switch toggle - match GRP style */
.compact-switch {
  flex: 0 0 auto !important;
}

.compact-switch :deep(.v-input__control) {
  flex: 0 0 auto;
}

.compact-switch :deep(.v-switch__track) {
  width: 32px;
  height: 16px;
  border-radius: 8px;
}

.compact-switch :deep(.v-switch__thumb) {
  width: 12px;
  height: 12px;
}

.compact-switch :deep(.v-selection-control) {
  min-height: 22px;
  flex: 0 0 auto;
}

.compact-switch :deep(.v-selection-control__wrapper) {
  height: 16px;
  width: 32px;
}

/* Tab Content */
.tab-content {
  padding: 8px 0;
  width: 100%;
}

.section-card {
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 12px;
  margin-bottom: 8px;
  width: 100%;
}

.section-card--results {
  /* Results section uses all available width */
  max-width: 100%;
}

/* Dialog */
.dialog-title {
  font-size: 14px;
  font-weight: 600;
}

/* Tabs */
.custom-tabs {
  display: flex;
  flex-wrap: nowrap;
  overflow-x: auto;
  white-space: nowrap;
  min-height: 32px;
  border-bottom: 1px solid #ddd;
}

.custom-tabs :deep(.v-slide-group__content) {
  gap: 2px;
}

.v-tab.tab-item {
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 10px;
  min-height: 28px;
  height: 28px;
  position: relative;
  border: 1px solid #ddd;
  border-bottom: none;
  border-radius: 4px 4px 0 0;
  background-color: #f5f5f5;
  margin-right: 2px;
  transition: background-color 0.15s ease;
  white-space: nowrap;
  overflow: hidden;
  text-transform: none;
  letter-spacing: normal;
  font-size: 12px;
  font-weight: 500;
}

.v-tab.tab-item.v-tab--selected {
  background-color: #fff;
  border-color: #ccc;
  font-weight: 600;
}

.v-tab.tab-item:hover {
  background-color: #eee;
}

.v-tab.tab-item .tab-label {
  flex-grow: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 80px;
  max-width: 160px;
  margin-right: 6px;
  font-size: 12px;
  line-height: 1.2;
}

.v-tab.tab-item .tab-actions {
  display: flex;
  gap: 2px;
  visibility: hidden;
}

.v-tab.tab-item .tab-actions :deep(.v-btn) {
  width: 18px;
  height: 18px;
}

.v-tab.tab-item .tab-actions :deep(.v-icon) {
  font-size: 12px;
}

.v-tab.tab-item:hover .tab-actions {
  visibility: visible;
}

.v-chip .v-icon {
  margin-inline-end: 4px;
}
</style>