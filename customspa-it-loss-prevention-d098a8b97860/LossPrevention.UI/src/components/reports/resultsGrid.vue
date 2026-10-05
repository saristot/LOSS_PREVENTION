<template>
  <div>
    <!-- Snackbar for error/success messages -->
    <v-snackbar
      v-model="showSnackbar"
      :color="snackbarColor"
      :timeout="snackbarTimeout"
      location="top"
      multi-line
    >
      <div style="white-space: pre-wrap;">{{ snackbarMessage }}</div>
      <template v-slot:actions>
        <v-btn variant="text" @click="showSnackbar = false">Close</v-btn>
      </template>
    </v-snackbar>

    <!-- Pagination & Sort Bar -->
    <div class="results-toolbar">
      <div class="toolbar-group">
        <v-btn 
          size="small"
          variant="outlined"
          :disabled="currentPage <= 1 || fraudStore.loadingFraudAnalysis" 
          @click="previousPage"
        >
          Previous
        </v-btn>
        <v-btn 
          size="small"
          variant="outlined"
          :disabled="currentPage >= totalPages || fraudStore.loadingFraudAnalysis" 
          @click="nextPage"
        >
          Next
        </v-btn>
        <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
      </div>

      <div class="toolbar-group">
        <v-select
          v-model="sortField"
          :items="sortFieldOptions"
          label="Sort by"
          density="compact"
          variant="outlined"
          hide-details
          class="compact-select"
          style="width: 140px"
          :menu-props="{ contentClass: 'compact-menu' }"
          :disabled="fraudStore.loadingFraudAnalysis"
          @update:model-value="changeSortField"
        />
        <v-select
          v-model="sortDirection"
          :items="sortDirectionOptions"
          label="Order"
          density="compact"
          variant="outlined"
          hide-details
          class="compact-select"
          style="width: 100px"
          :menu-props="{ contentClass: 'compact-menu' }"
          :disabled="fraudStore.loadingFraudAnalysis"
          @update:model-value="changeSortField"
        />
      </div>

      <v-spacer />

      <v-select
        v-model="pageSize"
        :items="pageSizeOptions"
        label="Records per page"
        density="compact"
        variant="outlined"
        hide-details
        class="compact-select"
        style="width: 130px"
        :menu-props="{ contentClass: 'compact-menu' }"
        :disabled="fraudStore.loadingFraudAnalysis"
        @update:model-value="changePageSize"
      />
    </div>

    <!-- Action Buttons -->
    <div class="action-toolbar">
      <v-btn 
        size="small"
        color="primary" 
        @click="submitQuery" 
        :disabled="!isValid"
        :loading="isSubmittingQuery"
      >
        Run Query
      </v-btn>
      
      <!-- Export Menu -->
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn 
            size="small"
            color="secondary"
            variant="outlined"
            v-bind="props"
          >
            <v-icon size="small" class="mr-1">mdi-download</v-icon>
            Export
          </v-btn>
        </template>
        <v-list density="compact">
          <v-list-subheader>Current Page</v-list-subheader>
          <v-list-item @click="exportPage('csv')">
            <v-list-item-title>CSV</v-list-item-title>
          </v-list-item>
          <v-list-item @click="exportPage('xlsx')">
            <v-list-item-title>Excel</v-list-item-title>
          </v-list-item>
          <v-list-item @click="exportPage('pdf')">
            <v-list-item-title>PDF</v-list-item-title>
          </v-list-item>
          <v-divider />
          <v-list-subheader>All Pages</v-list-subheader>
          <v-list-item @click="exportAllPages('csv')">
            <v-list-item-title>CSV</v-list-item-title>
          </v-list-item>
          <v-list-item @click="exportAllPages('xlsx')">
            <v-list-item-title>Excel</v-list-item-title>
          </v-list-item>
          <v-list-item @click="exportAllPages('pdf')">
            <v-list-item-title>PDF</v-list-item-title>
          </v-list-item>
          <template v-if="fraudHighlightsAvailable && fraudIdMap.size > 0">
            <v-divider />
            <v-list-subheader>Fraud Cases</v-list-subheader>
            <v-list-item @click="exportFraudPage('csv')">
              <v-list-item-title>Current Page (CSV)</v-list-item-title>
            </v-list-item>
            <v-list-item @click="exportAllFraudPages('csv')">
              <v-list-item-title>All Pages (CSV)</v-list-item-title>
            </v-list-item>
          </template>
        </v-list>
      </v-menu>
      
      <v-btn 
        size="small"
        color="warning" 
        @click="analyzeFraud" 
        :disabled="currentPageData.length === 0 || fraudStore.loadingFraudAnalysis"
      >
        <v-icon size="small" class="mr-1">mdi-shield-alert</v-icon>
        Detect Potential Fraud
      </v-btn>
      <v-btn 
        size="small"
        color="primary" 
        variant="outlined"
        @click="fraudSettingsOpen = true"
        :disabled="fraudStore.loadingFraudAnalysis"
      >
        <v-icon size="small">mdi-cog</v-icon>
      </v-btn>
      <v-switch
        v-if="fraudHighlightsAvailable"
        v-model="showOnlyFraud"
        label="Show Only Fraud"
        hide-details
        density="compact"
        color="error"
        class="compact-switch"
        @update:model-value="filterFraudRows"
      />
      <v-spacer />
      <v-btn 
        v-if="fraudHighlightsAvailable" 
        size="small"
        color="info" 
        variant="outlined"
        @click="clearFraudAnalysis"
        :disabled="fraudStore.loadingFraudAnalysis"
      >
        Clear Highlights
      </v-btn>
      <v-btn 
        v-if="fraudHighlightsAvailable" 
        size="small"
        color="info" 
        variant="text"
        @click="showFraudLegend = !showFraudLegend"
        :disabled="fraudStore.loadingFraudAnalysis"
      >
        <v-icon size="small">{{ showFraudLegend ? 'mdi-eye-off' : 'mdi-eye' }}</v-icon>
        Legend
      </v-btn>
    </div>

    <v-alert 
      v-if="fraudHighlightsAvailable && fraudIdMap.size > 0"
      type="warning"
      variant="tonal"
      density="compact"
      class="compact-alert"
      icon="mdi-shield-alert"
    >
      <strong>{{ fraudOnCurrentPage }}</strong> fraud case(s) on this page ({{ fraudIdMap.size }} total)
    </v-alert>

    <div v-if="showFraudLegend && fraudHighlightsAvailable" class="fraud-legend">
      <div class="legend-title">Legend</div>
      <div class="legend-items">
        <div class="legend-item">
          <div class="legend-color" style="background-color: #ffcccc;"></div>
          <span><strong>High:</strong> Strong fraud indicators</span>
        </div>
        <div class="legend-item">
          <div class="legend-color" style="background-color: #fff3cd;"></div>
          <span><strong>Medium:</strong> Suspicious patterns</span>
        </div>
        <div class="legend-item">
          <div class="legend-color" style="background-color: #ffffdd;"></div>
          <span><strong>Low:</strong> Minor anomalies</span>
        </div>
      </div>
      <div class="legend-hint">Hover over rows for details</div>
    </div>

    <!-- Results Grid Container (for overlay containment) -->
    <div class="results-grid-wrapper" @contextmenu.prevent>
      <ag-grid-vue
        ref="gridRef"
        class="ag-theme-quartz-auto-dark"
        style="width: 100%; height: 600px;"
        theme="legacy"
        :columnDefs="columnDefs"
        :rowData="rowData"
        :defaultColDef="defaultColDef"
        :pagination="false"
        :rowSelection="'multiple'"
        :getRowStyle="getRowStyle"
        :tooltipShowDelay="200"
        @grid-ready="onGridReady"
        @cell-context-menu="onCellContextMenu"
      >
      </ag-grid-vue>
      
      <!-- Custom Context Menu -->
      <teleport to="body">
        <v-card
          v-if="contextMenuShow"
          class="context-menu-card"
          :style="{ position: 'fixed', left: contextMenuX + 'px', top: contextMenuY + 'px', zIndex: 9999 }"
          elevation="8"
        >
          <v-list density="compact">
            <v-list-item @click="handleAddConditionFromContext">
              <template v-slot:prepend>
                <v-icon size="small">mdi-filter-plus</v-icon>
              </template>
              <v-list-item-title>Add Condition</v-list-item-title>
            </v-list-item>
            <v-list-item @click="handleAddFormattingRuleFromContext">
              <template v-slot:prepend>
                <v-icon size="small">mdi-format-color-fill</v-icon>
              </template>
              <v-list-item-title>Add Formatting Rule</v-list-item-title>
            </v-list-item>
            <v-list-item @click="handleEuclideanDistanceFromContext">
              <template v-slot:prepend>
                <v-icon size="small">mdi-chart-scatter-plot</v-icon>
              </template>
              <v-list-item-title>Euclidean Distance</v-list-item-title>
            </v-list-item>
            
            <v-divider class="my-1" />
            
            <!-- Export Selected Cells submenu -->
            <v-list-group value="export-cells">
              <template v-slot:activator="{ props }">
                <v-list-item v-bind="props">
                  <template v-slot:prepend>
                    <v-icon size="small">mdi-table-arrow-right</v-icon>
                  </template>
                  <v-list-item-title>Export Selected Rows</v-list-item-title>
                </v-list-item>
              </template>
              <v-list-item @click="contextMenuShow = false; exportCells('csv')">
                <v-list-item-title class="pl-4">CSV</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportCells('xlsx')">
                <v-list-item-title class="pl-4">Excel</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportCells('pdf', 'portrait')">
                <v-list-item-title class="pl-4">PDF (Portrait)</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportCells('pdf', 'landscape')">
                <v-list-item-title class="pl-4">PDF (Landscape)</v-list-item-title>
              </v-list-item>
            </v-list-group>
            
            <!-- Export Current Page submenu -->
            <v-list-group value="export-page">
              <template v-slot:activator="{ props }">
                <v-list-item v-bind="props">
                  <template v-slot:prepend>
                    <v-icon size="small">mdi-file-export</v-icon>
                  </template>
                  <v-list-item-title>Export Current Page</v-list-item-title>
                </v-list-item>
              </template>
              <v-list-item @click="contextMenuShow = false; exportPage('csv')">
                <v-list-item-title class="pl-4">CSV</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportPage('xlsx')">
                <v-list-item-title class="pl-4">Excel</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportPage('pdf', 'portrait')">
                <v-list-item-title class="pl-4">PDF (Portrait)</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportPage('pdf', 'landscape')">
                <v-list-item-title class="pl-4">PDF (Landscape)</v-list-item-title>
              </v-list-item>
            </v-list-group>
            
            <!-- Export All Pages submenu -->
            <v-list-group value="export-all">
              <template v-slot:activator="{ props }">
                <v-list-item v-bind="props">
                  <template v-slot:prepend>
                    <v-icon size="small">mdi-file-multiple</v-icon>
                  </template>
                  <v-list-item-title>Export All Pages</v-list-item-title>
                </v-list-item>
              </template>
              <v-list-item @click="contextMenuShow = false; exportAllPages('csv')">
                <v-list-item-title class="pl-4">CSV</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportAllPages('xlsx')">
                <v-list-item-title class="pl-4">Excel</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportAllPages('pdf', 'portrait')">
                <v-list-item-title class="pl-4">PDF (Portrait)</v-list-item-title>
              </v-list-item>
              <v-list-item @click="contextMenuShow = false; exportAllPages('pdf', 'landscape')">
                <v-list-item-title class="pl-4">PDF (Landscape)</v-list-item-title>
              </v-list-item>
            </v-list-group>
            
            <!-- Fraud exports if available -->
            <template v-if="fraudHighlightsAvailable && fraudIdMap.size > 0">
              <v-divider class="my-1" />
              
              <v-list-group value="export-fraud-page">
                <template v-slot:activator="{ props }">
                  <v-list-item v-bind="props">
                    <template v-slot:prepend>
                      <v-icon size="small">mdi-shield-alert</v-icon>
                    </template>
                    <v-list-item-title>Export Fraud - Current Page</v-list-item-title>
                  </v-list-item>
                </template>
                <v-list-item @click="contextMenuShow = false; exportFraudPage('csv')">
                  <v-list-item-title class="pl-4">CSV</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportFraudPage('xlsx')">
                  <v-list-item-title class="pl-4">Excel</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportFraudPage('pdf', 'portrait')">
                  <v-list-item-title class="pl-4">PDF (Portrait)</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportFraudPage('pdf', 'landscape')">
                  <v-list-item-title class="pl-4">PDF (Landscape)</v-list-item-title>
                </v-list-item>
              </v-list-group>
              
              <v-list-group value="export-fraud-all">
                <template v-slot:activator="{ props }">
                  <v-list-item v-bind="props">
                    <template v-slot:prepend>
                      <v-icon size="small">mdi-shield-alert-outline</v-icon>
                    </template>
                    <v-list-item-title>Export Fraud - All Pages</v-list-item-title>
                  </v-list-item>
                </template>
                <v-list-item @click="contextMenuShow = false; exportAllFraudPages('csv')">
                  <v-list-item-title class="pl-4">CSV</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportAllFraudPages('xlsx')">
                  <v-list-item-title class="pl-4">Excel</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportAllFraudPages('pdf', 'portrait')">
                  <v-list-item-title class="pl-4">PDF (Portrait)</v-list-item-title>
                </v-list-item>
                <v-list-item @click="contextMenuShow = false; exportAllFraudPages('pdf', 'landscape')">
                  <v-list-item-title class="pl-4">PDF (Landscape)</v-list-item-title>
                </v-list-item>
              </v-list-group>
            </template>
          </v-list>
        </v-card>
      </teleport>
      
      <!-- Fraud Detection Overlay -->
      <v-overlay 
        v-model="showFraudModal" 
        class="align-center justify-center"
        persistent
        contained
      >
        <v-card class="pa-8 text-center" min-width="500" max-width="600">
          <div class="d-flex justify-center mb-6">
            <v-progress-circular
              :model-value="fraudAnalysisProgress"
              size="120"
              width="12"
              color="warning"
            >
              <span class="text-h4 font-weight-bold">{{ fraudAnalysisProgress }}%</span>
            </v-progress-circular>
          </div>
          
          <h3 class="text-h5 mb-4">{{ fraudAnalysisTitle }}</h3>
          
          <v-chip 
            v-if="fraudAnalysisFraudCount > 0"
            color="error" 
            size="large" 
            class="mb-4 font-weight-bold"
          >
            <v-icon left>mdi-alert-circle</v-icon>
            {{ fraudAnalysisFraudCount }} Potential Fraud Cases Detected
          </v-chip>
          
          <p class="text-body-1 mb-3">
            {{ fraudAnalysisMessage }}
          </p>
          
          <p class="text-body-2 text-medium-emphasis mb-1">
            Elapsed Time: {{ (fraudStore.elapsedTimeMs / 1000).toFixed(1) }}s
          </p>
          
          <v-btn 
            color="error" 
            variant="outlined"
            size="large"
            class="mt-4"
            @click="stopFraudAnalysis"
          >
            <v-icon left>mdi-stop</v-icon>
            Stop Analysis
          </v-btn>
        </v-card>
      </v-overlay>
      
      <!-- Data Loading Overlay -->
      <v-overlay 
        v-model="showLoadingOverlay" 
        class="align-center justify-center"
        persistent
        contained
      >
        <v-card class="pa-8 text-center" min-width="400">
          <div class="d-flex justify-center mb-6">
            <v-progress-circular
              :model-value="loadingProgress"
              size="100"
              width="10"
              color="primary"
            >
              <span class="text-h5 font-weight-bold">{{ loadingProgress }}%</span>
            </v-progress-circular>
          </div>
          
          <h3 class="text-h6 mb-2">Loading Data</h3>
          <p class="text-body-2 text-medium-emphasis">{{ loadingMessage }}</p>
        </v-card>
      </v-overlay>
    </div>
    <div v-if="shouldUpdateLoadTime" class="table-footer">
      <span class="load-time">Loaded in {{ duration }} ms</span>
    </div>

    <details v-if="submittedQuery" class="pipeline-details">
      <summary class="pipeline-summary">Generated Pipeline</summary>
      <pre class="pipeline-code">{{ toShellSyntax(submittedQuery) }}</pre>
    </details>
  </div>

  <distanceDialog
    v-model:open="distanceDialogOpen"
    :selectedFields="distanceFields"
    :dateFields="availableDateFields"
  />

  <FraudSettingsDialog v-model:open="fraudSettingsOpen" />
</template>

<script setup lang="ts">
import { ref, Ref, computed, onMounted, nextTick, shallowRef, watch } from 'vue';
import type { NamedField } from '@/interfaces/distance';
import { useReportDataStore } from '@/stores/reportDataStore';
import { useMappingStore } from '@/stores/mappingStore';
import { useAiStore } from '@/stores/aiStore';
import { useFraudDetectionStore } from '@/stores/fraudDetectionStore';
import { AgGridVue } from 'ag-grid-vue3';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';
import type { ColDef, GridOptions, GridReadyEvent, RowClassParams, RowStyle } from 'ag-grid-community';
import { normalize, toShellSyntax, generateSimpleMatch } from '@/helpers/queryUtils';
import { getUserLockFromToken } from '@/helpers/fieldLock';
import distanceDialog from './distanceDialog.vue';
import FraudSettingsDialog from './fraudSettingsDialog.vue';
import Papa from 'papaparse';
import pdfMake from 'pdfmake/build/pdfmake';
import pdfFonts from 'pdfmake/build/vfs_fonts';
import * as XLSX from 'xlsx';

const distanceDialogOpen = ref(false);
const distanceFields: Ref<NamedField[]> = ref([]);
const availableDateFields = ref<string[]>([]);
const fraudHighlightsAvailable = ref(false); // Track if fraud rules are available for highlighting
const currentPageData = ref<any[]>([]); // Current page data only
const fraudRules = ref<any[]>([]); // Store fraud detection rules from AI
const showFraudModal = ref(false); // Control fraud detection modal visibility
const fraudSettingsOpen = ref(false); // Control fraud settings dialog
const showLoadingOverlay = ref(false); // Control chunked loading overlay
const loadingProgress = ref(0); // Loading progress percentage
const loadingMessage = ref(''); // Loading status message
const fraudIdMap = new Map<string, { 
  fraudType: string; 
  explanation: string; 
  severity: string;
  confidence: number;
  relevantFields?: Record<string, any>;
}>(); // Map of _id -> fraud details for CURRENT PAGE only
const showFraudLegend = ref(false); // Default collapsed
const showOnlyFraud = ref(false); // Filter to show only fraud records
const isSubmittingQuery = ref(false); // Track if query is being submitted
const shouldUpdateLoadTime = ref(false); // Track if load time should be displayed

// Fraud analysis modal state
const fraudAnalysisProgress = ref(0);
const fraudAnalysisTitle = ref('Detecting Potential Fraud Cases');
const fraudAnalysisMessage = ref('');
const fraudAnalysisFraudCount = ref(0);

// Snackbar state
const showSnackbar = ref(false);
const snackbarMessage = ref('');
const snackbarColor = ref('success');
const snackbarTimeout = ref(5000);

// Context menu state
const contextMenuShow = ref(false);
const contextMenuX = ref(0);
const contextMenuY = ref(0);
const contextMenuCell = ref<any>(null);

// @ts-ignore - vfs assignment compatibility
pdfMake.vfs = pdfFonts.pdfMake?.vfs || pdfFonts;

const props = defineProps({ tab: Object });
const query = computed(() => props.tab?.query);
const selectedFields = computed(() => props.tab?.selectedFields);

const reportStore = useReportDataStore();
const mappingsStore = useMappingStore();
const aiStore = useAiStore();
const fraudStore = useFraudDetectionStore();

// AG Grid references
const gridRef = ref(null);
let gridApi: any = null;

// AG Grid reactive data
const columnDefs = ref<ColDef[]>([]);
const rowData = ref<any[]>([]);
const defaultColDef = ref<ColDef>({
  resizable: true,
  sortable: false, // Server-side sorting
  filter: false,
  minWidth: 80,
  tooltipValueGetter: (params: any) => {
    const rowData = params.data;
    if (!rowData || !rowData._id) return '';
    
    const fraudDetails = fraudIdMap.get(String(rowData._id));
    if (!fraudDetails) return '';
    
    return `Fraud Type: ${fraudDetails.fraudType}\nSeverity: ${fraudDetails.severity}\nConfidence: ${fraudDetails.confidence.toFixed(0)}%\n\n${fraudDetails.explanation}`;
  }
});

const duration = computed(() => reportStore.duration);
const fraudAnalysisTimeInSeconds = computed(() => {
  const ms = fraudStore.elapsedTimeMs;
  return ms > 0 ? (ms / 1000).toFixed(2) : '0.00';
});

// Count fraud cases on current page only
const fraudOnCurrentPage = computed(() => {
  if (!currentPageData.value || currentPageData.value.length === 0) return 0;
  let count = 0;
  currentPageData.value.forEach((row: any) => {
    if (row._id && fraudIdMap.has(row._id)) {
      count++;
    }
  });
  return count;
});

const currentPage = ref(1);
const totalPages = ref(1);
const pageSize = ref(50);
const totalRecords = ref(0);
const pageSizeOptions = [10, 25, 50, 100, 1000];
const pipelineCache = ref<any[]>([]);
const submittedQuery = ref<any[] | null>(null);
const sortField = ref('_id'); // Default sort by _id for consistency
const sortDirection = ref<1 | -1>(1); // 1 = ascending, -1 = descending
const sortFieldOptions = ref<string[]>(['_id']); // Will be populated from selected fields
const sortDirectionOptions = [
  { title: 'Ascending', value: 1 },
  { title: 'Descending', value: -1 }
];

// Cache for expensive computations
const mappingCache = new Map<string, any>();

onMounted(async () => {
  try {
    await mappingsStore.fetchMappings();
    buildMappingCache();
    
    // Close context menu on click outside
    document.addEventListener('click', () => {
      contextMenuShow.value = false;
    });
  } catch (error) {
    console.error('Mount error:', error);
  }
});

// No watchers needed - client-side pagination with cached data

const isValid = computed(() => {
  // Allow queries with no conditions (fetch all)
  const conditions = query.value?.conditions;
  if (!conditions || !Array.isArray(conditions)) return true; // No conditions = valid (fetch all)
  
  // If there are conditions, they must all be valid
  return conditions.every((c: any) =>
    c.field && c.operator && c.value !== undefined && c.value !== ''
  );
});

/* ------------------------ helpers: mappings & types ------------------------ */
const dtOf = (m: any) => (m?.DataType ?? m?.dataType ?? '').toString().trim().toLowerCase();
const isDecimalType = (dt?: string) => !!dt && (dt === 'decimal' || dt === 'decimal128');
const isIntType = (dt?: string) => !!dt && (dt === 'int32' || dt === 'int64' || dt === 'long' || dt === 'integer');
const isDoubleType = (dt?: string) => !!dt && (dt === 'double' || dt === 'number' || dt === 'float' || dt === 'numeric');

const toDecimalExtended = (v: any) => ({ $numberDecimal: String(v) });

function buildMappingCache() {
  mappingCache.clear();
  mappingsStore.mappings.forEach((m: any) => {
    const nameKey = normalize(m.name ?? m.Name);
    const aliasKey = normalize(m.alias ?? m.Alias);
    if (nameKey) mappingCache.set(nameKey, m);
    if (aliasKey && aliasKey !== nameKey) mappingCache.set(aliasKey, m);
  });
}

function findMappingByAliasOrName(key?: string): any | undefined {
  if (!key) return undefined;
  return mappingCache.get(normalize(key));
}

function isArrayMapping(m?: any): boolean {
  if (!m) return false;
  const flag = (m.IsArray === true) || (m.isArray === true);
  const typeIsArray = dtOf(m) === 'array';
  return flag || typeIsArray;
}

function rootIsArray(path: string): boolean {
  if (!path || !path.includes('.')) return false;
  const root = path.split('.')[0];
  const rootMap = findMappingByAliasOrName(root);
  return isArrayMapping(rootMap);
}

function needsUnwindForPath(path: string, m?: any): boolean {
  if (!path) return false;
  
  // Check if the mapping itself is marked as an array
  if (m && (m.IsArray === true || m.isArray === true)) {
    return true;
  }
  
  // Check if path contains a dot (nested field)
  if (path.includes('.')) {
    // Check if the root of the path is an array mapping
    return rootIsArray(path);
  }
  
  // NEW: Check if this field belongs to an array by looking for array parent mappings
  // e.g., "LineItemBarcode" might belong to "LineItem" array
  // Look for potential array parent by checking if any array mapping name is a prefix
  const arrayMappings = mappingsStore.mappings.filter((mapping: any) => isArrayMapping(mapping));
  for (const arrayMapping of arrayMappings) {
    const arrayName = arrayMapping.name || arrayMapping.Name;
    // Check if the field path starts with the array name (case-insensitive partial match)
    if (path.toLowerCase().startsWith(arrayName.toLowerCase())) {
      return true;
    }
  }
  
  return false;
}

/** Build a type-aware $match using mapping metadata.
 *  RULE: for $gt/$gte/$lt/$lte we NEVER use $numberDecimal; emit plain numbers.
 *  - Equality ($eq/$ne) on Decimal128 => use {$numberDecimal:"..."}.
 *  - $in/$nin on Decimal128 => wrap each element as {$numberDecimal:"..."}.
 */
const COMPARATIVE_OPS = new Set(['$gt', '$gte', '$lt', '$lte']);
const EQUALITY_OPS    = new Set(['$eq', '$ne']);
const ARRAY_OPS       = new Set(['$in', '$nin']);
const VALUE_OPS       = new Set([...COMPARATIVE_OPS, ...EQUALITY_OPS, ...ARRAY_OPS]);

function buildTypeAwareMatch(mappings: any[], conditions: any[]) {
  if (!conditions?.length) return null;

  const parts: any[] = [];

  for (const c of conditions) {
    const m = mappings.find((mm: any) =>
      normalize(mm.name ?? mm.Name) === normalize(c.field)
    );
    const fieldPath = m?.name ?? m?.Name ?? c.field;
    const dt = dtOf(m);

    // Array operators
    if (ARRAY_OPS.has(c.operator) && Array.isArray(c.value)) {
      let arr: any[] = c.value;
      if (isDecimalType(dt)) {
        arr = c.value.map((x: any) => toDecimalExtended(x));
      } else if (isIntType(dt) || isDoubleType(dt)) {
        arr = c.value.map((x: any) => {
          const n = Number(x);
          return Number.isNaN(n) ? x : n;
        });
      }
      parts.push({ [fieldPath]: { [c.operator]: arr } });
      continue;
    }

    // Scalar operators
    if (VALUE_OPS.has(c.operator)) {
      let val: any = c.value;

      if (COMPARATIVE_OPS.has(c.operator)) {
        // For Decimal128, use $numberDecimal for comparisons too
        if (isDecimalType(dt)) {
          val = toDecimalExtended(val);
        } else if (isIntType(dt) || isDoubleType(dt)) {
          const n = Number(val);
          if (!Number.isNaN(n)) val = n;
        }
      } else if (EQUALITY_OPS.has(c.operator)) {
        if (isDecimalType(dt)) {
          val = toDecimalExtended(val);
        } else if (isIntType(dt) || isDoubleType(dt)) {
          const n = Number(val);
          if (!Number.isNaN(n)) val = n;
        }
      }

      parts.push({ [fieldPath]: { [c.operator]: val } });
      continue;
    }

    // Operators like $exists/$regex/etc.
    parts.push({ [fieldPath]: { [c.operator]: c.value } });
  }

  if (parts.length === 1) return parts[0];
  return { $and: parts };
}

/** Convert raw values to readable strings for the grid (defensive). */
function displayify(raw: any): string {
  if (raw === undefined || raw === null) return '';
  
  if (typeof raw === 'object') {
    // Extended JSON of Decimal128
    if (Object.prototype.hasOwnProperty.call(raw, '$numberDecimal')) {
      return (raw as any).$numberDecimal;
    }
    // BSON types that implement toString()
    const s = (raw as any).toString?.();
    if (s && s !== '[object Object]') return s;
    if (Array.isArray(raw)) return raw.map(displayify).join(', ');
    return JSON.stringify(raw);
  }
  return String(raw);
}

// Cache context menu to avoid recreating on every column
// Custom context menu handler
function onCellContextMenu(event: any) {
  event.event.preventDefault();
  contextMenuCell.value = event;
  contextMenuX.value = event.event.clientX;
  contextMenuY.value = event.event.clientY;
  contextMenuShow.value = true;
}

function handleAddConditionFromContext() {
  contextMenuShow.value = false;
  if (!contextMenuCell.value) return;
  
  const params = contextMenuCell.value;
  const fieldName = params.column?.getColId();
  let value = params.value;

  const mapping = findMappingByAliasOrName(fieldName);
  const dt = dtOf(mapping);

  if (isDecimalType(dt)) {
    value = String(value);
  } else if (isIntType(dt) || isDoubleType(dt)) {
    const n = Number(value);
    if (!Number.isNaN(n)) value = n;
  } else if (dt === 'boolean') {
    value = value === true || value === 'true';
  } else if (dt === 'date' || dt === 'datetime') {
    value = toDateTimeLocal(value);
  }

  if (value === undefined || value === null || value === '') {
    alert('Cannot add condition: the selected cell has no value.');
    return;
  }

  const fieldPath = mapping ? (mapping.name ?? mapping.Name) : fieldName;

  (query.value.conditions ||= []).push({
    id: Date.now().toString(36),
    field: fieldPath,
    operator: '$eq',
    value
  });

  submitQuery();
}

function handleAddFormattingRuleFromContext() {
  contextMenuShow.value = false;
  if (!contextMenuCell.value) return;
  
  const params = contextMenuCell.value;
  const fieldName = params.column?.getColId();
  let value = params.value;

  if (value === undefined || value === null || value === '') {
    alert('Cannot add formatting rule: the selected cell has no value.');
    return;
  }

  const isDate = mappingsStore.mappings.find((m: any) =>
    (m.alias ?? m.Alias) === fieldName && (m.dataType ?? m.DataType) === 'Date'
  );
  if (isDate) value = toDateTimeLocal(value);

  query.value.formattingRules = [
    ...(query.value.formattingRules || []),
    {
      id: Date.now().toString(36),
      field: fieldName,
      operator: '$eq',
      value,
      color: '#FFEB3B'
    }
  ];
  
  loadPage(currentPage.value);
}

function handleEuclideanDistanceFromContext() {
  contextMenuShow.value = false;
  if (!contextMenuCell.value) return;
  
  const params = contextMenuCell.value;
  const rowData = params.node?.data;
  if (!rowData) return;
  
  const selectedFieldNames = props.tab?.selectedFields
    ?.filter((f: any) => f.name && (f.alias || f.name))
    .map((f: any) => ({
      name: f.name,
      alias: f.alias?.trim() || f.name
    })) || [];

  if (selectedFieldNames.length < 3) {
    alert('You must select at least 3 fields to run distance analysis.');
    return;
  }

  const fieldValuePairs: { name: string; value: any }[] =
    selectedFieldNames.map((f: any) => {
      let value = rowData[f.name];
      
      // Handle nested properties
      if (value === undefined && f.name.includes('.')) {
        const parts = f.name.split('.');
        value = parts.reduce((obj: any, key: any) => obj?.[key], rowData);
      }
      
      return { name: f.name, value: value };
    });

  availableDateFields.value = mappingsStore.mappings
    .filter((m: any) => (m.DataType ?? m.dataType) === 'Date')
    .map((m: any) => (m.Alias ?? m.alias) || (m.Name ?? m.name));

  distanceFields.value = fieldValuePairs;
  distanceDialogOpen.value = true;
}

// Old context menu code (commented out)
/*
function getContextMenuItems(params: GetContextMenuItemsParams) {
  const result: any[] = [
    {
      name: 'Export Selected Cells',
      subMenu: [
        { name: 'CSV', action: () => exportCells('csv') },
        { name: 'PDF', action: () => exportCells('pdf') },
        { name: 'Excel', action: () => exportCells('xlsx') }
      ]
    },
    {
      name: 'Export Current Page',
      subMenu: [
        { name: 'CSV', action: () => exportPage('csv') },
        { name: 'PDF', action: () => exportPage('pdf') },
        { name: 'Excel', action: () => exportPage('xlsx') }
      ]
    },
    {
      name: 'Export All Pages',
      subMenu: [
        { name: 'CSV', action: () => exportAllPages('csv') },
        { name: 'PDF', action: () => exportAllPages('pdf') },
        { name: 'Excel', action: () => exportAllPages('xlsx') }
      ]
    }
  ];
  
  // Add fraud export options if fraud has been detected
  if (fraudIdMap.size > 0) {
    result.push('separator' as any);
    result.push({
      name: 'Export Fraud Cases - Current Page',
      subMenu: [
        { name: 'CSV', action: () => exportFraudPage('csv') },
        { name: 'PDF', action: () => exportFraudPage('pdf') },
        { name: 'Excel', action: () => exportFraudPage('xlsx') }
      ]
    });
    result.push({
      name: 'Export Fraud Cases - All Pages',
      subMenu: [
        { name: 'CSV', action: () => exportAllFraudPages('csv') },
        { name: 'PDF', action: () => exportAllFraudPages('pdf') },
        { name: 'Excel', action: () => exportAllFraudPages('xlsx') }
      ]
    });
  }
  
  result.push('separator' as any);
  
  // Euclidean Distance - only if cell is clicked
  if (params.column && params.node) {
    result.push({
      name: 'Euclidean Distance',
      action: () => handleEuclideanDistanceAG(params)
    });
  }
  
  result.push('separator' as any);
  
  // Add Condition - only if cell is clicked
  if (params.column && params.node) {
    result.push({
      name: 'Add Condition',
      action: () => handleAddConditionAG(params)
    });
    result.push({
      name: 'Add Formatting Rule',
      action: () => handleAddFormattingRuleAG(params)
    });
  }
  
  return result;
}

function handleEuclideanDistanceAG(params: any) {
  const rowData = params.node?.data;
  if (!rowData) return;
  
  const selectedFieldNames = props.tab?.selectedFields
    ?.filter((f: any) => f.name && (f.alias || f.name))
    .map((f: any) => ({
      name: f.name,
      alias: f.alias?.trim() || f.name
    })) || [];

  if (selectedFieldNames.length < 3) {
    alert('You must select at least 3 fields to run distance analysis.');
    return;
  }

  const fieldValuePairs: { name: string; value: any }[] =
    selectedFieldNames.map((f: any) => {
      let value = rowData[f.name];
      
      // Handle nested properties
      if (value === undefined && f.name.includes('.')) {
        const parts = f.name.split('.');
        value = parts.reduce((obj: any, key: any) => obj?.[key], rowData);
      }
      
      return { name: f.name, value: value };
    });

  availableDateFields.value = mappingsStore.mappings
    .filter((m: any) => (m.DataType ?? m.dataType) === 'Date')
    .map((m: any) => (m.Alias ?? m.alias) || (m.Name ?? m.name));

  distanceFields.value = fieldValuePairs;
  distanceDialogOpen.value = true;
}

function handleAddConditionAG(params: any) {
  const fieldName = params.column?.getColId();
  let value = params.value;

  const mapping = findMappingByAliasOrName(fieldName);
  const dt = dtOf(mapping);

  if (isDecimalType(dt)) {
    value = String(value);
  } else if (isIntType(dt) || isDoubleType(dt)) {
    const n = Number(value);
    if (!Number.isNaN(n)) value = n;
  } else if (dt === 'boolean') {
    value = value === true || value === 'true';
  } else if (dt === 'date' || dt === 'datetime') {
    value = toDateTimeLocal(value);
  }

  if (value === undefined || value === null || value === '') {
    alert('Cannot add condition: the selected cell has no value.');
    return;
  }

  const fieldPath = mapping ? (mapping.name ?? mapping.Name) : fieldName;

  (query.value.conditions ||= []).push({
    id: Date.now().toString(36),
    field: fieldPath,
    operator: '$eq',
    value
  });

  submitQuery();
}

function handleAddFormattingRuleAG(params: any) {
  const fieldName = params.column?.getColId();
  let value = params.value;

  if (value === undefined || value === null || value === '') {
    alert('Cannot add formatting rule: the selected cell has no value.');
    return;
  }

  const isDate = mappingsStore.mappings.find((m: any) =>
    (m.alias ?? m.Alias) === fieldName && (m.dataType ?? m.DataType) === 'Date'
  );
  if (isDate) value = toDateTimeLocal(value);

  query.value.formattingRules = [
    ...(query.value.formattingRules || []),
    {
      id: Date.now().toString(36),
      field: fieldName,
      operator: '$eq',
      value,
      color: '#FFEB3B'
    }
  ];
  
  loadPage(currentPage.value);
}
*/

// AG Grid row styling for fraud highlighting
function getRowStyle(params: RowClassParams): RowStyle | undefined {
  const rowData = params.data;
  if (!rowData || !rowData._id) return undefined;
  
  const fraudDetails = fraudIdMap.get(String(rowData._id));
  if (!fraudDetails) return undefined;
  
  const severity = fraudDetails.severity?.toLowerCase();

  if (severity === 'high') {
    return { backgroundColor: '#ffcdd2', color: '#b71c1c', fontWeight: '600' };
  } else if (severity === 'medium') {
    return { backgroundColor: '#ffe0b2', color: '#e65100', fontWeight: '600' };
  } else if (severity === 'low') {
    return { backgroundColor: '#fff9c4', color: '#827717', fontWeight: '500' };
  }

  return undefined;
}

/* ------------------------ Fraud Analysis (CURRENT PAGE ONLY) ------------------------ */
async function analyzeFraud() {
  if (!currentPageData.value?.length) {
    snackbarMessage.value = 'No data on current page to analyze';
    snackbarColor.value = 'warning';
    showSnackbar.value = true;
    return;
  }

  // Load fraud detection settings from server if not already loaded
  if (!fraudStore.lastSavedAt && !fraudStore.loadingSettings) {
    try {
      await fraudStore.loadSettings();
    } catch (error) {
      console.warn('Failed to load fraud settings, using defaults:', error);
    }
  }

  // Initialize modal state
  fraudAnalysisProgress.value = 0;
  fraudAnalysisTitle.value = 'Detecting Potential Fraud Cases';
  fraudAnalysisMessage.value = `Starting analysis across ${totalPages.value} pages (${totalRecords.value} records)...`;
  fraudAnalysisFraudCount.value = 0;
  showFraudModal.value = true;
  
  try {
    console.log(`🚀 Analyzing fraud across all ${totalPages.value} pages (${totalRecords.value} total records)...`);
    
    // Clear ALL previous fraud data and highlights when re-running analysis
    fraudHighlightsAvailable.value = false;
    showFraudLegend.value = false;
    showOnlyFraud.value = false;
    fraudRules.value = [];
    fraudIdMap.clear();
    if (fraudStore.fraudMetadataMap) {
      fraudStore.fraudMetadataMap.clear();
    }
    aiStore.resetAnalysisState();
    
    const mappings = mappingsStore.mappings || [];
    let totalFraudCount = 0;
    
    // Loop through ALL pages
    for (let page = 1; page <= totalPages.value; page++) {
      if (fraudStore.cancelAnalysis) {
        console.log('⏹️ Analysis stopped by user');
        fraudAnalysisTitle.value = 'Analysis Stopped';
        fraudAnalysisMessage.value = `Stopped at page ${page}/${totalPages.value}`;
        await new Promise(resolve => setTimeout(resolve, 2000));
        break;
      }
      
      const skip = (page - 1) * pageSize.value;
      console.log(`📄 Analyzing page ${page}/${totalPages.value}...`);
      
      // Update progress and message (single consistent message per page)
      fraudAnalysisProgress.value = Math.round(((page - 1) / totalPages.value) * 100);
      fraudAnalysisMessage.value = `Processing page ${page}/${totalPages.value}...`;
      
      // Fetch page data (will use backend cache on subsequent requests)
      await reportStore.fetchReportData(pipelineCache.value, pageSize.value, skip);
      const pageData = reportStore.data;
      
      if (pageData.length === 0) continue;
      
      // Analyze this page (don't update message again to prevent flicker)
      const result = await aiStore.analyzeFraudInData(
        pageData,
        mappings,
        page === 1,  // isFirstChunk only for first page
        totalRecords.value
      );
      
      if (result?.cancelled) {
        console.log('⏹️ Analysis cancelled');
        fraudAnalysisTitle.value = 'Analysis Cancelled';
        fraudAnalysisMessage.value = `Cancelled at page ${page}/${totalPages.value}`;
        await new Promise(resolve => setTimeout(resolve, 2000));
        break;
      }
      
      // Extract fraud IDs from this page and store minimal data
      const fraudIds = aiStore.getFraudulentIds();
      const pageStartCount = totalFraudCount;
      fraudIds.forEach(id => {
        const metadata = aiStore.getFraudMetadata(id);
        if (metadata && !fraudIdMap.has(id)) {
          fraudIdMap.set(id, {
            fraudType: metadata.fraudTypes.join(', '),
            explanation: metadata.explanation,
            severity: metadata.severity,
            confidence: metadata.confidence,
            relevantFields: metadata.relevantFields
          });
          totalFraudCount++;
        }
      });
      
      const fraudFoundOnPage = totalFraudCount - pageStartCount;
      fraudAnalysisFraudCount.value = totalFraudCount;
      
      // Update completion message (removed to prevent flickering)
      // Message will update when next page starts loading
    }
    
    fraudRules.value = fraudStore.fraudRulesCache || [];
    fraudHighlightsAvailable.value = true;
    
    // Final update
    fraudAnalysisProgress.value = 100;
    fraudAnalysisTitle.value = totalFraudCount > 0 ? 'Fraud Detection Complete!' : 'Analysis Complete';
    fraudAnalysisMessage.value = `Reloading page to apply fraud highlights...`;
    
    // Close modal BEFORE reloading to prevent overlap
    await new Promise(resolve => setTimeout(resolve, 1000));
    showFraudModal.value = false;
    
    // Reload current page to apply highlighting
    await loadPage(currentPage.value);
    
    // Modal already closed above before reload
    
  } catch (error: any) {
    console.error('Error analyzing fraud:', error);
    fraudAnalysisTitle.value = 'Analysis Failed';
    fraudAnalysisMessage.value = `Error: ${error.message}`;
    await new Promise(resolve => setTimeout(resolve, 3000));
    showFraudModal.value = false;
  } finally {
    console.log('🔒 Closing fraud modal');
    fraudStore.loadingFraudAnalysis = false;
    fraudStore._stopTimer();
    await nextTick();
  }
}

async function clearFraudAnalysis() {
  fraudHighlightsAvailable.value = false;
  showFraudLegend.value = false;
  showOnlyFraud.value = false;
  fraudRules.value = [];
  fraudIdMap.clear();
  if (fraudStore.fraudMetadataMap) {
    fraudStore.fraudMetadataMap.clear();
  }
  aiStore.resetAnalysisState();
  
  // Reload current page to remove highlights (don't show load time)
  shouldUpdateLoadTime.value = false;
  await loadPage(currentPage.value);
}

function filterFraudRows() {
  // Re-render current page with filtered data (don't show load time)
  shouldUpdateLoadTime.value = false;
  loadPage(currentPage.value);
}

function stopFraudAnalysis() {
  aiStore.stopFraudAnalysis();
  showFraudModal.value = false;
  
  // Keep partial results if any
  if (fraudStore.fraudMetadataMap.size > 0) {
    // Build fraud ID map from partial results
    fraudIdMap.clear();
    const fraudIds = aiStore.getFraudulentIds();
    fraudIds.forEach(id => {
      const metadata = aiStore.getFraudMetadata(id);
      if (metadata) {
        fraudIdMap.set(id, {
          fraudType: metadata.fraudTypes.join(', '),
          explanation: metadata.explanation,
          severity: metadata.severity,
          confidence: metadata.confidence,
          relevantFields: metadata.relevantFields
        });
      }
    });
    
    fraudRules.value = fraudStore.fraudRulesCache || [];
    fraudHighlightsAvailable.value = true;
    loadPage(currentPage.value);
  }
}
/* -------------------------------------------------------------------------- */

function createFormattedColumns(data: Record<string, any>[]) {
  const columns: ColDef[] = [];
  
  // Add checkbox selection column first
  columns.push({
    headerCheckboxSelection: true,
    checkboxSelection: true,
    width: 50,
    minWidth: 50,
    maxWidth: 50,
    pinned: 'left',
    lockPosition: true,
    filter: false,
  });
  
  const rules = query.value?.formattingRules || [];
  
  // Only process selected fields
  const fieldsToProcess = (props.tab?.selectedFields || [])
    .filter((f: any) => f.visible !== false && f.name !== '_id' && f.alias !== '_id');
  
  const isLargeDataset = data.length > 200;
  
  fieldsToProcess.forEach((field: any, index: number) => {
    const isCalculated = field.isCalculated ?? field.IsCalculated;
    
    // Use the mapping name for data lookup (matches projection key)
    const mapping = findMappingByAliasOrName(field.name) ?? findMappingByAliasOrName(field.alias);
    const mappingName = mapping ? (mapping.name ?? mapping.Name) : field.name;
    
    // For title: use field alias, or fall back to mapping alias, or finally use field name
    let fieldTitle = field.alias?.trim();
    if (!fieldTitle) {
      fieldTitle = mapping?.alias || mapping?.Alias || field.name;
    }
    
    // For the field key (used to lookup data in row), use the alias which matches the projection key
    const fieldAlias = field.alias?.trim() || mapping?.alias || mapping?.Alias || mappingName;
    const fieldKey = fieldAlias;

    if (isCalculated) {
      const expression = field.expression || field.Expression;
      columns.push({
        headerName: field.alias?.trim() || `Calc ${index + 1}`,
        field: `__calc_${index}`,
        width: 150,
        minWidth: 100,
        valueGetter: (params: any) => {
          const row = params.data;
          if (!row) return '';
          
          try {
            const expr = expression.replace(/\b(\w+)\b/g, (m: any) => {
              if (Object.prototype.hasOwnProperty.call(row, m)) {
                return row[m] === undefined || row[m] === null ? '""' : `row["${m}"]`;
              }
              return m;
            });
            const result = eval(expr);
            
            if (result === undefined || result === null) return '';
            
            return `${field.prefix || ''}${typeof result === 'number' ? result.toFixed(2) : result}${field.suffix || ''}`;
          } catch {
            return 'ERR';
          }
        }
      });
    } else {
      // Use cached mapping lookup
      const mapping = findMappingByAliasOrName(field.name) ?? findMappingByAliasOrName(field.alias);
      const dataType = mapping?.DataType ?? mapping?.dataType;

      if (dataType === 'Boolean') {
        columns.push({
          headerName: fieldTitle,
          field: fieldKey,
          width: 80,
          minWidth: 60,
          cellRenderer: (params: any) => {
            const value = params.value;
            const bool = value === true || value === "true";
            
            return `<div style="font-size: 1.2em; color: ${bool ? 'green' : 'red'}; text-align: center;">${bool ? "✔" : "✖"}</div>`;
          }
        });
      } else {
        // Determine width based on data type
        let width = 150; // default
        let minWidth = 100;
        
        if (dataType === 'Date' || dataType === 'DateTime') {
          width = 180;
          minWidth = 150;
        } else if (dataType === 'String' || dataType === 'string') {
          width = 200;
          minWidth = 120;
        } else if (dataType === 'Decimal' || dataType === 'Decimal128' || dataType === 'Double' || dataType === 'Int32' || dataType === 'Int64') {
          width = 120;
          minWidth = 80;
        } else if (fieldKey === '_id' || fieldKey.includes('ID') || fieldKey.includes('Id')) {
          width = 220;
          minWidth = 180;
        }
        
        const colDef: ColDef = {
          headerName: fieldTitle,
          field: fieldKey,
          width: width,
          minWidth: minWidth,
        };

        // Add cell renderer for formatted values
        if (!isLargeDataset || rules.length > 0) {
          colDef.cellRenderer = (params: any) => {
            const raw = params.value;
            const value = displayify(raw);
            
            return `${field.prefix || ''}${value}${field.suffix || ''}`;
          };
          
          // Add cell style for formatting rules
          if (rules.length > 0) {
            colDef.cellStyle = (params: any) => {
              const value = displayify(params.value);
              const match = rules.find((rule: any) =>
                rule.field === field.name && rule.operator === '$eq' && rule.value == value
              );
              if (match) {
                return { backgroundColor: match.color };
              }
              return null;
            };
          }
        }
        
        columns.push(colDef);
      }
    }
  });
  
  return columns;
}

// Cache columns to avoid rebuilding on pagination
let cachedColumnDefs: ColDef[] = [];
let lastSelectedFieldsHash = '';

async function loadPage(page: number) {
  const skip = (page - 1) * pageSize.value;
  
  await reportStore.fetchReportData(pipelineCache.value, pageSize.value, skip);
  
  // Update pagination
  totalRecords.value = reportStore.total;
  totalPages.value = Math.max(1, Math.ceil(reportStore.total / pageSize.value));
  currentPageData.value = reportStore.data;

  if (!gridApi) return;

  // Filter fraud if enabled
  const displayData = showOnlyFraud.value 
    ? reportStore.data.filter((row: any) => fraudIdMap.has(String(row._id || '')))
    : reportStore.data;

  if (displayData.length === 0) {
    rowData.value = [];
    return;
  }

  // Only rebuild columns if fields changed
  const currentFieldsHash = JSON.stringify(props.tab?.selectedFields?.map((f: any) => f.name));
  if (cachedColumnDefs.length === 0 || currentFieldsHash !== lastSelectedFieldsHash) {
    cachedColumnDefs = createFormattedColumns(displayData);
    lastSelectedFieldsHash = currentFieldsHash;
    columnDefs.value = cachedColumnDefs;
  }
  
  // Update row data - AG Grid's virtual DOM handles the rest efficiently
  // Fraud highlighting is handled automatically via getRowStyle function
  rowData.value = displayData;
}

async function previousPage() {
  if (currentPage.value > 1 && pipelineCache.value.length > 0) {
    currentPage.value--;
    shouldUpdateLoadTime.value = true; // Show load time for pagination
    await loadPage(currentPage.value);
  }
}

async function nextPage() {
  if (currentPage.value < totalPages.value && pipelineCache.value.length > 0) {
    currentPage.value++;
    shouldUpdateLoadTime.value = true; // Show load time for pagination
    await loadPage(currentPage.value);
  }
}

async function changePageSize() {
  // Only reload if we have a pipeline (query has been run)
  if (pipelineCache.value.length === 0) return;
  
  currentPage.value = 1;
  totalPages.value = Math.max(1, Math.ceil(totalRecords.value / pageSize.value));
  shouldUpdateLoadTime.value = true; // Show load time for page size change
  await loadPage(currentPage.value);
}

function changeSortField() {
  // Reset to first page when sort changes
  currentPage.value = 1;
  // Re-run the query to apply new sort
  submitQuery();
}

function toDateTimeLocal(dateString: string | number | Date) {
  if (!dateString) return '';
  try {
    const date = new Date(dateString);
    return isNaN(date.getTime()) ? dateString : date.toISOString().slice(0, 16);
  } catch {
    return dateString;
  }
}

/**
 * Recursively adjust field paths in a $match object after $unwind operations.
 * E.g., if "Tender" was unwound, "Tender.Amount" becomes "Amount"
 */
function adjustMatchFieldPaths(match: any, unwindRoots: Set<string>): any {
  if (!match || typeof match !== 'object') return match;
  
  // Handle logical operators ($and, $or, $nor)
  if (match.$and) {
    return { $and: match.$and.map((m: any) => adjustMatchFieldPaths(m, unwindRoots)) };
  }
  if (match.$or) {
    return { $or: match.$or.map((m: any) => adjustMatchFieldPaths(m, unwindRoots)) };
  }
  if (match.$nor) {
    return { $nor: match.$nor.map((m: any) => adjustMatchFieldPaths(m, unwindRoots)) };
  }
  
  // Adjust field paths
  const adjusted: any = {};
  for (const [fieldPath, condition] of Object.entries(match)) {
    // Check if this field's root was unwound
    if (fieldPath.includes('.')) {
      const root = fieldPath.split('.')[0];
      if (unwindRoots.has(root)) {
        // Use the last segment (flattened name)
        const flattenedField = fieldPath.split('.').pop()!;
        adjusted[flattenedField] = condition;
        continue;
      }
    }
    // Keep original field path
    adjusted[fieldPath] = condition;
  }
  
  return adjusted;
}

/**
 * Convert numeric values in $match conditions to proper types for MongoDB
 */
function convertDecimalComparisons(match: any): any {
  if (!match || typeof match !== 'object') return match;
  
  // Handle logical operators recursively
  if (match.$and) {
    return { $and: match.$and.map((m: any) => convertDecimalComparisons(m)) };
  }
  if (match.$or) {
    return { $or: match.$or.map((m: any) => convertDecimalComparisons(m)) };
  }
  if (match.$nor) {
    return { $nor: match.$nor.map((m: any) => convertDecimalComparisons(m)) };
  }
  
  const converted: any = {};
  for (const [fieldPath, condition] of Object.entries(match)) {
    // Find the mapping for this field
    const mapping = findMappingByAliasOrName(fieldPath);
    const dt = dtOf(mapping);
    
    if ((isDecimalType(dt) || isDoubleType(dt) || isIntType(dt)) && typeof condition === 'object' && condition !== null) {
      // For numeric comparisons, use plain numbers (MongoDB handles the type conversion)
      const convertedCondition: any = {};
      for (const [op, val] of Object.entries(condition)) {
        if (['$gt', '$gte', '$lt', '$lte', '$eq', '$ne'].includes(op)) {
          // Convert to plain number
          const numVal = Number(val);
          convertedCondition[op] = Number.isNaN(numVal) ? val : numVal;
        } else if (['$in', '$nin'].includes(op) && Array.isArray(val)) {
          // Convert array elements to numbers
          convertedCondition[op] = val.map((v: any) => {
            const numVal = Number(v);
            return Number.isNaN(numVal) ? v : numVal;
          });
        } else {
          convertedCondition[op] = val;
        }
      }
      converted[fieldPath] = convertedCondition;
    } else {
      converted[fieldPath] = condition;
    }
  }
  
  return converted;
}

async function submitQuery() {
  // Validate before proceeding
  if (!selectedFields.value || selectedFields.value.length === 0) {
    console.warn('⚠️ No fields selected, cannot run query');
    snackbarMessage.value = 'Please select at least one field before running the query.';
    snackbarColor.value = 'warning';
    snackbarTimeout.value = 3000;
    showSnackbar.value = true;
    return;
  }

  if (!isValid.value) {
    console.warn('⚠️ Query validation failed');
    snackbarMessage.value = 'Query is incomplete. Please check your conditions.';
    snackbarColor.value = 'warning';
    snackbarTimeout.value = 3000;
    showSnackbar.value = true;
    return;
  }

  isSubmittingQuery.value = true;
  
  try {
    const pipeline: any[] = [];

    // ---------- UNWIND: process selected fields AND condition fields ----------
    const unwindRoots = new Set<string>();
  
  // Check selected fields (silently - no excessive logging)
  selectedFields.value?.forEach((field: any) => {
    const m = findMappingByAliasOrName(field?.alias) ?? findMappingByAliasOrName(field?.name);
    const path = (m?.name ?? m?.Name) || field?.name || field?.alias;
    
    if (path && needsUnwindForPath(path, m)) {
      const root = path.split('.')[0];
      unwindRoots.add(root);
    }
  });
  
  // Also check condition fields to ensure they're unwound
  query.value?.conditions?.forEach((condition: any) => {
    const m = findMappingByAliasOrName(condition.field);
    const path = m?.name ?? m?.Name ?? condition.field;
    if (path && needsUnwindForPath(path, m)) {
      const root = path.split('.')[0];
      unwindRoots.add(root);
    }
  });
  
  if (unwindRoots.size > 0) {
    console.log('📦 Unwind roots:', Array.from(unwindRoots));
  }
  
  // ---------- $match (BEFORE $unwind for root-level fields) ----------
  const userLock = getUserLockFromToken(mappingsStore.mappings);
  let match = generateSimpleMatch(mappingsStore, query.value, userLock);
  
  // Convert numeric values to Decimal128 format for Decimal fields
  if (match) {
    match = convertDecimalComparisons(match);
  }
  
  // OPTIMIZATION: Add match BEFORE unwind when possible to filter documents early
  // MongoDB can use indexes on the original collection structure before unwinding
  let preUnwindMatch: any = null;
  let postUnwindMatch: any = null;
  
  if (match && Object.keys(match).length > 0) {
    // For optimal performance, put the match BEFORE $unwind
    // We'll add it again AFTER $unwind with adjusted paths
    preUnwindMatch = match;
    
    // Post-unwind match uses the same paths as pre-unwind.
    // After $unwind, "LineItem.UnitPrice" remains "LineItem.UnitPrice" — no flattening needed.
    postUnwindMatch = match;
  }
  
  // OPTIMIZED PIPELINE ORDER FOR PERFORMANCE:
  // 1. $match (pre-unwind) - filter as early as possible
  if (preUnwindMatch) {
    pipeline.push({ $match: preUnwindMatch });
  }
  
  // 2. $unwind - expand arrays only for filtered documents
  unwindRoots.forEach(root => pipeline.push({ 
    $unwind: { 
      path: `$${root}`,
      preserveNullAndEmptyArrays: true 
    } 
  }));
  
  // 3. $match (post-unwind) - filter on unwound fields
  if (postUnwindMatch) {
    pipeline.push({ $match: postUnwindMatch });
  }

  // 4. $sort - sort only the filtered results
  const sortSpec: any = {};
  sortSpec[sortField.value] = sortDirection.value;
  if (sortField.value !== '_id') {
    sortSpec['_id'] = 1; // Secondary sort by _id for consistency
  }
  pipeline.push({ $sort: sortSpec });

  // 5. $project - Map field names to aliases for frontend consumption
  // This ensures the grid columns match the data keys
  const projection: any = { _id: 1 }; // Always include _id
  
  // Build a map of unwound roots to their order (earlier unwinds take precedence)
  const unwoundArrays = Array.from(unwindRoots);
  
  selectedFields.value?.forEach((field: any) => {
    if (field.visible === false) return; // Skip hidden fields
    
    if (field.isCalculated || field.IsCalculated) {
      // Calculated fields are computed on frontend, skip in projection
      return;
    }
    
    const mapping = findMappingByAliasOrName(field.name) ?? findMappingByAliasOrName(field.alias);
    const originalSourcePath = mapping ? (mapping.name ?? mapping.Name) : field.name;
    let sourcePath = originalSourcePath;
    const alias = field.alias?.trim() || mapping?.alias || mapping?.Alias || sourcePath;
    const dt = dtOf(mapping);
    
    // CRITICAL: Determine if this field belongs to an unwound array
    // After $unwind, we must reference fields using the full dotted path
    if (unwindRoots.size > 0) {
      for (const root of unwoundArrays) {
        // Case 1: Already dotted notation (e.g., "LineItem.Barcode")
        if (originalSourcePath.startsWith(root + '.')) {
          // Keep as-is, it's already correct
          sourcePath = originalSourcePath;
          break;
        }
        // Case 2: Prefixed naming convention (e.g., "LineItemBarcode" where root is "LineItem")
        const lowerSource = originalSourcePath.toLowerCase();
        const lowerRoot = root.toLowerCase();
        if (lowerSource.startsWith(lowerRoot) && 
            originalSourcePath.length > root.length &&
            originalSourcePath !== root) {
          const remainder = originalSourcePath.substring(root.length);
          // Ensure remainder doesn't match another unwound array
          const isValidRemainder = !unwoundArrays.some(otherRoot => 
            remainder.toLowerCase().startsWith(otherRoot.toLowerCase())
          );
          if (isValidRemainder) {
            // Convert prefixed name to dotted path for MongoDB after $unwind
            // "LineItemBarcode" becomes "LineItem.Barcode"
            sourcePath = `${root}.${remainder}`;
            break;
          }
        }
      }
    }
    
    // Skip if source path is empty after processing
    if (!sourcePath || sourcePath.trim() === '') {
      console.warn(`⚠️ Skipping field ${alias} - empty source path after processing`);
      return;
    }
    
    // For Decimal fields, convert to string to preserve precision
    const sourceExpr = `$${sourcePath}`;
    projection[alias] = isDecimalType(dt) ? { $toString: sourceExpr } : sourceExpr;
  });
  
  if (Object.keys(projection).length > 1) { // More than just _id
    pipeline.push({ $project: projection });
  }

  // Populate sort field options from selected fields only
  sortFieldOptions.value = ['_id'];
  selectedFields.value?.forEach((field: any) => {
    const m = findMappingByAliasOrName(field.name) ?? findMappingByAliasOrName(field.alias);
    const fieldName = m ? (m.name ?? m.Name) : field.name;
    if (fieldName && fieldName !== '_id' && !sortFieldOptions.value.includes(fieldName)) {
      sortFieldOptions.value.push(fieldName);
    }
  });

  submittedQuery.value = pipeline;
  pipelineCache.value = pipeline;

  // Clear fraud data when new query runs
  fraudIdMap.clear();
  fraudHighlightsAvailable.value = false;
  if (fraudStore.fraudMetadataMap) {
    fraudStore.fraudMetadataMap.clear();
  }

  console.log('🔍 Submitting query with', pipeline.length, 'stages');
  console.log('📋 Pipeline:', JSON.stringify(pipeline, null, 2));

  shouldUpdateLoadTime.value = true; // Enable load time display for query execution
  await nextTick();
  await loadPage(currentPage.value);
  
  isSubmittingQuery.value = false;
  } catch (err: any) {
    console.error('Error in submitQuery:', err);
    snackbarMessage.value = `Error: ${err.message}`;
    snackbarColor.value = 'error';
    showSnackbar.value = true;
  } finally {
    isSubmittingQuery.value = false;
  }
}


defineExpose({ submitQuery });

/* ------------------------------- exports ---------------------------------- */
const exportToCSV = (data: any[], filename = 'data.csv') => {
  if (!data || data.length === 0) {
    alert('No data to export');
    return;
  }
  
  // Clean data to remove undefined values
  const cleanData = data.map(row => {
    const cleanRow: any = {};
    Object.keys(row).forEach(key => {
      const val = row[key];
      cleanRow[key] = (val === undefined || val === null) ? '' : val;
    });
    return cleanRow;
  });
  
  const csv = Papa.unparse(cleanData);
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.setAttribute('download', filename);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

const exportToPDF = (data: any[], filename = 'data.pdf', orientation: 'portrait' | 'landscape' = 'portrait') => {
  if (!data || data.length === 0) {
    alert('No data to export');
    return;
  }
  
  const headers = Object.keys(data[0] || {});
  
  // Calculate available page width based on orientation
  // A4 page dimensions: 595.28 x 841.89 points
  // Subtract margins (40 points each side)
  const pageWidth = orientation === 'landscape' ? 841.89 - 80 : 595.28 - 80;
  const columnWidth = pageWidth / headers.length;
  
  const docDefinition: any = {
    pageOrientation: orientation,
    pageSize: 'A4',
    pageMargins: [40, 60, 40, 40],
    content: [
      { text: 'Exported Data', style: 'header' },
      { 
        table: { 
          headerRows: 1, 
          widths: Array(headers.length).fill(columnWidth),
          body: [ 
            headers.map(h => ({ text: h, style: 'tableHeader', fontSize: 8 })), 
            ...data.map((row: any) => headers.map(h => {
              const val = row[h];
              // Convert undefined/null to empty string, convert objects to JSON string
              let cellValue = '';
              if (val === undefined || val === null) cellValue = '';
              else if (typeof val === 'object') cellValue = JSON.stringify(val);
              else cellValue = String(val);
              
              return { text: cellValue, fontSize: 7 };
            }))
          ] 
        },
        layout: {
          fillColor: (rowIndex: number) => (rowIndex === 0) ? '#CCCCCC' : null,
          hLineWidth: () => 0.5,
          vLineWidth: () => 0.5
        }
      }
    ],
    styles: { 
      header: { fontSize: 16, bold: true, margin: [0, 0, 0, 10] },
      tableHeader: { bold: true, fontSize: 8, color: 'black' }
    }
  };
  pdfMake.createPdf(docDefinition).download(filename);
};

const exportToXLSX = (data: any[], filename = 'data.xlsx') => {
  if (!data || data.length === 0) {
    alert('No data to export');
    return;
  }
  
  // Clean data to remove undefined values
  const cleanData = data.map(row => {
    const cleanRow: any = {};
    Object.keys(row).forEach(key => {
      const val = row[key];
      cleanRow[key] = (val === undefined || val === null) ? '' : val;
    });
    return cleanRow;
  });
  
  const worksheet = XLSX.utils.json_to_sheet(cleanData);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');
  XLSX.writeFile(workbook, filename);
};

const exportCells = (format: string, orientation: 'portrait' | 'landscape' = 'portrait') => {
  if (!gridApi) { alert('Grid not initialized'); return; }
  const selectedNodes = gridApi.getSelectedNodes();
  if (!selectedNodes || selectedNodes.length === 0) {
    alert('No rows selected. Please select rows by clicking on them first.');
    return;
  }
  const selectedData = selectedNodes.map((node: any) => node.data);
  const filename = `selected_cells.${format}`;
  if (format === 'csv') exportToCSV(selectedData, filename);
  else if (format === 'pdf') exportToPDF(selectedData, filename, orientation);
  else if (format === 'xlsx') exportToXLSX(selectedData, filename);
};

const exportPage = (format: string, orientation: 'portrait' | 'landscape' = 'portrait') => {
  if (!rowData.value) { alert('No data on current page'); return; }
  const pageData = rowData.value;
  if (!pageData.length) { alert('No data on current page'); return; }
  const filename = `page_${currentPage.value}.${format}`;
  if (format === 'csv') exportToCSV(pageData, filename);
  else if (format === 'pdf') exportToPDF(pageData, filename, orientation);
  else exportToXLSX(pageData, filename);
};

const exportAllPages = async (format: string, orientation: 'portrait' | 'landscape' = 'portrait') => {
  // Show loading message
  snackbarMessage.value = 'Fetching all records for export...';
  snackbarColor.value = 'info';
  showSnackbar.value = true;
  
  try {
    // Fetch with a very large page size and skip=0 to get all records
    await reportStore.fetchReportData(pipelineCache.value, totalRecords.value || 999999, 0);
    const allData = reportStore.data;
    
    if (!allData.length) { 
      alert('No data available'); 
      return; 
    }
    
    const filename = `all_pages_${totalRecords.value}_records.${format}`;
    if (format === 'csv') exportToCSV(allData, filename);
    else if (format === 'pdf') exportToPDF(allData, filename, orientation);
    else if (format === 'xlsx') exportToXLSX(allData, filename);
    
    snackbarMessage.value = `Successfully exported ${allData.length} records`;
    snackbarColor.value = 'success';
    showSnackbar.value = true;
  } catch (error: any) {
    snackbarMessage.value = `Export failed: ${error.message}`;
    snackbarColor.value = 'error';
    showSnackbar.value = true;
  }
};

const exportFraudPage = (format: string, orientation: 'portrait' | 'landscape' = 'portrait') => {
  if (!rowData.value) { alert('No data on current page'); return; }
  
  // Get current page data and filter for fraud cases
  const pageData = rowData.value;
  const fraudData = pageData.filter((row: any) => {
    const id = String(row._id || '');
    return fraudIdMap.has(id);
  });
  
  if (!fraudData.length) { 
    alert('No fraud cases on current page'); 
    return; 
  }
  
  // Enrich with fraud details from fraudMetadataMap
  const enrichedData = fraudData.map((row: any) => {
    const id = String(row._id || '');
    const fraudInfo = fraudIdMap.get(id);
    return {
      ...row,
      FRAUD_TYPE: fraudInfo?.fraudType || 'Unknown',
      FRAUD_SEVERITY: fraudInfo?.severity || 'medium',
      FRAUD_CONFIDENCE: fraudInfo?.confidence || 0,
      FRAUD_EXPLANATION: fraudInfo?.explanation || ''
    };
  });
  
  const filename = `fraud_cases_page_${currentPage.value}.${format}`;
  if (format === 'csv') exportToCSV(enrichedData, filename);
  else if (format === 'pdf') exportToPDF(enrichedData, filename, orientation);
  else exportToXLSX(enrichedData, filename);
};

const exportAllFraudPages = async (format: string, orientation: 'portrait' | 'landscape' = 'portrait') => {
  if (!fraudIdMap.size) { 
    alert('No fraud cases detected on current page.\n\nNote: Fraud detection works page-by-page. Navigate to other pages and run detection there too.'); 
    return; 
  }
  
  // Export fraud from current page only (since detection is per-page)
  exportFraudPage(format, orientation);
};
/* -------------------------------------------------------------------------- */

// AG Grid ready callback
function onGridReady(params: GridReadyEvent) {
  gridApi = params.api;
  console.log('✅ AG Grid initialized successfully');
}
</script>

<style scoped>
/* Results Toolbar */
.results-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 0;
  margin-bottom: 6px;
  border-bottom: 1px solid #eee;
}

.toolbar-group {
  display: flex;
  align-items: center;
  gap: 4px;
}

.page-info {
  font-size: 12px;
  color: #666;
  margin-left: 8px;
  white-space: nowrap;
}

/* Compact Select */
.compact-select :deep(.v-field) {
  font-size: 12px;
}

.compact-select :deep(.v-field__input) {
  padding: 2px 6px;
  min-height: 28px;
  font-size: 12px;
}

.compact-select :deep(.v-field--variant-outlined) {
  --v-field-border-opacity: 0.15;
}

.compact-select :deep(.v-select .v-field__append-inner) {
  padding-top: 0;
  padding-inline-start: 0;
}

.compact-select :deep(.v-select .v-field__append-inner .v-icon) {
  font-size: 16px;
}

/* Action Toolbar */
.action-toolbar {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 0;
  margin-bottom: 6px;
}

/* Custom Context Menu */
.context-menu-card {
  min-width: 200px;
  box-shadow: 0 4px 8px rgba(0,0,0,0.2) !important;
  font-size: 12px;
}

.context-menu-card :deep(.v-list-item-title) {
  font-size: 12px;
}

.context-menu-card :deep(.v-list-subheader) {
  font-size: 11px;
}

.compact-switch {
  margin-left: 8px;
}

.compact-switch :deep(.v-label) {
  font-size: 12px;
}

.compact-switch :deep(.v-selection-control__wrapper) {
  height: 18px;
  width: 32px;
}

/* Compact Alert */
.compact-alert {
  margin-bottom: 8px;
  padding: 6px 12px;
  font-size: 12px;
}

.compact-alert :deep(.v-alert__content) {
  font-size: 12px;
}

/* Fraud Legend */
.fraud-legend {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 6px 12px;
  background: #f9f9f9;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  margin-bottom: 8px;
  font-size: 11px;
}

.legend-title {
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.legend-items {
  display: flex;
  gap: 16px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.legend-color {
  width: 16px;
  height: 12px;
  border-radius: 2px;
  border: 1px solid #ccc;
}

.legend-hint {
  color: #888;
  font-style: italic;
  margin-left: auto;
}

/* Results Grid Wrapper (for overlay containment) */
.results-grid-wrapper {
  position: relative;
  min-height: 300px;
}

/* Table Container */
.table-container {
  height: 500px;
  margin-top: 4px;
  overflow: auto;
}

.table-footer {
  display: flex;
  align-items: center;
  padding: 4px 0;
  border-top: 1px solid #eee;
  margin-top: 4px;
}

.load-time {
  font-size: 11px;
  color: #888;
}

/* Pipeline Details */
.pipeline-details {
  margin-top: 12px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

/* AG Grid Custom Styles */
.results-grid-wrapper :deep(.ag-theme-quartz-auto-dark) {
  font-size: 12px;
}

.results-grid-wrapper :deep(.ag-header-cell-text) {
  font-weight: 600;
  font-size: 12px;
}

.results-grid-wrapper :deep(.ag-cell) {
  line-height: 32px;
}

.results-grid-wrapper :deep(.ag-row) {
  border-bottom: 1px solid #e0e0e0;
}

.pipeline-summary {
  padding: 6px 12px;
  background: #f5f5f5;
  font-size: 11px;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  cursor: pointer;
}

.pipeline-summary:hover {
  background: #eee;
}

.pipeline-code {
  padding: 8px 12px;
  margin: 0;
  font-size: 11px;
  font-family: 'Consolas', 'Monaco', monospace;
  background: #fafafa;
  overflow-x: auto;
  max-height: 200px;
}
</style>

