<template>
  <div class="condition-group-header">
    <span class="section-title">Conditions</span>
  </div>
  <div class="condition-group-container" v-if="group">
    <!-- Toolbar -->
    <div class="group-toolbar">
      <v-select 
        :items="['AND', 'OR', 'NOR']" 
        v-model="group.type" 
        density="compact"
        variant="outlined"
        hide-details
        class="type-select"
        :menu-props="{ contentClass: 'compact-menu' }"
      />
      <v-spacer />
      <v-btn size="small" variant="tonal" color="primary" @click="addCondition">
        <v-icon size="small" class="mr-1">mdi-plus</v-icon>
        Condition
      </v-btn>
      <v-btn size="small" variant="outlined" color="primary" @click="addGroup" class="ml-1">
        <v-icon size="small" class="mr-1">mdi-plus</v-icon>
        Group
      </v-btn>
      <v-btn v-if="removable" size="small" variant="text" color="error" @click="$emit('remove')" class="ml-1">
        <v-icon size="small">mdi-delete</v-icon>
      </v-btn>
    </div>

    <!-- Conditions list -->
    <div class="conditions-list">
      <div 
        v-for="(condition, i) in group.conditions" 
        :key="condition.id || i"
        draggable="true"
        @dragstart="handleDragStart(i, $event)"
        @dragend="handleDragEnd"
        @dragover="handleDragOver(i, $event)"
        @drop="handleDrop(i, $event)"
        :class="{ 
          'condition-dragging': draggedIndex === i,
          'condition-drag-over': dragOverIndex === i && draggedIndex !== i
        }"
        class="condition-item"
      >
        <v-icon class="drag-handle" size="x-small" color="grey">mdi-drag-vertical</v-icon>
        <div class="condition-content">
          <ConditionRow v-if="!condition.type" :condition="condition" @update="(val: any) => updateCondition(i, val)"
            @remove="() => removeCondition(i)" />
          <ConditionGroup v-else :group="condition" :removable="true" @update="val => updateCondition(i, val)"
            @remove="() => removeCondition(i)" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref } from 'vue';
import ConditionRow from './conditionRow.vue';
import FormattingRow from './formattingRow.vue';
import ConditionGroup from './conditionGroup.vue'; // recursive import

const props = defineProps<{
  group: any,
  removable?: boolean
}>();

const emit = defineEmits(['update', 'remove']);

// Drag and drop state
const draggedIndex = ref<number | null>(null);
const dragOverIndex = ref<number | null>(null);

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
  if (event.dataTransfer) {
    event.dataTransfer.dropEffect = 'move';
  }
  dragOverIndex.value = index;
};

const handleDrop = (dropIndex: number, event: DragEvent) => {
  event.preventDefault();
  
  if (draggedIndex.value === null || draggedIndex.value === dropIndex) {
    dragOverIndex.value = null;
    return;
  }

  const conditions = [...props.group.conditions];
  const draggedItem = conditions[draggedIndex.value];
  
  // Remove from old position
  conditions.splice(draggedIndex.value, 1);
  
  // Insert at new position
  const newIndex = draggedIndex.value < dropIndex ? dropIndex - 1 : dropIndex;
  conditions.splice(newIndex, 0, draggedItem);
  
  emit('update', { ...props.group, conditions });
  
  draggedIndex.value = null;
  dragOverIndex.value = null;
};

function makeCondition(field = '', value = '', operator = '$eq') {
  return {
    id: Date.now() + Math.random().toString(36).substr(2),
    field,
    operator,
    value
  };
}

function makeFormattingRule() {
  return {
    id: Date.now() + Math.random().toString(36).substr(2),
    field: '',
    operator: '$eq',
    value: '',
    color: '#FFEB3B'
  };
}

function addCondition() {
  const next = {
    ...props.group,
    conditions: [...(props.group.conditions || []), makeCondition()]
  };
  emit('update', next);
}

function addGroup() {
  const newGroup = {
    id: Date.now() + Math.random().toString(36).substr(2),
    type: 'AND',
    conditions: [makeCondition()]
  };

  const next = {
    ...props.group,
    conditions: [...(props.group.conditions || []), newGroup]
  };
  emit('update', next);
}

function updateCondition(i: number, val: any) {
  const conditions = [...props.group.conditions];
  conditions[i] = val;
  emit('update', { ...props.group, conditions });
}

function removeCondition(i: number) {
  const conditions = props.group.conditions.filter((_: any, index: number) => index !== i);
  emit('update', { ...props.group, conditions });
}


</script>

<style scoped>
/* Section title */
.condition-group-header {
  margin-bottom: 4px;
}

.section-title {
  font-size: 11px;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

/* Container */
.condition-group-container {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 8px;
  background: #fafafa;
  margin-bottom: 8px;
}

/* Toolbar */
.group-toolbar {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-bottom: 6px;
  padding-bottom: 6px;
  border-bottom: 1px solid #eee;
}

.type-select {
  width: 80px;
  max-width: 80px;
}

.type-select :deep(.v-field__input) {
  padding: 2px 6px;
  min-height: 24px;
  font-size: 12px;
}

/* Conditions list */
.conditions-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

/* Condition item */
.condition-item {
  display: flex;
  align-items: center;
  padding: 2px 4px;
  background: #fff;
  border: 1px solid transparent;
  border-radius: 2px;
  min-height: 32px;
}

.condition-item:hover {
  background: #f5f5f5;
  border-color: #e0e0e0;
}

.condition-item.condition-dragging {
  opacity: 0.5;
}

.condition-item.condition-drag-over {
  border-top: 2px solid #1976d2;
}

.condition-content {
  flex: 1;
  min-width: 0;
}

/* Drag handle */
.drag-handle {
  cursor: grab;
  opacity: 0.3;
  margin-right: 4px;
  flex-shrink: 0;
}

.condition-item:hover .drag-handle {
  opacity: 0.7;
}

.drag-handle:active {
  cursor: grabbing;
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

:deep(.v-select .v-field__append-inner) {
  padding-top: 0;
  padding-inline-start: 0;
}

:deep(.v-select .v-field__append-inner .v-icon) {
  font-size: 16px;
}
</style>
