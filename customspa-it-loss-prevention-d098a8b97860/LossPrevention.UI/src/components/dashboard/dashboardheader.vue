<template>
  <!-- dashboardHeader.vue -->
  <div class="flex items-center flex-wrap gap-3">
    <!-- Workspace (corrected) -->
    <v-select :model-value="selectedWorkspaceIdLocal" :items="workspaces" item-title="name" item-value="id"
      label="Workspace" dense hide-details style="min-width: 200px" @update:model-value="onWorkspaceSelect" />

    <!-- Tab -->
    <v-select :key="selectedWorkspaceIdLocal" :model-value="selectedTabIdLocal" :items="tabs" item-title="title"
      item-value="id" label="Report Tab" dense hide-details style="min-width: 200px"
      @update:model-value="onTabSelect" />

    <v-switch :model-value="isDesignMode" @update:model-value="val => emit('toggleMode', val)" label="Design Mode" inset
      color="primary" />
  </div>

</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';

interface Tab {
  id: string;
  title: string;
}

interface Workspace {
  id: string;
  title: string;
  tabs: Tab[];
}

const props = defineProps({
  workspaces: {
    type: Array,
    default: () => []
  },
  selectedWorkspace: {
    type: Object,
    default: null
  },
  selectedTab: {
    type: Object,
    default: null
  },
  isDesignMode: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['workspaceChange', 'tabChange', 'toggleMode']);

const selectedWorkspaceIdLocal = ref(props.selectedWorkspace?.id ?? '');
const selectedTabIdLocal = ref(props.selectedTab?.id ?? '');

/* ---- Only write to locals if the incoming prop value actually changed ---- */
watch(
  () => props.selectedWorkspace?.id,
  (id) => {
    const next = id ?? '';
    if (next !== selectedWorkspaceIdLocal.value) {
      selectedWorkspaceIdLocal.value = next;      // ✅ no redundant write
    }
  }
);

watch(
  () => props.selectedTab?.id,
  (id) => {
    const next = id ?? '';
    if (next !== selectedTabIdLocal.value) {
      selectedTabIdLocal.value = next;            // ✅ no redundant write
    }
  }
);

// Computed property for tabs based on selected workspace
const tabs = computed(() => {
  const workspace = props.workspaces.find(w => w.id === selectedWorkspaceIdLocal.value);
  return workspace?.tabs ?? [];
});

/* handlers */
function onWorkspaceSelect(id: string) {
  if (id === selectedWorkspaceIdLocal.value) return;   // ✅ guard
  selectedWorkspaceIdLocal.value = id;

  // Reset tab if it doesn't belong to new workspace
  if (!tabs.value.some(t => t.id === selectedTabIdLocal.value)) {
    selectedTabIdLocal.value = '';
  }

  emit('workspaceChange', id);
}

function onTabSelect(id: string) {
  if (id === selectedTabIdLocal.value) return;         // ✅ guard
  selectedTabIdLocal.value = id;
  emit('tabChange', id);
}
</script>
