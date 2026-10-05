<template>
  <div class="formatting-section">
    <div class="section-header">
      <span class="section-title">Conditional Formatting</span>
      <v-btn size="small" variant="tonal" color="primary" @click="addRule">
        <v-icon size="small" class="mr-1">mdi-plus</v-icon>
        Add Rule
      </v-btn>
    </div>

    <div class="formatting-container" v-if="group && group.length > 0">
      <!-- Header row -->
      <div class="header-row">
        <div class="col-field">Field</div>
        <div class="col-operator">Operator</div>
        <div class="col-value">Value</div>
        <div class="col-color">Color</div>
        <div class="col-actions"></div>
      </div>

      <!-- Rules list -->
      <div class="rules-list">
        <div
          v-for="(rule, index) in group"
          :key="index"
          class="rule-row"
        >
          <!-- Field selector -->
          <div class="col-field">
            <v-select
              :items="fieldMappings"
              item-title="label"
              item-value="value"
              v-model="rule.field"
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
              v-model="rule.operator"
              item-title="text"
              item-value="value"
              placeholder="Op"
              density="compact"
              variant="outlined"
              hide-details
              :menu-props="{ contentClass: 'compact-menu' }"
            />
          </div>

          <!-- Value input -->
          <div class="col-value">
            <template v-if="getFieldType(rule.field) === 'Boolean'">
              <v-select
                v-model="rule.value"
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
            <template v-else-if="getFieldType(rule.field) === 'Date'">
              <v-text-field
                :model-value="formatDateForInput(rule.value)"
                @update:model-value="rule.value = formatDateFromInput($event)"
                type="datetime-local"
                density="compact"
                variant="outlined"
                hide-details
              />
            </template>
            <template v-else>
              <v-text-field
                v-model="rule.value"
                placeholder="Value"
                density="compact"
                variant="outlined"
                hide-details
              />
            </template>
          </div>

          <!-- Color picker -->
          <div class="col-color">
            <v-menu
              v-model="colorMenus[index]"
              :close-on-content-click="false"
              location="bottom"
            >
              <template #activator="{ props }">
                <div 
                  v-bind="props"
                  class="color-swatch"
                  :style="{ backgroundColor: rule.color || '#ffffff' }"
                ></div>
              </template>
              <v-color-picker
                v-model="rule.color"
                show-swatches
                mode="hexa"
                hide-inputs
                elevation="4"
                width="280"
              />
            </v-menu>
          </div>

          <!-- Remove button -->
          <div class="col-actions">
            <v-btn 
              icon="mdi-close" 
              size="x-small" 
              variant="text" 
              color="error"
              @click="removeRule(index)"
            />
          </div>
        </div>
      </div>
    </div>

    <div v-else class="empty-state">
      <span class="empty-text">No formatting rules. Click "Add Rule" to create one.</span>
    </div>
  </div>
</template>


<script setup>
import { ref, computed, watch } from 'vue';
import { useMappingStore } from '@/stores/mappingStore';

const props = defineProps({
  group: {
    type: Array,
    required: true,
    default: () => []
  }
});
const emit = defineEmits(['update:group']);

const colorMenus = ref([]);

// Ensure we react only when group is defined and is an array
watch(
  () => props.group?.length || 0,
  (newLength) => {
    colorMenus.value = new Array(newLength).fill(false);
  },
  { immediate: true }
);

const mappingsStore = useMappingStore();
const fieldMappings = computed(() =>
  mappingsStore.mappings
    .filter((m) => m.IsVisible)
    .map((m) => ({ label: m.Alias || m.Name, value: m.Name, type: m.DataType }))
);

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
function getFieldType(fieldName) {
  return fieldMappings.value.find((m) => m.value === fieldName)?.type;
}

function formatDateForInput(dateValue) {
  if (!dateValue) return '';
  try {
    const date = new Date(dateValue);
    return date.toISOString().slice(0, 16);
  } catch {
    return '';
  }
}

function formatDateFromInput(inputValue) {
  if (!inputValue) return '';
  return new Date(inputValue).toISOString();
}


function addRule() {
  if (Array.isArray(props.group)) {
    const updated = [...props.group, {
      field: '',
      operator: '$eq',
      value: '',
      color: '#ffffff'
    }];
    emit('update:group', updated);
  }
}

function removeRule(index) {
  if (Array.isArray(props.group)) {
    const updated = props.group.filter((_, i) => i !== index);
    emit('update:group', updated);
  }
}


</script>

<style scoped>
/* Section */
.formatting-section {
  margin-top: 8px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.section-title {
  font-size: 11px;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

/* Container */
.formatting-container {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

/* Header Row */
.header-row {
  display: flex;
  align-items: center;
  padding: 4px 4px;
  background: #f5f5f5;
  font-size: 10px;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  gap: 4px;
}

/* Rules list */
.rules-list {
  max-height: 200px;
  overflow-y: auto;
}

/* Rule Row */
.rule-row {
  display: flex;
  align-items: center;
  padding: 2px 4px;
  gap: 4px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  min-height: 32px;
}

.rule-row:last-child {
  border-bottom: none;
}

.rule-row:hover {
  background: #fafafa;
}

/* Column widths */
.col-field {
  flex: 1 1 160px;
  min-width: 100px;
}

.col-operator {
  width: 110px;
  min-width: 90px;
  max-width: 120px;
}

.col-value {
  flex: 1 1 120px;
  min-width: 80px;
}

.col-color {
  width: 32px;
  min-width: 32px;
  max-width: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.col-actions {
  width: 28px;
  min-width: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Color swatch */
.color-swatch {
  width: 22px;
  height: 22px;
  border-radius: 3px;
  border: 1px solid #ccc;
  cursor: pointer;
  transition: transform 0.1s;
}

.color-swatch:hover {
  transform: scale(1.1);
  border-color: #999;
}

/* Empty state */
.empty-state {
  padding: 16px;
  text-align: center;
  background: #fafafa;
  border: 1px dashed #ddd;
  border-radius: 4px;
}

.empty-text {
  font-size: 12px;
  color: #888;
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
