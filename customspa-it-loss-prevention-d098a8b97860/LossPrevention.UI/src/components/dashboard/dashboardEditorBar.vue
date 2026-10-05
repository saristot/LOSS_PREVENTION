<template>
  <div class="d-flex align-center ga-2 mb-4">
    <!-- Save (icon) -->
    <v-tooltip text="Save">
      <template #activator="{ props }">
        <v-btn v-bind="props" icon :loading="saving" @click="$emit('save')">
          <v-icon>mdi-content-save</v-icon>
        </v-btn>
      </template>
    </v-tooltip>
    {{ props.name }}
    <v-btn @click="$emit('revert')" :disabled="!canRevert">Revert</v-btn>

    <v-spacer />

    <v-btn color="error" variant="text" :disabled="!canDelete" @click="$emit('delete')">
      Delete
    </v-btn>

    <!-- Status chip -->
    <v-chip :color="saving ? 'info' : (dirty ? 'warning' : 'success')" size="small" class="ml-2">
      {{ saving ? 'Saving…' : (dirty ? 'Unsaved changes' : 'Saved') }}
    </v-chip>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
  name: string;
  dirty: boolean;
  status?: string;
  saving: boolean;
  canDelete?: boolean;
  canRevert?: boolean;
}>();
defineEmits(['rename','save','revert','delete']);

const nameLocal = ref(props.name);
watch(() => props.name, v => nameLocal.value = v);
</script>
