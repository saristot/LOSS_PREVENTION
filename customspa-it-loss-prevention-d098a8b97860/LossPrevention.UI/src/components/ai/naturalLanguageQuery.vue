<template>
  <v-container fluid class="pa-6">
    <!-- Natural Language Query Section -->
    <v-card class="mb-6" elevation="2">
      <v-card-title class="bg-primary text-white d-flex align-center">
        <v-icon class="mr-2" color="white">mdi-robot</v-icon>
        Natural Language Query
      </v-card-title>
      
      <v-card-text class="pa-4">
        <v-textarea
          v-model="questionNQL"
          label="Ask a natural language question..."
          placeholder="e.g., show me all transactions where the total is greater than 1000"
          variant="outlined"
          rows="2"
          auto-grow
          class="mb-3"
        />
        
        <v-btn 
          @click="generate" 
          color="primary" 
          prepend-icon="mdi-creation"
          :loading="loading"
          :disabled="!questionNQL.trim() || loading"
        >
          Generate Query
        </v-btn>
      </v-card-text>

      <!-- Query Results -->
      <v-card-text v-if="nlqResult && !loading" class="pt-0">
        <v-divider class="mb-4" />
        
        <div class="d-flex align-center justify-space-between mb-3">
          <div class="text-subtitle-2 text-grey-darken-2">
            Query Generated
          </div>
          <v-btn 
            color="primary" 
            variant="tonal"
            prepend-icon="mdi-plus-box"
            size="small"
            @click="addQueryToBuilder"
            :disabled="loading"
          >
            Add to Query Builder
          </v-btn>
        </div>

        <!-- Query Preview Card -->
        <v-card variant="outlined">
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1">
            {{ nlqResult.title }}
          </v-card-title>
          
          <v-card-text>
            <div class="mb-3">
              <div class="text-caption text-grey mb-1">Selected Fields:</div>
              <div class="d-flex flex-wrap gap-2">
                <v-chip 
                  v-for="field in nlqResult.selectedFields" 
                  :key="field.name"
                  size="small"
                  label
                  color="primary"
                  variant="tonal"
                  class="mr-2 mb-2"
                >
                  {{ field.name }}
                  <span v-if="field.aggregation" class="ml-1 text-caption">({{ field.aggregation }})</span>
                </v-chip>
              </div>
            </div>

            <div>
              <div class="text-caption text-grey mb-1">Query Conditions:</div>
              <v-chip 
                v-for="(condition, idx) in nlqResult.query.conditions" 
                :key="idx"
                size="small"
                class="mr-2 mb-2"
                label
              >
                {{ condition.field }} {{ condition.operator }} {{ condition.value }}
              </v-chip>
            </div>
          </v-card-text>
        </v-card>
      </v-card-text>
    </v-card>

    <!-- AI Fraud Detection Expert Section -->
    <v-card elevation="2">
      <v-card-title class="bg-blue-accent-2 text-white d-flex align-center">
        <v-icon class="mr-2" color="white">mdi-alert-circle</v-icon>
        AI Fraud Detection Expert
      </v-card-title>
      
      <v-card-text class="pa-4">
        <v-alert
          type="info"
          variant="tonal"
          class="mb-4"
          density="compact"
        >
          Click the button below to automatically generate 25 fraud detection reports based on common retail fraud patterns.
        </v-alert>
        
        <v-btn 
          @click="generateReports" 
          color="blue-accent-2" 
          prepend-icon="mdi-shield-search"
          size="large"
          :loading="loadingFraud"
          :disabled="loadingFraud"
        >
          Generate Fraud Reports
        </v-btn>

        <!-- Loading Progress -->
        <v-card v-if="loadingFraud" class="mt-4" variant="outlined">
          <v-card-text>
            <div class="d-flex align-center mb-3">
              <v-progress-circular indeterminate color="blue-accent-2" size="32" width="3" class="mr-3" />
              <div>
                <div class="text-subtitle-1 font-weight-medium">Generating fraud reports...</div>
                <div class="text-caption text-grey">This may take a minute</div>
              </div>
            </div>
            
            <v-progress-linear
              :model-value="batchProgress"
              color="blue-accent-2"
              height="8"
              rounded
              class="mb-2"
            />
            
            <div class="d-flex justify-space-between text-caption">
              <span>{{ batchStatus }}</span>
              <span>{{ elapsedSeconds }}s elapsed</span>
            </div>
          </v-card-text>
        </v-card>
      </v-card-text>

      <!-- Fraud Reports List -->
      <v-card-text v-if="fraudReports.length && !loadingFraud" class="pt-0">
        <v-divider class="mb-4" />
        
        <div class="d-flex align-center justify-space-between mb-3">
          <div class="text-subtitle-2 text-grey-darken-2">
            Generated Reports ({{ fraudReports.length }})
          </div>
          <v-btn 
            color="blue-accent-2" 
            variant="tonal"
            prepend-icon="mdi-playlist-plus"
            size="small"
            @click="addAllReportsToTabs"
            :disabled="loadingFraud || !fraudReports.length"
          >
            Add All to Query Builder
          </v-btn>
        </div>

        <v-select
          v-model="selectedReportTitle"
          :items="reportTitles"
          label="Select a fraud report to preview"
          variant="outlined"
          density="comfortable"
          clearable
          prepend-inner-icon="mdi-file-document"
        />

        <!-- Selected Report Preview -->
        <v-card v-if="selectedReport" class="mt-4" variant="outlined">
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1">
            {{ selectedReport.title }}
          </v-card-title>
          
          <v-card-text>
            <div class="mb-3">
              <div class="text-caption text-grey mb-1">Description:</div>
              <div class="text-body-2">{{ selectedReportDescription }}</div>
            </div>

            <div class="mb-3">
              <div class="text-caption text-grey mb-1">Selected Fields:</div>
              <div class="d-flex flex-wrap gap-2">
                <v-chip 
                  v-for="field in selectedReport.selectedFields" 
                  :key="field.name"
                  size="small"
                  label
                  color="blue-accent-2"
                  variant="tonal"
                  class="mr-2 mb-2"
                >
                  {{ field.name }}
                  <span v-if="field.aggregation" class="ml-1 text-caption">({{ field.aggregation }})</span>
                </v-chip>
              </div>
            </div>

            <div>
              <div class="text-caption text-grey mb-1">Query Conditions:</div>
              <v-chip 
                v-for="(condition, idx) in selectedReport.query.conditions" 
                :key="idx"
                size="small"
                class="mr-2 mb-2"
                label
              >
                {{ condition.field }} {{ condition.operator }} {{ condition.value }}
              </v-chip>
            </div>
          </v-card-text>

          <v-card-actions>
            <v-spacer />
            <v-btn 
              color="blue-accent-2" 
              variant="flat"
              prepend-icon="mdi-plus"
              @click="addReportToTabs"
              :disabled="loadingFraud"
            >
              Add to Query Builder
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useMappingStore } from '../../stores/mappingStore'
import { useAiStore } from '../../stores/aiStore'
import { storeToRefs } from 'pinia'

const props = defineProps({
  tabs: Object,
  tabCounter: Number
});


const mappingStore = useMappingStore()
const queryStore = useAiStore()

const {
  generatedQuery,
  generatedFraudQuery,
  queryResults,
  nlqResult,
  fraudReports,
  loading,
  loadingFraud,
  elapsedTimeMs,
  currentBatch,
  totalBatches
} = storeToRefs(queryStore)

const { mappings } = storeToRefs(mappingStore)

const questionNQL = ref('')
const questionFraud = ref('')
const selectedReportTitle = ref('')

const batchProgress = computed(() => {
  if (totalBatches.value === 0) return 0
  return (currentBatch.value / totalBatches.value) * 100
})

const batchStatus = computed(() => {
  if (currentBatch.value === 0 && totalBatches.value === 0) return ''
  return `Batch ${currentBatch.value} of ${totalBatches.value}`
})

const elapsedSeconds = computed(() => {
  return (elapsedTimeMs.value / 1000).toFixed(1)
})

const reportTitles = computed(() => fraudReports.value.map(r => r.title))

const selectedReport = computed(() => {
  return fraudReports.value.find(r => r.title === selectedReportTitle.value) ?? null
})

const selectedReportDescription = computed(() => {
  return selectedReport.value?.description ?? ''
})

const selectedReportPipeline = computed(() => {
  return selectedReport.value?.pipeline ?? []
})

onMounted(async () => {
  try {
    await mappingStore.fetchMappings()
  } catch (err) {
    console.error('❌ Failed to load mappings:', err)
  }
})

const generate = async () => {
  if (!mappings.value.length) {
    console.warn('No mappings available')
    return
  }
  
  queryStore.question = questionNQL.value
  await queryStore.generateQuery(mappings.value)
}

const addQueryToBuilder = () => {
  if (!props.tabs || !nlqResult.value) return

  const tabCounterValue = props.tabs.length
  const newId = `tab-${tabCounterValue + 1}`

  props.tabs.push({
    id: newId,
    title: nlqResult.value.title.substring(0, 50) || 'Generated Query',
    description: 'Generated from natural language query',
    selectedFields: nlqResult.value.selectedFields,
    query: nlqResult.value.query,
    designMode: true
  })
}

const generateReports = async () => {
  if (!mappings.value.length) {
    console.warn('No mappings available')
    return
  }
  
  queryStore.question = questionFraud.value
  await queryStore.generateFraudReports(mappings.value)
}

const addReportToTabs = () => {
  if (!selectedReport.value) return

  const tabCounterValue = props.tabs?.length;
  const newId = `tab-${tabCounterValue + 1}`

  props.tabs?.push({
    id: newId,
    title: selectedReport.value.title,
    description: selectedReport.value.description,
    selectedFields: selectedReport.value.selectedFields,
    query: selectedReport.value.query ?? { type: 'AND', conditions: [], id: '' },
    designMode: true
  })
}

const addAllReportsToTabs = () => {
  if (!props.tabs || !fraudReports.value.length) return

  const startingIndex = props.tabs.length

  fraudReports.value.forEach((report, index) => {
    props.tabs?.push({
      id: `tab-${startingIndex + index + 1}`,
      title: report.title,
      description: report.description,
      selectedFields: report.selectedFields,
      query: report.query ?? { type: 'AND', conditions: [], id: '' },
      designMode: true
    })
  })
}


</script>
