<template>
  <div class="tabular-block border rounded">
    <div class="tabular-toolbar p-6 d-flex justify-end gap-2" v-if="inDesignMode">
      <v-btn icon size="small" @click="$emit('edit')"><v-icon>mdi-pencil</v-icon></v-btn>
      <v-btn icon size="small" @click="$emit('delete')"><v-icon>mdi-delete</v-icon></v-btn>
    </div>

    <h3 class="text-center">{{ title }}</h3>
    <p class="text-center text-subtitle-2">{{ description }}</p>

    <div class="my-4 d-flex align-center">
      <v-btn :disabled="currentPage <= 1" @click="previousPage">Previous</v-btn>
      <v-btn :disabled="currentPage >= totalPages" @click="nextPage">Next</v-btn>
      <span class="mx-4">Page {{ currentPage }} of {{ totalPages }}</span>
    </div>

    <ag-grid-vue
      ref="gridRef"
      class="ag-theme-quartz-auto-dark"
      style="width: 100%; height: 400px;"
      theme="legacy"
      :columnDefs="columnDefs"
      :rowData="rowData"
      :defaultColDef="defaultColDef"
      :pagination="false"
      @grid-ready="onGridReady"
    >
    </ag-grid-vue>
    
    <div class="d-flex justify-space-between align-center mt-2">
      <span>Loaded in {{ duration }} ms</span>
      <span class="text-caption text-grey">
        Showing {{ startRow }} - {{ endRow }} of {{ totalRecords }} rows
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, computed, watch } from 'vue'
import { useReportDataStore } from '@/stores/reportDataStore'
import { useMappingStore } from '@/stores/mappingStore'
import { useWorkspaceStore } from '@/stores/workspaceStore'
import { AgGridVue } from 'ag-grid-vue3'
import type { ColDef, GridApi, GridReadyEvent } from 'ag-grid-community'
import { generateSimpleMatch, normalize } from '@/helpers/queryUtils'
import { getUserLockFromToken } from '@/helpers/fieldLock'

// ----- Types -----
interface Mapping {
  alias?: string
  Alias?: string
  name?: string
  Name?: string
  DataType?: string
  dataType?: string
  IsArray?: boolean
  isArray?: boolean
}

interface FieldDef {
  alias?: string
  name?: string
  isCalculated?: boolean
  IsCalculated?: boolean
  prefix?: string
  suffix?: string
}

interface Tab {
  id?: string
  query?: any
  selectedFields?: FieldDef[]
}

interface BlockProp {
  data?: {
    pageSize?: number
    title?: string
    description?: string
    tabId?: string
    filters?: any
  }
}

const props = defineProps({
  block: { type: Object, required: true },
  inDesignMode: { type: Boolean, default: false },
  // NEW: optional fallback if block.data.tabId is missing / stale
  selectedTab: { type: Object, default: null }
})
defineEmits(['edit', 'delete'])

const gridRef = ref<any>(null)
let gridApi: GridApi | null = null

const reportStore = useReportDataStore()
const mappingsStore = useMappingStore()
const workspaceStore = useWorkspaceStore()

// AG Grid reactive data
const columnDefs = ref<ColDef[]>([])
const rowData = ref<any[]>([])
const defaultColDef = ref<ColDef>({
  resizable: true,
  sortable: false,
  filter: false,
  minWidth: 80,
})

const duration = computed(() => reportStore.duration)
const currentPage = ref(1)
const pageSize = ref<number>(props.block?.data?.pageSize || 50)
const totalPages = ref(1)
const totalRecords = ref(0)
const pipelineCache = ref<any[]>([])

const isInitialised = ref(false)

const title = computed(() => props.block?.data?.title || 'Untitled')
const description = computed(() => props.block?.data?.description || '')
const tabId = computed<string | null>(() => props.block?.data?.tabId || null)

/** Find tab by id from workspaces */
const tabFromId = computed<Tab | null>(() => {
  if (!tabId.value) return null
  for (const ws of workspaceStore.workspaces) {
    const t = (ws.tabs || []).find((tt: any) => tt.id === tabId.value)
    if (t) return t as Tab
  }
  return null
})

/** Effective tab: prefer block.tabId; fallback to parent-provided selectedTab */
const tab = computed<Tab | null>(() => tabFromId.value || (props.selectedTab as Tab) || null)

const query = computed(() => tab.value?.query || {})
const selectedFields = computed<FieldDef[]>(() => tab.value?.selectedFields || [])

const startRow = computed(() => {
  if (totalRecords.value === 0) return 0
  return (currentPage.value - 1) * pageSize.value + 1
})

const endRow = computed(() => {
  const end = currentPage.value * pageSize.value
  return Math.min(end, totalRecords.value)
})

/* ---------- helpers like results grid ---------- */
const dtOf = (m?: Mapping): string => (m?.DataType ?? m?.dataType ?? '').toString().trim().toLowerCase()
const isDecimalType = (dt?: string): boolean => !!dt && (dt === 'decimal' || dt === 'decimal128')

function findMappingByAliasOrName(key?: string): Mapping | undefined {
  if (!key) return undefined
  // capture a definite string for use inside the callback so TypeScript
  // doesn't complain about the possibly-undefined outer variable
  const search = key
  return mappingsStore.mappings.find((m: Mapping) =>
    normalize(m.alias ?? m.Alias ?? '') === normalize(search) ||
    normalize(m.name  ?? m.Name  ?? '') === normalize(search)
  ) as Mapping | undefined
}
function isArrayMapping(m: any): boolean {
  if (!m) return false
  const flag = (m as Mapping).IsArray === true || (m as Mapping).isArray === true
  const typeIsArray = dtOf(m as Mapping) === 'array'
  return flag || typeIsArray
}
function rootIsArray(path: string): boolean {
  if (!path || !path.includes('.')) return false
  const root = path.split('.')[0]
  const rootMap = findMappingByAliasOrName(root)
  return isArrayMapping(rootMap)
}
function needsUnwindForPath(path: string, m: any): boolean {
  return !!path && path.includes('.') && (isArrayMapping(m as Mapping) || rootIsArray(path))
}
function displayify(raw: unknown): string {
  if (raw == null) return ''
  if (typeof raw === 'object') {
    if (Object.prototype.hasOwnProperty.call(raw, '$numberDecimal')) return (raw as any).$numberDecimal
    const s = (raw as any).toString?.()
    if (s && s !== '[object Object]') return s
    if (Array.isArray(raw)) return (raw as any[]).map(displayify).join(', ')
    return JSON.stringify(raw)
  }
  return String(raw)
}
/* ---------------------------------------------- */

onMounted(async () => {
  await nextTick()
  // mappings used for projection; ok to call even if cached
  await mappingsStore.fetchMappings()
  isInitialised.value = true
  await submitQuery()
})

// AG Grid ready handler
function onGridReady(params: GridReadyEvent) {
  gridApi = params.api
}

/** Re-run when anything important changes */
watch(
  () => ({
    tid: tabId.value,
    tabsReady: workspaceStore.workspaces?.length,
    effTabId: tab.value?.id,
    selCount: selectedFields.value?.length,
    q: JSON.stringify(query.value ?? {}),
    ps: props.block?.data?.pageSize,
    filters: JSON.stringify(props.block?.data?.filters ?? {})
  }),
  async () => {
    if (!isInitialised.value) return
    // Update pageSize if changed
    pageSize.value = props.block?.data?.pageSize || 50
    await submitQuery()
  }
)

/* -------------------------------------------- */

async function submitQuery(): Promise<void> {
  // Debug breadcrumbs (remove if too chatty)
  console.debug('[Tabular] tabId:', tabId.value, 'effTab:', tab.value?.id, 'selectedFields:', selectedFields.value?.length)

  // If we still don’t know the tab → nothing to query yet.
  if (!tab.value) {
    console.warn('[Tabular] No effective tab, skipping query')
    pipelineCache.value = []
    await loadPage(currentPage.value)
    return
  }

  // If there are no selected fields for this tab, also no projection
  if (!selectedFields.value.length) {
    console.warn('[Tabular] No selectedFields on tab', tab.value?.id, '→ empty pipeline')
    pipelineCache.value = []
    await loadPage(currentPage.value)
    return
  }

  const pipeline: any[] = []
  const unwindRoots = new Set<string>()

  // Unwind:
  selectedFields.value.forEach((field: FieldDef) => {
    const m = findMappingByAliasOrName(field?.alias) ?? findMappingByAliasOrName(field?.name)
    const path = (m?.name ?? m?.Name) || field?.name || field?.alias
    if (path && needsUnwindForPath(path, m)) unwindRoots.add(path.split('.')[0])
  })
  ;[...unwindRoots].forEach(root => pipeline.push({ $unwind: `$${root}` }))

  // Match:
  // pass an empty array when no token parts are available to satisfy the expected parameter type
  const userLock = getUserLockFromToken([])
  const match = generateSimpleMatch(mappingsStore, query.value, userLock)
  
  // Apply additional filters from block config
  const blockFilters = props.block?.data?.filters
  if (blockFilters && blockFilters.conditions && blockFilters.conditions.length > 0) {
    const additionalMatch = generateSimpleMatch(mappingsStore, blockFilters, userLock)
    if (additionalMatch && Object.keys(additionalMatch).length > 0) {
      // Combine both matches with $and
      if (match && Object.keys(match).length > 0) {
        pipeline.push({ $match: { $and: [match, additionalMatch] } })
      } else {
        pipeline.push({ $match: additionalMatch })
      }
    } else if (match && Object.keys(match).length > 0) {
      pipeline.push({ $match: match })
    }
  } else if (match && Object.keys(match).length > 0) {
    pipeline.push({ $match: match })
  }

  // Project:
  const projection: Record<string, any> = {}
  selectedFields.value.forEach((field: FieldDef) => {
    if (!field || field.isCalculated || field.IsCalculated) return
    const m = findMappingByAliasOrName(field?.alias) ?? findMappingByAliasOrName(field?.name)
    if (!m) return
    const sourcePath = (m.name ?? m.Name) as string
    const alias = field.alias?.trim() || m.alias || m.Alias || sourcePath
    const dt = dtOf(m)
    const sourceExpr = `$${sourcePath}`
    projection[alias] = isDecimalType(dt) ? { $toString: sourceExpr } : sourceExpr
  })
  if (Object.keys(projection).length > 0) pipeline.push({ $project: projection })

  console.debug('[Tabular] Built pipeline:', JSON.stringify(pipeline))
  pipelineCache.value = pipeline
  await loadPage(currentPage.value)
}

async function loadPage(page: number): Promise<void> {
  if (!gridApi) return

  const skip = (page - 1) * pageSize.value
  try {
    await reportStore.fetchReportData(pipelineCache.value, pageSize.value, skip)
  } catch (e) {
    console.error('fetchReportData error', e)
    return
  }

  totalRecords.value = reportStore.total
  totalPages.value = Math.max(1, Math.ceil(reportStore.total / pageSize.value))
  const data = reportStore.data

  // Build AG Grid columns
  const columns: ColDef[] = selectedFields.value.map((f: FieldDef) => {
    const fieldKey = (f.alias || f.name)?.trim()
    const mapping = findMappingByAliasOrName(f?.alias) ?? findMappingByAliasOrName(f?.name)

    if ((mapping?.DataType ?? mapping?.dataType) === 'Boolean') {
      return {
        headerName: f.alias || f.name,
        field: fieldKey,
        width: 150,
        cellRenderer: (params: any) => {
          const value = params.value
          const bool = value === true || value === 'true'
          return `<div style="font-size: 1.2em; color: ${bool ? 'green' : 'red'};">${bool ? '✔' : '✖'}</div>`
        }
      }
    }

    return {
      headerName: f.alias || f.name,
      field: fieldKey,
      width: 150,
      cellRenderer: (params: any) => {
        const raw = params.value
        const val = displayify(raw)
        const rule = (query.value?.formattingRules || []).find((r: any) => r.field === f.name && r.operator === '$eq' && r.value == val)
        let style = ''
        if (rule) style = `background-color: ${rule.color};`
        return `<div style="${style}">${f.prefix || ''}${val}${f.suffix || ''}</div>`
      }
    }
  })

  columnDefs.value = columns
  rowData.value = data
}

function previousPage() { if (currentPage.value > 1) { currentPage.value--; loadPage(currentPage.value) } }
function nextPage() { if (currentPage.value < totalPages.value) { currentPage.value++; loadPage(currentPage.value) } }
</script>

<style scoped>
.tabular-block { display:flex; flex-direction:column; height:100%; padding:20px; overflow:hidden; }
.table-container { flex-grow:1; overflow:hidden; }
</style>
