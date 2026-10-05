<template>
  <div class="select-fields-container">
    <!-- Loading Skeleton -->
    <div v-if="loading" class="loading-skeleton">
      <v-row dense class="mb-3">
        <v-col cols="auto">
          <v-skeleton-loader type="button" width="100"></v-skeleton-loader>
        </v-col>
        <v-col cols="auto">
          <v-skeleton-loader type="button" width="100"></v-skeleton-loader>
        </v-col>
      </v-row>
      <div v-for="i in 4" :key="i" class="mb-2">
        <v-skeleton-loader type="list-item" height="48"></v-skeleton-loader>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else>
      <!-- Compact Toolbar -->
      <div class="toolbar-row">
        <div class="toolbar-actions">
          <v-btn 
            color="primary" 
            size="small" 
            variant="flat"
            prepend-icon="mdi-plus"
            @click="addField"
          >
            Add Field
          </v-btn>
          <v-btn 
            size="small" 
            variant="outlined"
            prepend-icon="mdi-playlist-plus"
            @click="addAllFields"
          >
            Add All
          </v-btn>
          <v-divider vertical class="mx-2"></v-divider>
          <v-btn 
            size="small" 
            variant="text"
            prepend-icon="mdi-sort-alphabetical-ascending"
            @click="sortAlphabetically"
            :disabled="!tab.selectedFields?.length"
          >
            Sort A-Z
          </v-btn>
          <v-btn 
            size="small" 
            variant="text"
            :prepend-icon="allVisible ? 'mdi-eye-off' : 'mdi-eye'"
            @click="toggleAllVisibility"
          >
            {{ allVisible ? 'Hide All' : 'Show All' }}
          </v-btn>
          <v-btn 
            size="small" 
            variant="text"
            prepend-icon="mdi-delete-sweep"
            color="error"
            @click="removeAllFields"
            :disabled="!tab.selectedFields?.length"
          >
            Remove All
          </v-btn>
        </div>
        <div class="field-counter">
          <v-chip size="small" variant="tonal" color="primary">
            {{ visibleCount }}/{{ tab.selectedFields?.length || 0 }} visible
          </v-chip>
        </div>
      </div>

      <!-- Compact Column Headers -->
      <div class="header-row">
        <div class="col-drag"></div>
        <div class="col-visible">
          <v-icon size="x-small">mdi-eye</v-icon>
        </div>
        <div class="col-field">Field</div>
        <div class="col-alias">Alias</div>
        <div class="col-group">
          <v-tooltip text="Group By" location="top">
            <template #activator="{ props }">
              <span v-bind="props">Grp</span>
            </template>
          </v-tooltip>
        </div>
        <div class="col-aggregate">Aggregate</div>
        <div class="col-format">Format</div>
        <div class="col-actions">Actions</div>
      </div>

      <!-- Scrollable Field List -->
      <div 
        ref="scrollContainer" 
        class="fields-container"
        @scroll="handleScroll"
      >
        <!-- Virtual scroll spacer before -->
        <div :style="{ height: `${offsetBefore}px` }"></div>
        
        <div 
          v-for="field in visibleFields" 
          :key="field.id" 
          class="field-row"
          :class="{ 
            'field-hidden': !field.visible,
            'field-dragging': draggedIndex === field.originalIndex,
            'field-drag-over': dragOverIndex === field.originalIndex && draggedIndex !== field.originalIndex
          }"
          draggable="true"
          @dragstart="handleDragStart(field.originalIndex, $event)"
          @dragend="handleDragEnd"
          @dragover="handleDragOver(field.originalIndex, $event)"
          @drop="handleDrop(field.originalIndex, $event)"
        >
          <!-- Drag Handle -->
          <div class="col-drag">
            <v-icon size="x-small" class="drag-handle">mdi-drag-vertical</v-icon>
          </div>

          <!-- Visibility Toggle -->
          <div class="col-visible">
            <v-checkbox
              v-model="field.visible"
              hide-details
              density="compact"
              :true-icon="'mdi-eye'"
              :false-icon="'mdi-eye-off'"
              color="primary"
            />
          </div>

          <!-- Field Selection -->
          <div class="col-field">
            <v-autocomplete
              v-if="!field.isCalculated"
              v-model="field.name"
              :items="fieldOptions"
              item-title="Name"
              item-value="Name"
              placeholder="Select field..."
              density="compact"
              hide-details
              variant="outlined"
              single-line
              @update:model-value="val => onFieldSelected(field, typeof val === 'string' ? val : val?.Name)"
            >
              <template #item="{ props: itemProps, item }">
                <v-list-item 
                  v-bind="itemProps"
                  density="compact"
                  :title="(item.raw as any).Name"
                  :subtitle="(item.raw as any).Alias"
                />
              </template>
              <template #prepend-inner>
                <v-icon 
                  v-if="field.isCalculated" 
                  size="x-small" 
                  color="success"
                >mdi-function</v-icon>
              </template>
            </v-autocomplete>
            <v-text-field
              v-else
              v-model="field.expression"
              placeholder="Expression..."
              density="compact"
              hide-details
              variant="outlined"
              single-line
            >
              <template #prepend-inner>
                <v-icon size="x-small" color="success">mdi-function</v-icon>
              </template>
            </v-text-field>
          </div>

          <!-- Alias -->
          <div class="col-alias">
            <v-text-field 
              v-model="field.alias" 
              placeholder="Display name"
              density="compact" 
              hide-details
              variant="outlined"
              single-line
            />
          </div>

          <!-- Group By -->
          <div class="col-group">
            <v-switch
              v-model="field.groupBy" 
              hide-details
              density="compact"
              color="success"
              class="field-toggle"
              @update:modelValue="onGroupByChange(field)"
            />
          </div>

          <!-- Aggregation -->
          <div class="col-aggregate">
            <v-select
              v-model="field.aggregation"
              :items="getAggregationOptions?.(field.name) || []"
              placeholder="None"
              density="compact"
              hide-details
              variant="outlined"
              single-line
              :disabled="field.groupBy || field.isCalculated"
              :menu-props="{ contentClass: 'compact-menu' }"
            />
          </div>

          <!-- Format (Prefix/Suffix combined) -->
          <div class="col-format">
            <div class="format-inputs">
              <v-text-field 
                v-model="field.prefix" 
                placeholder="$"
                density="compact" 
                hide-details
                variant="outlined"
                single-line
                class="format-input"
              />
              <span class="format-separator">•</span>
              <v-text-field 
                v-model="field.suffix" 
                placeholder="%"
                density="compact" 
                hide-details
                variant="outlined"
                single-line
                class="format-input"
              />
            </div>
          </div>

          <!-- Actions -->
          <div class="col-actions">
            <v-tooltip text="Calculated field" location="top">
              <template #activator="{ props }">
                <v-btn 
                  v-bind="props"
                  icon="mdi-function" 
                  size="x-small" 
                  variant="text"
                  density="compact"
                  :color="field.isCalculated ? 'success' : 'grey'"
                  @click="onCalculatedToggle(field)"
                />
              </template>
            </v-tooltip>
            <v-tooltip text="Duplicate" location="top">
              <template #activator="{ props }">
                <v-btn 
                  v-bind="props"
                  icon="mdi-content-copy" 
                  size="x-small" 
                  variant="text"
                  density="compact"
                  @click="duplicateField(field.originalIndex)"
                />
              </template>
            </v-tooltip>
            <v-tooltip text="Remove" location="top">
              <template #activator="{ props }">
                <v-btn 
                  v-bind="props"
                  icon="mdi-close" 
                  size="x-small" 
                  variant="text"
                  density="compact"
                  color="error"
                  @click="removeField(field.originalIndex)"
                />
              </template>
            </v-tooltip>
          </div>
        </div>
        
        <!-- Virtual scroll spacer after -->
        <div :style="{ height: `${offsetAfter}px` }"></div>
      </div>

      <!-- Empty State -->
      <div v-if="!tab.selectedFields?.length" class="empty-state">
        <v-icon size="48" color="grey-lighten-1">mdi-table-column-plus-after</v-icon>
        <p class="text-body-2 text-grey mt-2">No fields selected</p>
        <v-btn size="small" color="primary" variant="tonal" @click="addField">
          Add your first field
        </v-btn>
      </div>
    </div>
  </div>
</template>



<script setup lang="ts">
import { computed, ref, watch, onMounted, nextTick } from 'vue';

const props = defineProps({
  tab: {
    type: Object,
    required: true
  },
  fieldOptions: Array,
  getAggregationOptions: Function,
  resetFieldProps: Function,
  loading: {
    type: Boolean,
    default: false
  }
});

// Ensure all fields have unique IDs
onMounted(() => {
  if (props.tab.selectedFields && Array.isArray(props.tab.selectedFields)) {
    props.tab.selectedFields.forEach((field: any) => {
      if (!field.id) {
        field.id = `field-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      }
    });
  }
});

// Drag and drop state
const draggedIndex = ref<number | null>(null);
const dragOverIndex = ref<number | null>(null);

// Virtual scrolling state
const scrollContainer = ref<HTMLElement | null>(null);
const itemHeight = 44; // Height of each field row
const overscan = 5;
const scrollTop = ref(0);
const containerHeight = ref(400);
let scrollTimeout: number | null = null;
const forceUpdateKey = ref(0);

// Watch for changes to selectedFields
watch(() => props.tab.selectedFields?.length, () => {
  forceUpdateKey.value++;
}, { deep: true });

// Virtual scrolling computed
const startIndex = computed(() => {
  forceUpdateKey.value;
  return Math.max(0, Math.floor(scrollTop.value / itemHeight) - overscan);
});

const endIndex = computed(() => {
  forceUpdateKey.value;
  const visibleItems = Math.ceil(containerHeight.value / itemHeight);
  const fields = props.tab.selectedFields || [];
  return Math.min(fields.length, startIndex.value + visibleItems + overscan * 2);
});

const visibleFields = computed(() => {
  forceUpdateKey.value;
  const fields = props.tab.selectedFields || [];
  return fields.slice(startIndex.value, endIndex.value).map((field: any, idx: number) => {
    field.originalIndex = startIndex.value + idx;
    return field;
  });
});

const offsetBefore = computed(() => startIndex.value * itemHeight);
const offsetAfter = computed(() => {
  const fields = props.tab.selectedFields || [];
  return (fields.length - endIndex.value) * itemHeight;
});

const handleScroll = (event: Event) => {
  const target = event.target as HTMLElement;
  if (scrollTimeout) cancelAnimationFrame(scrollTimeout);
  scrollTimeout = requestAnimationFrame(() => {
    scrollTop.value = target.scrollTop;
  });
};

// Drag handlers
const handleDragStart = (index: number, event: DragEvent) => {
  draggedIndex.value = index;
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('text/html', index.toString());
  }
};

const handleDragEnd = () => {
  draggedIndex.value = null;
  dragOverIndex.value = null;
};

const handleDragOver = (index: number, event: DragEvent) => {
  event.preventDefault();
  if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
  dragOverIndex.value = index;
};

const handleDrop = (dropIndex: number, event: DragEvent) => {
  event.preventDefault();
  if (draggedIndex.value === null || draggedIndex.value === dropIndex) {
    dragOverIndex.value = null;
    return;
  }
  const fields = props.tab.selectedFields;
  const draggedItem = fields[draggedIndex.value];
  fields.splice(draggedIndex.value, 1);
  const newIndex = draggedIndex.value < dropIndex ? dropIndex - 1 : dropIndex;
  fields.splice(newIndex, 0, draggedItem);
  draggedIndex.value = null;
  dragOverIndex.value = null;
};

// Computed
const visibleCount = computed(() => 
  props.tab.selectedFields?.filter((f: any) => f.visible !== false).length || 0
);

const allVisible = computed(() => {
  const fields = props.tab.selectedFields || [];
  return fields.length > 0 && fields.every((f: any) => f.visible !== false);
});

// Methods
function addField() {
  if (!props.tab.selectedFields) props.tab.selectedFields = [];
  
  const newField = {
    id: `field-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
    name: '',
    alias: '',
    dataType: '',
    groupBy: false,
    aggregation: '',
    isCalculated: false,
    expression: '',
    prefix: '',
    suffix: '',
    visible: true
  };
  
  props.tab.selectedFields.push(newField);
  
  nextTick(() => {
    if (scrollContainer.value) {
      scrollContainer.value.scrollTop = scrollContainer.value.scrollHeight;
    }
  });
}

function addAllFields() {
  if (!props.fieldOptions || props.fieldOptions.length === 0) return;
  
  const existingFieldNames = new Set(
    props.tab.selectedFields?.map((f: any) => f.name).filter(Boolean) || []
  );

  let addedCount = 0;
  props.fieldOptions.forEach((field: any) => {
    if (!existingFieldNames.has(field.Name)) {
      props.tab.selectedFields.push({
        id: `field-${Date.now()}-${Math.random().toString(36).substr(2, 9)}-${addedCount}`,
        name: field.Name,
        alias: field.Alias || field.Name,
        dataType: field.DataType,
        groupBy: false,
        aggregation: '',
        isCalculated: false,
        expression: '',
        prefix: '',
        suffix: '',
        visible: true
      });
      addedCount++;
    }
  });
}

function onFieldSelected(field: any, selectedName: string) {
  if (!selectedName) return;
  const selected = props.fieldOptions?.find((f: any) => f.Name === selectedName) as any;
  if (selected) {
    field.name = selected.Name;
    field.alias = selected.Alias || selected.Name;
    field.dataType = selected.DataType;
  }
  if (props.resetFieldProps) props.resetFieldProps(field);
}

function onCalculatedToggle(field: any) {
  field.isCalculated = !field.isCalculated;
  if (field.isCalculated) {
    field.name = '';
    field.aggregation = '';
    field.groupBy = false;
  } else {
    field.expression = '';
  }
}

function onGroupByChange(field: any) {
  if (field.groupBy) field.aggregation = '';
}

function duplicateField(index: number) {
  const field = props.tab.selectedFields[index];
  const duplicate = { 
    ...field,
    id: `field-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
    alias: field.alias ? `${field.alias} (Copy)` : ''
  };
  props.tab.selectedFields.splice(index + 1, 0, duplicate);
}

function removeField(index: number) {
  props.tab.selectedFields.splice(index, 1);
}

function toggleAllVisibility() {
  const newVisibility = !allVisible.value;
  props.tab.selectedFields.forEach((f: any) => f.visible = newVisibility);
}

function sortAlphabetically() {
  if (!props.tab.selectedFields?.length) return;
  props.tab.selectedFields.sort((a: any, b: any) => {
    const nameA = (a.name || a.alias || '').toLowerCase();
    const nameB = (b.name || b.alias || '').toLowerCase();
    return nameA.localeCompare(nameB);
  });
}

function removeAllFields() {
  if (!props.tab.selectedFields?.length) return;
  if (confirm('Are you sure you want to remove all fields?')) {
    props.tab.selectedFields.splice(0, props.tab.selectedFields.length);
  }
}
</script>

<style scoped>
.select-fields-container {
  width: 100%;
}

.toolbar-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  margin-bottom: 8px;
  border-bottom: 1px solid #e0e0e0;
}

.toolbar-actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

.field-counter {
  font-size: 12px;
  color: #666;
}

/* Header Row */
.header-row {
  display: flex;
  align-items: center;
  padding: 4px 4px;
  background: #f5f5f5;
  border-radius: 2px;
  font-size: 10px;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  margin-bottom: 2px;
}

/* Fields Container */
.fields-container {
  max-height: calc(100vh - 500px);
  min-height: 200px;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Field Row - Compact */
.field-row {
  display: flex;
  align-items: center;
  padding: 2px 4px;
  border: 1px solid transparent;
  border-radius: 2px;
  margin-bottom: 1px;
  background: #fff;
  min-height: 32px;
}

.field-row:hover {
  background: #fafafa;
  border-color: #e0e0e0;
}

.field-row.field-hidden {
  opacity: 0.5;
  background: #f9f9f9;
}

.field-row.field-dragging {
  opacity: 0.5;
}

.field-row.field-drag-over {
  border-top: 2px solid #1976d2;
}

/* Column Widths - Compact fixed widths */
.col-drag {
  width: 20px;
  min-width: 20px;
  max-width: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.col-visible {
  width: 28px;
  min-width: 28px;
  max-width: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.col-field {
  flex: 1 1 180px;
  min-width: 100px;
  padding: 0 2px;
}

.col-alias {
  flex: 1 1 140px;
  min-width: 80px;
  padding: 0 2px;
}

.col-group {
  width: 50px;
  min-width: 50px;
  max-width: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.col-aggregate {
  width: 80px;
  min-width: 80px;
  max-width: 80px;
  padding: 0 2px;
}

.col-format {
  width: 80px;
  min-width: 80px;
  max-width: 80px;
  padding: 0 2px;
}

.col-actions {
  width: 60px;
  min-width: 60px;
  max-width: 60px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0;
  padding-right: 2px;
}

/* Format inputs */
.format-inputs {
  display: flex;
  align-items: center;
  gap: 1px;
}

.format-input {
  flex: 1;
}

.format-input :deep(.v-field__input) {
  font-size: 11px;
  padding: 2px 4px;
  min-height: 22px;
}

.format-separator {
  color: #ccc;
  font-size: 9px;
}

/* Drag handle */
.drag-handle {
  cursor: grab;
  opacity: 0.3;
  transition: opacity 0.2s;
}

.field-row:hover .drag-handle {
  opacity: 0.7;
}

.drag-handle:active {
  cursor: grabbing;
}

/* Empty state */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  text-align: center;
}

/* Input styling for compact layout */
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

/* Compact toggle switch for field rows */
.field-toggle {
  margin: 0 4px;
}

.field-toggle :deep(.v-switch__track) {
  width: 32px;
  height: 16px;
  border-radius: 8px;
}

.field-toggle :deep(.v-switch__thumb) {
  width: 12px;
  height: 12px;
}

.field-toggle :deep(.v-selection-control) {
  min-height: 22px;
}

.field-toggle :deep(.v-selection-control__wrapper) {
  height: 16px;
  width: 32px;
}

:deep(.v-checkbox .v-selection-control) {
  min-height: auto;
}

:deep(.v-selection-control__wrapper) {
  height: 16px;
  width: 16px;
}

:deep(.v-selection-control__input) {
  height: 16px;
  width: 16px;
}

:deep(.v-selection-control__input .v-icon) {
  font-size: 16px;
}

/* Make selects more compact */
:deep(.v-select .v-field__append-inner) {
  padding-top: 0;
  padding-inline-start: 0;
}

:deep(.v-select .v-field__append-inner .v-icon) {
  font-size: 16px;
}

/* Compact action buttons */
.col-actions :deep(.v-btn) {
  width: 20px;
  height: 20px;
}

.col-actions :deep(.v-btn .v-icon) {
  font-size: 13px;
}
</style>
