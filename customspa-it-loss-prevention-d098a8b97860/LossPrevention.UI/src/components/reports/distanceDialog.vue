<template>
  <v-dialog v-model:modelValue="dialogOpen" max-width="1200px" scrollable>
    <v-card>
      <v-card-title class="bg-primary">
        <div class="d-flex align-center">
          <v-icon class="mr-2">mdi-chart-scatter-plot</v-icon>
          Euclidean Distance Analysis
          <v-spacer></v-spacer>
          <v-btn icon="mdi-help-circle" variant="text" @click="showHelp = !showHelp" color="white"></v-btn>
          <v-btn icon="mdi-close" variant="text" @click="dialogOpen = false" color="white"></v-btn>
        </div>
      </v-card-title>
      
      <v-alert v-if="showHelp" type="info" variant="tonal" class="ma-4 mb-0" border="start">
        <v-expansion-panels variant="accordion" density="compact" class="mt-2">
          <v-expansion-panel>
            <v-expansion-panel-title class="text-body-2 py-2">
              <v-icon size="small" class="mr-2">mdi-information</v-icon><strong>Quick Help</strong>
            </v-expansion-panel-title>
            <v-expansion-panel-text class="text-body-2">
              Finds records most similar to your selection. Configure date range and key field below, then click "Run Analysis".
              <strong>Tip:</strong> Select 5-10 fields for best results. Include numeric fields for better accuracy.
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>
      </v-alert>

      <v-card-text class="pa-4">
        <v-container fluid>
          <!-- Configuration Section -->
          <v-row dense>
            <!-- Left Column: Field Selection & Date Range -->
            <v-col cols="12" md="6">
              <!-- Selected Fields Panel -->
              <v-card variant="outlined" class="pa-3 mb-3" color="grey-lighten-2">
                <div class="text-subtitle-2 mb-2 d-flex align-center text-black">
                  <v-icon size="small" class="mr-1" color="black">mdi-check-circle</v-icon>
                  Selected Fields ({{ validSelectedFields.length }})
                  <v-chip size="x-small" :color="validSelectedFields.length >= 3 ? 'success' : 'warning'" class="ml-2">
                    {{ validSelectedFields.length >= 3 ? 'Ready' : `Need ${3 - validSelectedFields.length} more` }}
                  </v-chip>
                </div>
                
                <div class="field-chips-container">
                  <v-chip v-for="f in validSelectedFields" :key="f.name" size="small" class="ma-1" 
                    :color="dateFields.includes(f.name) ? 'blue' : 'grey-darken-1'"
                    :prepend-icon="dateFields.includes(f.name) ? 'mdi-calendar' : 'mdi-database'">
                    <span class="text-caption">{{ getFieldDisplayName(f.name) }}: {{ formatValue(f.value) }}</span>
                  </v-chip>
                </div>
              </v-card>

              <!-- Date Range Panel -->
              <v-card variant="outlined" class="pa-3" color="grey-lighten-2">
                <div class="text-subtitle-2 mb-2 text-black">
                  <v-icon size="small" class="mr-1" color="black">mdi-calendar-range</v-icon>
                  Date Range
                </div>
                
                <v-btn-group density="compact" variant="outlined" divided class="mb-3">
                  <v-btn size="small" @click="setDateRange('day')">1 Day</v-btn>
                  <v-btn size="small" @click="setDateRange('week')">1 Week</v-btn>
                  <v-btn size="small" @click="setDateRange('month')">1 Month</v-btn>
                  <v-btn size="small" @click="setDateRange('6months')">6 Months</v-btn>
                  <v-btn size="small" @click="setDateRange('year')">1 Year</v-btn>
                </v-btn-group>

                <v-row dense>
                  <v-col cols="6">
                    <v-select v-model="startField" :items="dateFields" label="Start Field" density="compact" hide-details
                      variant="outlined" bg-color="white"
                      @update:modelValue="onStartFieldChange" />
                  </v-col>
                  <v-col cols="6">
                    <v-text-field v-model="startFieldValue" label="Start Date" type="datetime-local" 
                      density="compact" hide-details variant="outlined" bg-color="white" />
                  </v-col>
                </v-row>

                <v-row dense class="mt-2">
                  <v-col cols="6">
                    <v-select v-model="endField" :items="dateFields" label="End Field" density="compact" hide-details
                      variant="outlined" bg-color="white"
                      @update:modelValue="onEndFieldChange" />
                  </v-col>
                  <v-col cols="6">
                    <v-text-field v-model="endFieldValue" label="End Date" type="datetime-local" 
                      density="compact" hide-details variant="outlined" bg-color="white" />
                  </v-col>
                </v-row>
              </v-card>
            </v-col>

            <!-- Right Column: Legend Field & Run Button -->
            <v-col cols="12" md="6">
              <!-- Identification Field Panel -->
              <v-card variant="outlined" class="pa-3 mb-3" color="grey-lighten-2">
                <div class="text-subtitle-2 mb-2 text-black">
                  <v-icon size="small" class="mr-1" color="black">mdi-label</v-icon>
                  Identification Field
                </div>
                
                <v-select v-model="legendLabelField" :items="mappingsStore.mappings" item-title="alias" item-value="name"
                  label="Field to identify records" 
                  variant="outlined"
                  bg-color="white"
                  density="compact" 
                  hint="Used to label results (e.g., Customer Name, Transaction ID)" 
                  persistent-hint
                  prepend-inner-icon="mdi-tag" />
              </v-card>

              <!-- Validation & Run Button Card -->
              <v-card variant="outlined" class="pa-3" color="grey-lighten-2">
                <div v-if="!canSubmit" class="mb-3">
                  <v-alert type="warning" variant="tonal" density="compact" class="text-caption">
                    <div><strong>Missing:</strong> {{ getMissingRequirements() }}</div>
                  </v-alert>
                </div>

                <v-btn color="primary" :disabled="!canSubmit" :loading="store.loading" @click="submit" 
                  size="large" block prepend-icon="mdi-play">
                  Run Analysis
                </v-btn>

                <div v-if="distanceResults.length" class="mt-3 text-center">
                  <v-chip color="success" prepend-icon="mdi-check-circle">
                    Analysis complete in {{ store.duration }}ms
                  </v-chip>
                </div>
              </v-card>
            </v-col>
          </v-row>

          <!-- Error Display -->
          <v-row v-if="store.error" dense class="mt-2">
            <v-col>
              <v-alert type="error" variant="tonal" density="compact" closable>
                {{ store.error }}
              </v-alert>
            </v-col>
          </v-row>

          <!-- Results Section -->
          <template v-if="distanceResults.length">
            <v-divider class="my-4"></v-divider>
            
            <div class="text-h6 mb-3 text-black">
              <v-icon class="mr-2" color="black">mdi-chart-line</v-icon>
              Analysis Results
            </div>

            <v-tabs v-model="resultsTab" color="primary" density="compact">
              <v-tab value="chart">
                <v-icon start>mdi-chart-box</v-icon>
                Visual
              </v-tab>
              <v-tab value="table">
                <v-icon start>mdi-table</v-icon>
                Table
              </v-tab>
            </v-tabs>

            <v-window v-model="resultsTab" class="mt-4">
              <v-window-item value="chart">
                <RadarChart :results="distanceResults" :legend-label-field="legendLabelField" />
              </v-window-item>

              <v-window-item value="table">
                <DistanceTable :results="distanceResults" :mappings="mappingsStore.mappings" />
              </v-window-item>
            </v-window>
          </template>
        </v-container>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.field-chips-container {
  max-height: 120px;
  overflow-y: auto;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 4px;
  padding: 8px;
  background-color: #ffffff;
}

:deep(.v-messages__message) {
  color: black !important;
}
</style>


<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useReportDataStore } from '@/stores/reportDataStore'
import { useMappingStore } from '@/stores/mappingStore';
import RadarChart from './radarChart.vue'
import DistanceTable from './distanceTable.vue'

const mappingsStore = useMappingStore();

const resultsTab = ref('chart')

const props = defineProps<{
  open: boolean,
  selectedFields: { name: string; value: any }[],
  dateFields: string[]
}>()

const emit = defineEmits(['update:open'])

const dialogOpen = computed({
  get: () => props.open,
  set: (val) => emit('update:open', val)
})

const showHelp = ref(false)
const startField = ref('')
const endField = ref('')

const startFieldValue = ref('')
const endFieldValue = ref('')
const legendLabelField = ref('')

const store = useReportDataStore()
const distanceResults = computed(() => store.distanceResults)

// Filter out invalid fields (empty name or undefined)
const validSelectedFields = computed(() => 
  props.selectedFields.filter(f => f && f.name && f.name.trim() !== '')
)

const canSubmit = computed(() =>
  startField.value && endField.value && startFieldValue.value && endFieldValue.value && legendLabelField.value && validSelectedFields.value.length >= 3
)

function getMissingRequirements(): string {
  const missing: string[] = []
  if (validSelectedFields.value.length < 3) missing.push(`${3 - validSelectedFields.value.length} more fields`)
  if (!startField.value) missing.push('start date field')
  if (!endField.value) missing.push('end date field')
  if (!startFieldValue.value) missing.push('start date value')
  if (!endFieldValue.value) missing.push('end date value')
  if (!legendLabelField.value) missing.push('identification field')
  return missing.join(', ')
}

function formatValue(val: any): string {
  // Check for boolean first, before null/undefined check, since false is falsy
  if (typeof val === 'boolean') return val ? '✓' : '✗'
  if (val === null || val === undefined) return 'N/A'
  if (typeof val === 'object' && val instanceof Date) return val.toLocaleDateString()
  if (typeof val === 'object') return JSON.stringify(val).substring(0, 50) + '...'
  return String(val).substring(0, 50)
}

function getFieldDisplayName(fieldName: string): string {
  const mapping = mappingsStore.mappings.find((m: any) => m.name === fieldName || m.Name === fieldName)
  return mapping?.alias || mapping?.Alias || fieldName.split('.').pop() || fieldName
}

onMounted(async () => {
  await mappingsStore.fetchMappings();
});

// Update field value based on selectedFields
const onStartFieldChange = (fieldName: string) => {
  const match = validSelectedFields.value.find(f => f.name === fieldName)
  if (match && match.value) {
    startFieldValue.value = toLocalDateTime(match.value)
  }
}

const onEndFieldChange = (fieldName: string) => {
  const match = validSelectedFields.value.find(f => f.name === fieldName)
  if (match && match.value) {
    endFieldValue.value = toLocalDateTime(match.value)
  }
}

// Converts ISO string or date to yyyy-MM-ddTHH:mm (for input type="datetime-local")
function toLocalDateTime(val: any): string {
  const d = new Date(val)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

// Set date range based on quick selection
function setDateRange(range: 'day' | 'week' | 'month' | '6months' | 'year') {
  const now = new Date()
  const end = toLocalDateTime(now)
  
  const start = new Date()
  switch (range) {
    case 'day':
      start.setDate(start.getDate() - 1)
      break
    case 'week':
      start.setDate(start.getDate() - 7)
      break
    case 'month':
      start.setMonth(start.getMonth() - 1)
      break
    case '6months':
      start.setMonth(start.getMonth() - 6)
      break
    case 'year':
      start.setFullYear(start.getFullYear() - 1)
      break
  }
  
  startFieldValue.value = toLocalDateTime(start)
  endFieldValue.value = end
}

async function submit() {
  await store.fetchDistanceAnalysis({
    id: validSelectedFields.value.find(f => f.name === '_id')?.value || '',
    startDateField: startField.value,
    endDateField: endField.value,
    startDate: new Date(startFieldValue.value).toISOString(),
    endDate: new Date(endFieldValue.value).toISOString(),
    keyField:  legendLabelField.value,
    fields: validSelectedFields.value.map(f => f.name)
  })
}

</script>
