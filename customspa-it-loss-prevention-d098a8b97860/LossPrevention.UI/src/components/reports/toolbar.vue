<template>
  <v-tooltip text="Save" location="bottom">
    <template v-slot:activator="{ props: tooltipProps }">
      <v-btn icon size="x-small" @click="onSave" :color="isSaved ? 'default' : 'warning'" class="toolbar-btn" v-bind="tooltipProps">
        <v-icon size="small">mdi-content-save</v-icon>
      </v-btn>
    </template>
  </v-tooltip>

  <v-tooltip text="Copy Report" location="bottom">
    <template v-slot:activator="{ props: tooltipProps }">
      <v-btn icon size="x-small" @click="copyReport" class="toolbar-btn" v-bind="tooltipProps">
        <v-icon size="small">mdi-content-copy</v-icon>
      </v-btn>
    </template>
  </v-tooltip>

  <v-tooltip text="Show Chart" location="bottom">
    <template v-slot:activator="{ props: tooltipProps }">
      <v-btn icon size="x-small" @click="showChart" disabled class="toolbar-btn" v-bind="tooltipProps">
        <v-icon size="small">mdi-chart-bar</v-icon>
      </v-btn>
    </template>
  </v-tooltip>

  <v-tooltip text="AI Assistant" location="bottom">
    <template v-slot:activator="{ props: tooltipProps }">
      <v-btn icon size="x-small" @click="aiDialog = true" class="toolbar-btn" v-bind="tooltipProps">
        <v-icon size="small">mdi-robot</v-icon>
      </v-btn>
    </template>
  </v-tooltip>

  <v-tooltip text="Heat Map" location="bottom">
    <template v-slot:activator="{ props: tooltipProps }">
      <v-btn icon size="x-small" @click="heatMapDialog = true" :disabled="!props.canShowHeatmap" class="toolbar-btn" v-bind="tooltipProps">
        <v-icon size="small">mdi-fire</v-icon>
      </v-btn>
    </template>
  </v-tooltip>

  <!-- AI Natural Language Query Dialog -->
  <v-dialog v-model="aiDialog" persistent max-width="1000px" scrollable>
    <v-card>
      <v-card-title class="d-flex align-center justify-space-between bg-primary pa-4">
        <div class="d-flex align-center text-white">
          <v-icon class="mr-2 text-white">mdi-robot</v-icon>
          <span>AI Assistant</span>
        </div>
        <v-btn icon variant="text" @click="aiDialog = false" class="text-white">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>
      <v-card-text class="pa-0">
        <NqlAiComponent 
          :tabs="tabs" 
          :tabCounter="tabCounter" 
          @close-dialog="aiDialog = false"
          @switch-tab="handleSwitchTab"
        />
      </v-card-text>
    </v-card>
  </v-dialog>

  <!-- Heat Map Dialog -->
  <v-dialog v-model="heatMapDialog" persistent max-width="1200px">
    <v-card class="pa-3">
      <v-card-title>
        <v-icon class="mr-2">mdi-fire</v-icon>
        Heat Map
      </v-card-title>

      <v-row>
        <v-col cols="12" md="4">
          <v-select v-model="xAxis" :items="availableFields" item-title="label" item-value="value"
                    label="X Axis" return-object />
        </v-col>
        <v-col cols="12" md="4">
          <v-select v-model="yAxis" :items="availableFields" item-title="label" item-value="value"
                    label="Y Axis" return-object />
        </v-col>
        <v-col cols="12" md="4">
          <v-select v-model="metric" :items="numericFields" item-title="label" item-value="value"
                    label="Metric (e.g., Count or Sum)" return-object />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <template v-if="previewData.length > 0">
            <HeatmapPreview
              :data="previewData"
              :xLabels="xAxisLabels"
              :yLabels="yAxisLabels"
              :xLabelTitle="xAxis?.label"
              :yLabelTitle="yAxis?.label"
              :xAxisDef="xAxis"
              :yAxisDef="yAxis"
              :metricDef="metric"
              :capPercentile="0.98"
              :widenOnlyDomain="true"
              :showZerosAsEmpty="true"
              :enableDrilldown="true"
              @drill="onHeatmapDrill"
            />
          </template>
          <template v-else>
            <div class="text-center pa-4">
              <v-icon size="32" color="grey">mdi-information-outline</v-icon>
              <div class="mt-2">No results found</div>
            </div>
          </template>
        </v-col>
      </v-row>

      <v-card-actions>
        <v-btn color="primary"
               @click="generateHeatmap"
               :showLegend="true"
               legendLabel="Total"
               legendPosition="bottom"
               :disabled="!xAxis || !yAxis || !metric">
          Generate Heatmap
        </v-btn>

        <v-spacer />
        <v-btn @click="heatMapDialog = false">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from 'vue'
import NqlAiComponent from '../ai/naturalLanguageQuery.vue'
import HeatmapPreview from './heatmapPreview.vue'
import { generateSimpleMatch } from '../../helpers/queryUtils'
import { useMappingStore } from '@/stores/mappingStore'
import { useReportDataStore } from '@/stores/reportDataStore'
import { getUserLockFromToken } from '@/helpers/fieldLock'

const mappingsStore = useMappingStore()
const reportStore = useReportDataStore()

const aiDialog = ref(false)
const heatMapDialog = ref(false)

const previewData = ref<any[]>([])
const xAxis = ref<any|null>(null)
const yAxis = ref<any|null>(null)
const xAxisLabels = ref<any[]>([])
const yAxisLabels = ref<any[]>([])
const metric = ref<any|null>(null)

const props = defineProps<{
  tabs: any[],
  tabCounter: number, // 1-based
  isSaved: boolean,
  canShowHeatmap: boolean
}>()

const emit = defineEmits(['save', 'copy-tab', 'heatmap-drill', 'switch-tab'])

function onSave() { emit('save') }

function copyReport() {
  const index = Math.max(0, Number(props.tabCounter) - 1)
  emit('copy-tab', { index })
}

function showChart() { /* no-op */ }

async function generateHeatmap() {
  const tab = (props.tabs as any)?.[Number(props.tabCounter) - 1]
  if (!tab || !xAxis.value || !yAxis.value || !metric.value) return

  const matchStage = generateSimpleMatch(
    mappingsStore,
    tab.query,
    getUserLockFromToken(mappingsStore.mappings)
  )

  const pipeline: any[] = []
  if (matchStage) pipeline.push({ $match: matchStage })
  pipeline.push({
    $group: {
      _id: { x: `$${xAxis.value.value}`, y: `$${yAxis.value.value}` },
      value: { $sum: `$${metric.value.value}` }
    }
  })
  pipeline.push({ $project: { x: '$_id.x', y: '$_id.y', v: '$value', _id: 0 } })

  await reportStore.fetchReportData(pipeline, 100000, 0)
  const result = reportStore.data as any[]

  const valid = result?.filter(d => d.x !== undefined && d.y !== undefined && d.v !== undefined) || []
  previewData.value = valid
  xAxisLabels.value = [...new Set(valid.map(r => r.x))]
  yAxisLabels.value = [...new Set(valid.map(r => r.y))]
}

async function onHeatmapDrill(payload: any) {
  // close dialog first, then emit so parent can switch tabs cleanly
  heatMapDialog.value = false
  await nextTick()
  const index = Math.max(0, Number(props.tabCounter) - 1)
  emit('heatmap-drill', { index, payload })
}

function handleSwitchTab(tabId: string) {
  emit('switch-tab', tabId)
}

const allFields = computed(() => {
  const all = (props.tabs as any)?.flatMap((t: any) => t.selectedFields || []) || []
  return all.map((f: any) => ({
    label: f.alias || f.name,
    value: f.name,
    dataType: f.dataType
  }))
})

const availableFields = computed(() =>
  allFields.value.filter((f: any) =>
    ['string', 'date', 'datetime', 'timestamp'].includes((f.dataType || '').toLowerCase())
  )
)

const numericFields = computed(() =>
  allFields.value.filter((f: any) =>
    ['integer', 'decimal', 'float', 'double', 'long', 'number'].includes((f.dataType || '').toLowerCase())
  )
)
</script>

<style scoped>
.toolbar-btn {
  width: 26px !important;
  height: 26px !important;
  min-width: 26px !important;
  margin-right: 4px;
}
</style>
