<template>
  <div class="condition-row">
    <!-- Field selector -->
    <div class="col-field">
      <v-select 
        :items="fieldMappings" 
        item-title="label" 
        item-value="value" 
        v-model="localCondition.field" 
        placeholder="Field"
        density="compact"
        variant="outlined"
        hide-details
        :menu-props="{ contentClass: 'compact-menu' }"
      />
    </div>

    <!-- Operator -->
    <div class="col-operator">
      <v-select 
        :items="operatorOptions" 
        v-model="localCondition.operator" 
        item-title="text" 
        item-value="value"
        placeholder="Op"
        density="compact"
        variant="outlined"
        hide-details
        :menu-props="{ contentClass: 'compact-menu' }"
      />
    </div>

    <!-- Value input: Boolean dropdown or text input -->
    <div class="col-value">
      <template v-if="selectedMapping?.type === 'Boolean'">
        <v-select 
          v-model="localCondition.value" 
          :items="booleanOptions" 
          item-title="text" 
          item-value="value"
          placeholder="Value"
          density="compact"
          variant="outlined"
          hide-details
          :menu-props="{ contentClass: 'compact-menu' }"
        />
      </template>
      <template v-else-if="selectedMapping?.type === 'Date'">
        <v-text-field 
          v-model="localCondition.value" 
          type="datetime-local" 
          density="compact"
          variant="outlined"
          hide-details
        />
      </template>
      <template v-else>
        <v-text-field 
          v-model="localCondition.value" 
          placeholder="Value"
          density="compact"
          variant="outlined"
          hide-details
        />
      </template>
    </div>

    <!-- Remove -->
    <div class="col-actions">
      <v-btn 
        icon="mdi-close"
        size="x-small"
        variant="text"
        color="error"
        @click="$emit('remove')"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue';
import { useMappingStore } from '@/stores/mappingStore';

const props = defineProps({ condition: Object });
const emit = defineEmits(['update', 'remove']);

const localCondition = computed({
  get: () => props.condition,
  set: (v) => emit('update', v)
});

const mappingsStore = useMappingStore();

onMounted(async () => {
  // ensure mappings are loaded if parent didn't fetch yet
  if (!mappingsStore.mappings?.length) {
    await mappingsStore.fetchMappings?.();
  }
});

const fieldMappings = computed(() =>
  (mappingsStore.mappings || [])
    .filter(m => (m.IsVisible ?? m.isVisible) !== false)  // default to visible
    .map(m => {
      const name = m.Name ?? m.name;
      const alias = m.Alias ?? m.alias;
      const type = m.DataType ?? m.dataType;
      return {
        label: alias || name || '(unnamed field)',
        value: name,            // use source path for filters
        type
      };
    })
);

// used to switch Value control (boolean/date/text)
const selectedMapping = computed(() => {
  const fieldPath = localCondition.value?.field;
  if (!fieldPath) return undefined;
  return fieldMappings.value.find(f => f.value === fieldPath);
});

const operatorOptions = [
  { text: 'Equal', value: '$eq' },
  { text: 'Not Equal', value: '$ne' },
  { text: 'Greater Than', value: '$gt' },
  { text: 'Greater Than or Equal', value: '$gte' },
  { text: 'Less Than', value: '$lt' },
  { text: 'Less Than or Equal', value: '$lte' },
  { text: 'In', value: '$in' },
  { text: 'Not In', value: '$nin' },
  { text: 'Exists', value: '$exists' },
  { text: 'Matches (Regex)', value: '$regex' }
];

const booleanOptions = [
  { text: 'True', value: true },
  { text: 'False', value: false }
];
</script>

<style scoped>
/* Condition Row - Compact */
.condition-row {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 2px 0;
  min-height: 32px;
}

/* Column widths */
.col-field {
  flex: 1 1 180px;
  min-width: 120px;
}

.col-operator {
  width: 120px;
  min-width: 100px;
  max-width: 140px;
}

.col-value {
  flex: 1 1 140px;
  min-width: 100px;
}

.col-actions {
  width: 28px;
  min-width: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Compact inputs */
:deep(.v-field) {
  font-size: 12px;
}

:deep(.v-field__input) {
  padding: 2px 6px;
  min-height: 24px;
  font-size: 12px;
}

:deep(.v-field--variant-outlined) {
  --v-field-border-opacity: 0.15;
}

:deep(.v-field--variant-outlined:hover) {
  --v-field-border-opacity: 0.3;
}

:deep(.v-select .v-field__append-inner) {
  padding-top: 0;
  padding-inline-start: 0;
}

:deep(.v-select .v-field__append-inner .v-icon) {
  font-size: 16px;
}

/* Compact action button */
.col-actions :deep(.v-btn) {
  width: 20px;
  height: 20px;
}

.col-actions :deep(.v-btn .v-icon) {
  font-size: 14px;
}
</style>