<!-- DistanceTable.vue -->
<template>
  <div>
    <div class="text-subtitle-2 mb-2 d-flex align-center">
      Similar Records (Top 10)
      <v-tooltip location="top">
        <template v-slot:activator="{ props: tooltipProps }">
          <v-icon v-bind="tooltipProps" size="small" class="ml-2">mdi-information-outline</v-icon>
        </template>
        <div style="max-width: 300px;">
          <strong>How to read this table:</strong><br/>
          • <strong>Score:</strong> Overall similarity (higher = more similar)<br/>
          • <strong>Field Match:</strong> % of fields compared<br/>
          • <strong>Distance Match:</strong> How close values are<br/>
          • Remaining columns show actual field values from similar records
        </div>
      </v-tooltip>
    </div>
    <div ref="tableRef" class="tabulator-container"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { TabulatorFull as Tabulator } from 'tabulator-tables';
import 'tabulator-tables/dist/css/tabulator.min.css';

const props = defineProps<{
  results: any[],
  mappings: any[]
}>()

const tableRef = ref<HTMLDivElement | null>(null)
let table: Tabulator | null = null

function buildTableData() {
  return props.results.map(r => {
    const baseData: Record<string, any> = {
      id: r.id,
      key: r.keyField.value,
      score: r.score,
      fieldMatch: r.fieldMatch,
      distanceMatch: r.distanceMatch,
    }
    
    // Map comparedFields using the mapping names
    Object.keys(r.comparedFields).forEach(field => {
      const mapping = props.mappings?.find(m => m.field === field || m.name === field)
      const fieldName = mapping?.name || field
      const alias = mapping?.alias
      console.log(`Mapping for field ${field}:`, mapping)
      baseData[alias] = r.comparedFields[fieldName]
    })
    
    return baseData
  })
}

function buildColumns() {
  const baseCols = [
    { 
      title: 'Key', 
      field: 'key',
      tooltip: 'Identifier for this record'
    },
    { 
      title: 'Score', 
      field: 'score',
      tooltip: 'Overall similarity score (Field Match × 50% + Distance Match × 50%)',
      sorter: 'string',
      cssClass: 'font-weight-bold'
    },
    { 
      title: 'Field Match', 
      field: 'fieldMatch',
      tooltip: 'Percentage of selected fields available for comparison',
      sorter: 'string'
    },
    { 
      title: 'Distance Match', 
      field: 'distanceMatch',
      tooltip: 'How close the field values are (100% = identical)',
      sorter: 'string'
    },
  ]

  const sample = props.results[0]
  const dynamicCols = Object.keys(sample.comparedFields).map(field => {

    const mapping = props.mappings?.find(m => m.field === field || m.name === field)
    const title = mapping?.alias || field

    // Debug: Check if mapping is found and what values we're working with
    console.log(`Mapping alias: ${title}`, `name: ${mapping?.name}`)
    return {
      title: title,
      field: mapping?.alias || field,
      tooltip: `${title} value for this record`
    }
  })


  return [...baseCols, ...dynamicCols]
}

onMounted(() => {
  if (tableRef.value && props.results.length > 0) {
    table = new Tabulator(tableRef.value, {
      layout: 'fitColumns',
      data: buildTableData(),
      columns: buildColumns(),
      height: 300,
      autoResize: true,
    })
  }
})

watch(() => props.results, (newVal) => {
  if (table && newVal.length > 0) {
    table.replaceData(buildTableData())
    table.setColumns(buildColumns())
  }
})

watch(() => props.mappings, () => {
  if (table && props.results.length > 0) {
    table.setColumns(buildColumns())
  }
})
</script>

<style scoped>
.tabulator-container {
  width: 100%;
}
</style>
