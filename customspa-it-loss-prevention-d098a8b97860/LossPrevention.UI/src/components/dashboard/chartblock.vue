<template>
  <div class="chart-block border rounded shadow-md relative bg-white h-full w-full p-6">
    <div class="chart-toolbar p-6" v-if="inDesignMode">
      <v-btn icon size="small" @click="$emit('edit')">
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
      <v-btn icon size="small" @click="$emit('delete')">
        <v-icon>mdi-delete</v-icon>
      </v-btn>
    </div>

    <h3 class="text-center chart-title">{{ block.data.title }}</h3>

    <div class="chart-container">
      <canvas ref="chartCanvas"></canvas>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, onBeforeUnmount, nextTick } from 'vue';
import { useReportDataStore } from '@/stores/reportDataStore';
import { useMappingStore } from '@/stores/mappingStore';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const props = defineProps({
  block: Object,
  inDesignMode: Boolean
});

const chartCanvas = ref(null);
let chartInstance = null;

const reportStore = useReportDataStore();
const mappingStore = useMappingStore();

/* ---------------------------- helpers & types ---------------------------- */
function normalize(s) { return s?.toLowerCase(); }
const dtOf = (m) => (m?.DataType ?? m?.dataType ?? '').toString().trim().toLowerCase();
const isDecimalType = (dt) => !!dt && (dt === 'decimal' || dt === 'decimal128' || dt === 'number' || dt === 'double' || dt === 'int' || dt === 'long');
const isDateType = (dt) => !!dt && (dt === 'date' || dt === 'datetime' || dt === 'timestamp');
const isArrayMapping = (m) => (m?.IsArray === true) || (dtOf(m) === 'array');

function findMappingByAliasOrName(key) {
  if (!key) return undefined;
  return (mappingStore.mappings ?? []).find(m =>
    normalize(m.alias ?? m.Alias) === normalize(key) ||
    normalize(m.name  ?? m.Name ) === normalize(key)
  );
}

// Accept either a mapping alias/name or a literal dotted path.
function resolvePath(key) {
  if (!key) return null;
  const map = findMappingByAliasOrName(key);
  if (map) return map.name ?? map.Name ?? null;
  if (String(key).includes('.')) return String(key);
  return null;
}

// stringify anything weird (Decimal128 Extended JSON, arrays, objects)
function displayify(raw) {
  if (raw == null) return '';
  if (typeof raw === 'object') {
    if (Object.prototype.hasOwnProperty.call(raw, '$numberDecimal')) return raw.$numberDecimal;
    const s = raw.toString?.();
    if (s && s !== '[object Object]') return s;
    if (Array.isArray(raw)) return raw.map(displayify).join(', ');
    return JSON.stringify(raw);
  }
  return String(raw);
}

// numberify for Chart.js y-values
function numify(v) {
  if (v == null) return NaN;
  if (typeof v === 'object' && Object.prototype.hasOwnProperty.call(v, '$numberDecimal')) {
    const n = Number(v.$numberDecimal);
    return Number.isNaN(n) ? NaN : n;
  }
  const n = Number(v);
  return Number.isNaN(n) ? NaN : n;
}

/* ------------------------ pipeline builder (smart) ----------------------- */
async function buildPipeline(cfg) {
  let xPath = resolvePath(cfg?.xField ?? cfg?.xName);
  let yPath = resolvePath(cfg?.yField ?? cfg?.yName);
  let agg = (cfg?.aggregation || '').toLowerCase() || null;

  if (!xPath || !yPath) {
    console.error('[ChartBlock] Missing or invalid x/y path.', { xPath, yPath });
    return { pipeline: null, reason: 'missing-path' };
  }

  // mappings for type hints
  let xMap = findMappingByAliasOrName(cfg?.xField) ?? findMappingByAliasOrName(cfg?.xName);
  let yMap = findMappingByAliasOrName(cfg?.yField) ?? findMappingByAliasOrName(cfg?.yName);
  let xIsDate = isDateType(dtOf(xMap));
  let yIsDate = isDateType(dtOf(yMap));

  // If the user picked date on Y and not on X, auto-swap so date is on X.
  if (!xIsDate && yIsDate) {
    [xPath, yPath] = [yPath, xPath];
    [xMap, yMap] = [yMap, xMap];
    [xIsDate, yIsDate] = [yIsDate, xIsDate];
    // Flip labels for render (without mutating props)
    cfg = { ...cfg, xLabel: cfg.yLabel || cfg.yField || cfg.yName, yLabel: cfg.xLabel || cfg.xField || cfg.xName };
  }

  // If Y is not numeric and no agg provided, default to 'count' per X.
  if (!isDecimalType(dtOf(yMap)) && !agg) agg = 'count';

  const pipeline = [];
  const projection = {};
  const unwindRoots = new Set();

  if (xMap && isArrayMapping(xMap) && String(xPath).includes('.')) {
    unwindRoots.add(String(xPath).split('.')[0]);
  }
  if (yMap && isArrayMapping(yMap) && String(yPath).includes('.')) {
    unwindRoots.add(String(yPath).split('.')[0]);
  }
  [...unwindRoots].forEach(root => pipeline.push({ $unwind: `$${root}` }));

  // ---- Project X
  if (xIsDate) {
    projection.x = {
      $dateToString: {
        format: cfg.dateFormat || "%Y-%m-%d",
        date: {
          $toDate: {
            $cond: [
              { $and: [ { $isNumber: `$${xPath}` }, { $lt: [ `$${xPath}`, 1000000000000 ] } ] },
              { $multiply: [ `$${xPath}`, 1000 ] },
              `$${xPath}`
            ]
          }
        }
      }
    };
  } else {
    const xDt = dtOf(xMap);
    projection.x = isDecimalType(xDt) ? { $toString: `$${xPath}` } : `$${xPath}`;
  }

  // ---- Project Y (must be numeric unless agg==='count')
  if (agg) {
    if (agg !== 'count') projection.yRaw = { $toDouble: `$${yPath}` };
  } else {
    projection.y = { $toDouble: `$${yPath}` };
  }

  pipeline.push({ $project: projection });

  if (agg) {
    if (agg === 'count') {
      pipeline.push({ $group: { _id: '$x', value: { $sum: 1 } } });
    } else {
      pipeline.push({ $group: { _id: '$x', value: { [`$${agg}`]: '$yRaw' } } });
    }
    pipeline.push({ $project: { _id: 0, x: '$_id', y: { $toDouble: '$value' } } });
  }

  pipeline.push({ $sort: { x: 1 } });

  return { pipeline, reason: null, cfgOut: cfg };
}

/* --------------------- query memoization (avoid refetch) ------------------ */
let lastQueryKey = '';
let lastCached = { labels: [], values: [] };

/* -------------------------- chart render lifecycle ------------------------ */
const renderChart = async () => {
  if (!props.block?.data || !chartCanvas.value) return;

  if (!mappingStore.mappings?.length) {
    await mappingStore.fetchMappings();
    await nextTick();
  }

  const baseCfg = props.block.data;
  const { pipeline, reason, cfgOut } = await buildPipeline(baseCfg);

  // Build a stable key for the data query (pipeline only)
  const queryKey = pipeline ? JSON.stringify(pipeline) : '';

  // If no pipeline, render empty chart once
  if (!pipeline) {
    if (chartInstance) chartInstance.destroy();
    chartInstance = new Chart(chartCanvas.value, {
      type: (baseCfg.chartType || 'bar'),
      data: { labels: [], datasets: [{ label: baseCfg.yLabel || 'Value', data: [] }] },
      options: { responsive: true, maintainAspectRatio: false }
    });
    if (reason === 'missing-path') {
      console.warn('[ChartBlock] Set x/y to valid field aliases or full paths (mapping.name).');
    }
    return;
  }

  // Use cache if the pipeline didn't change
  let labels, numericValues;
  if (queryKey === lastQueryKey && lastCached.labels.length) {
    ({ labels, values: numericValues } = lastCached);
  } else {
    await reportStore.fetchReportData(pipeline, 100000, 0);
    const result = reportStore.data || [];
    labels = result.map(r => displayify(r.x));
    numericValues = result.map(r => numify(r.y)).map(v => (Number.isFinite(v) ? v : NaN));

    lastQueryKey = queryKey;
    lastCached = { labels, values: numericValues };
  }

  const yLabel = (cfgOut?.yLabel) || baseCfg.yLabel || baseCfg.yField || 'Value';
  const xLabel = (cfgOut?.xLabel) || baseCfg.xLabel || baseCfg.xField || 'Category';
  const prefix = baseCfg.prefix || '';
  const suffix = baseCfg.suffix || '';
  const chartType = String(baseCfg.chartType || 'bar').toLowerCase();

  if (chartInstance) chartInstance.destroy();

  // palette for categorical slices
  const palette = (n) => {
    const base = [
      '#42A5F5','#66BB6A','#FFA726','#AB47BC','#26C6DA',
      '#FF7043','#7E57C2','#26A69A','#EF5350','#EC407A',
      '#9CCC65','#FFCA28','#29B6F6','#8D6E63','#78909C'
    ];
    const out = [];
    for (let i = 0; i < n; i++) out.push(base[i % base.length]);
    return out;
  };

  // Common options
  let options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      title: { display: !!baseCfg.title, text: baseCfg.title || '' },
      tooltip: {
        callbacks: {
          label: (ctx) => `${prefix}${ctx.raw}${suffix}`
        }
      },
      legend: { display: true }
    }
  };

  let datasets;

  if (chartType === 'pie' || chartType === 'doughnut') {
    datasets = [{
      label: baseCfg.label || yLabel,
      data: numericValues,
      backgroundColor: palette(labels.length),
      borderWidth: 1
    }];
    options.scales = undefined; // no Cartesian scales
  } else if (chartType === 'radar') {
    const color = baseCfg.color || '#42A5F5';
    datasets = [{
      label: baseCfg.label || yLabel,
      data: numericValues,
      backgroundColor: color + '33',
      borderColor: color,
      pointBackgroundColor: color
    }];
    options.scales = { r: { beginAtZero: true, pointLabels: { display: true } } };
  } else {
    const color = baseCfg.color || '#42A5F5';
    datasets = [{
      label: baseCfg.label || yLabel,
      data: numericValues,
      backgroundColor: color,
      borderColor: color,
      borderWidth: 1,
      fill: chartType === 'line' ? false : undefined,
      tension: chartType === 'line' ? 0.3 : undefined
    }];
    options.scales = {
      x: { title: { display: true, text: xLabel }, ticks: { autoSkip: true } },
      y: { title: { display: true, text: yLabel }, beginAtZero: true }
    };
  }

  chartInstance = new Chart(chartCanvas.value, {
    type: chartType,
    data: { labels, datasets },
    options
  });
};

onMounted(renderChart);
// Only re-render on config changes, not layout changes
watch(() => props.block?.data, () => renderChart(), { deep: true });
onBeforeUnmount(() => chartInstance?.destroy());
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
