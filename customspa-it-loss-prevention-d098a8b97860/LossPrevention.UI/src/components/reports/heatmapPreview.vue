<template>
  <div style="width: 100%; height: 400px;">
    <canvas ref="canvasRef" style="width: 100%; height: 100%;"></canvas>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, watch, ref, nextTick, shallowRef } from 'vue'
import { Chart, registerables, type Plugin } from 'chart.js'
import { MatrixController, MatrixElement } from 'chartjs-chart-matrix'

// Register once outside component
Chart.register(...registerables, MatrixController, MatrixElement)

const props = defineProps<{
  data: Array<{ x: string | number; y: string | number; v: number }>
  xLabels: any[]
  yLabels: any[]
  xLabelTitle?: string
  yLabelTitle?: string
  reverseY?: boolean
  metric?: unknown
  showZerosAsEmpty?: boolean
  valueScale?: 'linear' | 'log'
  capPercentile?: number
  showLegend?: boolean
  legendPosition?: 'bottom' | 'right'
  legendLabel?: string
  freezeDomain?: boolean
  widenOnlyDomain?: boolean
  referenceAxis?: 'x' | 'y'
  xAxisDef?: { label?: string; value?: string } | null
  yAxisDef?: { label?: string; value?: string } | null
  metricDef?: { label?: string; value?: string } | null
  enableDrilldown?: boolean
}>()

const emit = defineEmits<{
  (e: 'drill', payload: {
    x: string | number
    y: string | number
    v: number
    fields: {
      xField?: string | null
      yField?: string | null
      metricField?: string | null
      xLabel?: string | null
      yLabel?: string | null
      metricLabel: string
    }
    filters: Array<{ id?: string; field: string; operator: '=' | '=='; value: string | number }>
  }): void
}>()

// Use shallowRef for better performance
const chartInstance = shallowRef<Chart<any, any, any> | null>(null)
const canvasRef = ref<HTMLCanvasElement | null>(null)
const domainRef = ref<{ min: number; max: number } | null>(null)

// Memoization for expensive computations
const memoCache = new Map()
const memoize = (key: string, fn: () => any) => {
  if (!memoCache.has(key)) {
    memoCache.set(key, fn())
  }
  return memoCache.get(key)
}

// Debounce chart updates
let updateTimeout: number | null = null
const debounceUpdate = (fn: () => void, delay = 100) => {
  if (updateTimeout) clearTimeout(updateTimeout)
  updateTimeout = setTimeout(fn, delay) as any
}

// Clear memo cache when data changes
watch(() => props.data, () => {
  memoCache.clear()
}, { deep: false })

// Optimized watch with debouncing
watch(
  () => [
    props.data, props.xLabels, props.yLabels, props.valueScale, props.capPercentile,
    props.showLegend, props.legendPosition, props.freezeDomain, props.widenOnlyDomain,
    props.metric, props.referenceAxis, props.legendLabel, props.xAxisDef, props.yAxisDef, props.metricDef
  ],
  () => debounceUpdate(drawChart),
  { deep: false } // Shallow watching is faster
)

// Throttled click handler
let lastClickTime = 0
const CLICK_THROTTLE = 150

// Pre-compute stable references
const toStr = (v: unknown) => {
  if (v == null) return ''
  if (typeof v === 'string' || typeof v === 'number' || typeof v === 'boolean') return String(v)
  if (typeof v === 'object' && v && 'label' in (v as any)) return String((v as any).label)
  try { return JSON.stringify(v) } catch { return String(v) }
}

function metricLabelFrom(metric: unknown): string {
  if (metric == null) return 'Value'
  if (typeof metric === 'string' || typeof metric === 'number') return String(metric)
  if (typeof metric === 'object') {
    const m: any = metric
    return String(m.label ?? m.name ?? m.text ?? 'Value')
  }
  return 'Value'
}

// Optimized date formatting with caching
const dateFormatCache = new Map()
function defaultDateFmt(s: string) {
  if (dateFormatCache.has(s)) return dateFormatCache.get(s)
  
  const d = new Date(s)
  if (isNaN(d.getTime())) {
    dateFormatCache.set(s, s)
    return s
  }
  
  const formatted = d.toLocaleDateString('en-GB') // More efficient than manual formatting
  dateFormatCache.set(s, formatted)
  return formatted
}

const isIsoLikeDate = (s: string) => !isNaN(new Date(s).getTime())

function toIsoMinute(val: any): any {
  if (val == null) return val
  const d = new Date(val)
  if (isNaN(d.getTime())) return val
  
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  const HH = String(d.getHours()).padStart(2, '0')
  const MM = String(d.getMinutes()).padStart(2, '0')
  const original = typeof val === 'string' ? val : ''
  const hasTime = /T\d{2}:\d{2}/.test(original) || /\d{2}:\d{2}/.test(original)
  const hh = hasTime ? HH : '00'
  const min = hasTime ? MM : '00'
  return `${yyyy}-${mm}-${dd}T${hh}:${min}`
}

// Optimized sorting with memoization
function sortStable(arr: string[]) {
  const key = arr.join('|')
  return memoize(`sort_${key}`, () => {
    if (arr.every(isIsoLikeDate)) {
      return [...arr].sort((a, b) => +new Date(a) - +new Date(b))
    }
    return [...arr].sort((a, b) => a.localeCompare(b, undefined, { numeric: true, sensitivity: 'base' }))
  })
}

onMounted(() => {
  nextTick(drawChart)
})

onBeforeUnmount(() => {
  if (updateTimeout) clearTimeout(updateTimeout)
  chartInstance.value?.destroy()
  memoCache.clear()
  dateFormatCache.clear()
})

function percentile(values: number[], p: number) {
  const arr = [...values].sort((a, b) => a - b)
  if (!arr.length) return 0
  const idx = Math.min(arr.length - 1, Math.max(0, Math.floor(p * (arr.length - 1))))
  return arr[idx]
}

function drawChart() {
  if (!canvasRef.value) return
  
  // Destroy previous chart efficiently
  if (chartInstance.value) {
    chartInstance.value.destroy()
    chartInstance.value = null
  }

  const metricLabel = metricLabelFrom(props.metric ?? props.metricDef)

  // Memoize expensive operations
  const xCats = sortStable((props.xLabels ?? []).map(toStr))
  const yCats = sortStable((props.yLabels ?? []).map(toStr))

  const rows = (props.data ?? []).map(d => ({ x: toStr(d.x), y: toStr(d.y), v: Number(d.v ?? 0) }))
  const valueMap = new Map(rows.map(r => [`${r.x}__${r.y}`, r.v]))
  
  // Pre-allocate grid array
  const gridSize = xCats.length * yCats.length
  const grid: Array<{ x: string; y: string; v: number }> = new Array(gridSize)
  let gridIndex = 0
  
  for (const x of xCats) {
    for (const y of yCats) {
      grid[gridIndex++] = { x, y, v: valueMap.get(`${x}__${y}`) ?? 0 }
    }
  }

  // Domain calculation (existing logic)
  const vals = grid.map(g => g.v)
  const nz = vals.filter(v => v > 0)
  let rawMin = nz.length ? Math.min(...nz) : 0
  let rawMax = nz.length ? Math.max(...nz) : 1
  
  if (props.capPercentile && props.capPercentile > 0 && props.capPercentile < 1) {
    const cap = percentile(nz.length ? nz : vals, props.capPercentile)
    rawMax = Math.max(rawMin, cap || rawMax)
  }
  
  if (props.freezeDomain) {
    if (!domainRef.value) domainRef.value = { min: rawMin, max: rawMax }
    rawMin = domainRef.value.min
    rawMax = domainRef.value.max
  } else if (props.widenOnlyDomain) {
    if (!domainRef.value) domainRef.value = { min: rawMin, max: rawMax }
    domainRef.value.min = Math.min(domainRef.value.min, rawMin)
    domainRef.value.max = Math.max(domainRef.value.max, rawMax)
    rawMin = domainRef.value.min
    rawMax = domainRef.value.max
  } else {
    domainRef.value = null
  }

  const eps = 1e-9
  const normLinear = (v: number) => (rawMax === rawMin ? (v > 0 ? 1 : 0) : (v - rawMin) / (rawMax - rawMin))
  const normLog = (v: number) => {
    const lo = Math.log(rawMin + eps), hi = Math.log(rawMax + eps)
    return (!isFinite(lo) || !isFinite(hi) || hi === lo) ? normLinear(v) : (Math.log(v + eps) - lo) / (hi - lo)
  }
  const norm = (props.valueScale === 'log') ? normLog : normLinear

  // Pre-compute colors
  const colorCache = new Map()
  function lerp(a: number, b: number, t: number) { return a + (b - a) * t }
  const gamma = 0.6
  const start = { r: 255, g: 235, b: 238 }
  const end   = { r: 194, g: 18,  b: 31 }
  
  const cellColor = (v: number) => {
    if (colorCache.has(v)) return colorCache.get(v)
    
    if (props.showZerosAsEmpty && v === 0) {
      const color = 'rgba(0,0,0,0)'
      colorCache.set(v, color)
      return color
    }
    
    let t = Math.max(0, Math.min(1, norm(v)))
    t = Math.pow(t, gamma)
    const r = Math.round(lerp(start.r, end.r, t))
    const g = Math.round(lerp(start.g, end.g, t))
    const b = Math.round(lerp(start.b, end.b, t))
    const color = `rgb(${r}, ${g}, ${b})`
    colorCache.set(v, color)
    return color
  }

  // Optimized colorbar plugin (existing implementation)
  const colorbar: Plugin<'matrix'> = {
    id: 'colorbar',
    beforeLayout(chart) {
      if (props.showLegend === false) return
      const pos = props.legendPosition ?? 'bottom'
      const pad = 8, barH = 12, titleH = (props.legendLabel ?? metricLabel) ? 12 : 0, textH = 12
      const extra = pos === 'bottom' ? pad + titleH + barH + 6 + textH : pad + barH + 16 + textH
      const layout = (chart.options.layout ??= { padding: {} as any })
      const padding = (layout.padding ??= {}) as any
      if (pos === 'bottom') padding.bottom = Math.max(padding.bottom ?? 0, extra + 6)
      else padding.right  = Math.max(padding.right  ?? 0, extra + 6)
    },
    afterDraw(chart) {
      if (props.showLegend === false) return
      const { ctx, chartArea } = chart
      if (!chartArea) return
      const pos = props.legendPosition ?? 'bottom'
      const pad = 8
      const barW = Math.min(220, pos === 'bottom' ? chartArea.width * 0.4 : chartArea.height * 0.5)
      const barH = 12
      const label = props.legendLabel ?? metricLabel
      let x = 0, y = 0
      if (pos === 'bottom') { x = chartArea.left + (chartArea.width - barW) / 2; y = chartArea.bottom + pad + (label ? 12 : 0) }
      else { x = chartArea.right + pad; y = chartArea.top + (chartArea.height - barH) / 2 }

      if (pos === 'bottom' && label) {
        ctx.fillStyle = 'rgba(0,0,0,0.7)'; ctx.font = '11px sans-serif'
        ctx.textAlign = 'center'; ctx.textBaseline = 'alphabetic'; ctx.fillText(label, x + barW / 2, y - 4)
      } else if (pos === 'right' && label) {
        ctx.save(); ctx.translate(x + barW + 16, y + barH / 2); ctx.rotate(-Math.PI / 2)
        ctx.fillStyle = 'rgba(0,0,0,0.7)'; ctx.font = '11px sans-serif'
        ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillText(label, 0, 0); ctx.restore()
      }

      const steps = 40 // Reduced from 80 for better performance
      for (let i = 0; i < steps; i++) {
        const t = Math.pow(i / (steps - 1), gamma)
        const r = Math.round(lerp(start.r, end.r, t))
        const g = Math.round(lerp(start.g, end.g, t))
        const b = Math.round(lerp(start.b, end.b, t))
        ctx.fillStyle = `rgb(${r}, ${g}, ${b})`
        if (pos === 'bottom') {
          const w = barW / steps; ctx.fillRect(x + i * w, y, Math.ceil(w + 0.5), barH)
        } else {
          const h = barH / steps; ctx.fillRect(x, y + (steps - 1 - i) * h, barW, Math.ceil(h + 0.5))
        }
      }
      ctx.strokeStyle = 'rgba(0,0,0,0.25)'; ctx.strokeRect(x, y, barW, barH)
    }
  }

  const chart = new Chart(canvasRef.value, {
    type: 'matrix',
    data: {
      datasets: [{
        label: 'Heatmap',
        parsing: false,
        data: grid,
        backgroundColor(ctx: { raw: any; }) {
          const v = (ctx.raw as any)?.v ?? 0
          return cellColor(v)
        },
        borderWidth: 0.5,
        borderColor: 'rgba(0,0,0,0.05)',
        width: ({ chart }) => {
          const area = chart.chartArea
          const count = xCats.length || 1
          return area ? (area.width / count) : 10
        },
        height: ({ chart }) => {
          const area = chart.chartArea
          const count = yCats.length || 1
          return area ? (area.height / count) : 10
        }
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      animation: false, // Keep disabled for performance
      interaction: {
        intersect: false,
        mode: 'nearest'
      },
      layout: { padding: {} },
      onClick: handleDrill,
      scales: {
        x: {
          type: 'category',
          labels: xCats,
          offset: true,
          grid: { display: false },
          ticks: { 
            callback: (_val, i) => defaultDateFmt(xCats[i] ?? ''), 
            maxRotation: 45, 
            autoSkip: true,
            maxTicksLimit: 20 // Limit ticks for performance
          },
          title: { display: true, text: props.xLabelTitle ?? 'X Axis', font: { weight: 'bold' } }
        },
        y: {
          type: 'category',
          labels: yCats,
          offset: true,
          reverse: !!props.reverseY,
          grid: { display: false },
          ticks: { 
            callback: (_val, i) => defaultDateFmt(yCats[i] ?? ''),
            maxTicksLimit: 20 // Limit ticks for performance
          },
          title: { display: true, text: props.yLabelTitle ?? 'Y Axis', font: { weight: 'bold' } }
        }
      },
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            title: (items) => {
              const raw = items?.[0]?.raw as { x: unknown; y: unknown } | undefined
              if (!raw) return ''
              const refAxis = props.referenceAxis ?? 'x'
              const refVal = toStr(refAxis === 'y' ? raw.y : raw.x)
              return isIsoLikeDate(refVal) ? defaultDateFmt(refVal) : refVal
            },
            label: (ctx) => {
              const raw = ctx.raw as { v: number }
              const v = raw?.v ?? 0
              const metricLabel2 = metricLabelFrom(props.metric ?? props.metricDef)
              return `${metricLabel2}: ${v}`
            }
          }
        }
      }
    },
    plugins: [colorbar]
  })

  chartInstance.value = chart

  function handleDrill(evt: MouseEvent) {
    if (!props.enableDrilldown) return
    
    // Throttle clicks
    const now = Date.now()
    if (now - lastClickTime < CLICK_THROTTLE) return
    lastClickTime = now
    
    const chart = chartInstance.value
    if (!chart) return

    const elems = chart.getElementsAtEventForMode(evt, 'nearest', { intersect: false }, true)
    if (!elems.length) return

    const { datasetIndex, index } = elems[0]
    const datum = (chart.data.datasets[datasetIndex].data as any[])[index] as { x: any; y: any; v: number }
    if (!datum) return

    const xField = props.xAxisDef?.value ?? null
    const yField = props.yAxisDef?.value ?? null
    const metricField = props.metricDef?.value ?? null

    const xVal = xField ? toIsoMinute(datum.x) : datum.x
    const yVal = yField ? toIsoMinute(datum.y) : datum.y

    emit('drill', {
      x: datum.x,
      y: datum.y,
      v: datum.v,
      fields: {
        xField, yField, metricField,
        xLabel: props.xLabelTitle ?? null,
        yLabel: props.yLabelTitle ?? null,
        metricLabel
      },
      filters: [
        ...(xField ? [{ field: xField, operator: '$eq', value: xVal }] : []),
        ...(yField ? [{ field: yField, operator: '$eq', value: yVal }] : []),
      ]
    })
  }
}
</script>
