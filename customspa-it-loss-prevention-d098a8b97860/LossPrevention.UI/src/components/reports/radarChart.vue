<template>
  <div class="chart-block border rounded shadow-md relative bg-white h-full w-full p-6">
    <h3 class="text-center chart-title">{{ title }}</h3>
    <div class="chart-container">
      <canvas ref="chartCanvas"></canvas>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { Chart, RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend, RadarController } from 'chart.js'

Chart.register(RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend, RadarController)

const props = defineProps<{
  results: any[]
  title?: string
  inDesignMode?: boolean,
  legendLabelField?: string
}>()

const chartCanvas = ref(null)
let chartInstance: Chart | null = null

function getColor(index: number): string {
  const colors = [
    '#3366CC', '#DC3912', '#FF9900', '#109618', '#990099',
    '#3B3EAC', '#0099C6', '#DD4477', '#66AA00', '#B82E2E',
    '#316395', '#994499', '#22AA99', '#AAAA11', '#6633CC'
  ]
  return colors[index % colors.length]
}

function renderRadarChart() {
  if (!props.results?.length || !chartCanvas.value) return

  const labels = Object.keys(props.results[0].comparedFields || {})

  const datasets = props.results.map((r, i) => ({
    label: props.results[i].keyField.value || `Result ${i + 1}`,
    data: labels.map(k => r.comparedFields[k]),
    backgroundColor: `${getColor(i)}33`,
    borderColor: getColor(i),
    pointBackgroundColor: getColor(i),
    fill: true
  }))

  if (chartInstance) chartInstance.destroy()

  chartInstance = new Chart(chartCanvas.value, {
    type: 'radar',
    data: {
      labels,
      datasets
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top'
        },
        title: {
          display: !!props.title,
          text: props.title || ''
        }
      },
      scales: {
        r: {
          beginAtZero: true,
          ticks: {
            precision: 0
          }
        }
      }
    }
  })
}

onMounted(renderRadarChart)
onBeforeUnmount(() => chartInstance?.destroy())
watch(() => props.results, renderRadarChart, { deep: true })
</script>

<style scoped>
.chart-block {
  height: 100%;
  width: 100%;
  display: flex;
  flex-direction: column;
  padding: 20px;
  overflow: hidden;
}

.chart-title {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin: 0.5rem 0;
}

.chart-container {
  flex: 1;
  position: relative;
  width: 100%;
  height: 100%;
}

.chart-toolbar {
  position: absolute;
  top: 4px;
  right: 4px;
  z-index: 10;
  display: flex;
  gap: 4px;
}

canvas {
  width: 100% !important;
  height: 100% !important;
}
</style>
