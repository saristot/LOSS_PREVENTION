<template>
  <v-container>
    <!-- Header / Actions -->
    <v-row align="center" class="mb-4">
      <v-col cols="8">
        <v-text-field
          v-model="searchQuery"
          label="Search Dashboards"
          clearable
        />
      </v-col>
      <v-col cols="4" class="text-end">
        <v-btn color="primary" @click="openCreateDialog">
          Add Dashboard
        </v-btn>
      </v-col>
    </v-row>

    <!-- Add/Edit Dialog -->
    <v-dialog v-model="showDialog" max-width="480px">
      <v-card>
        <v-card-title>{{ editingDashboard ? 'Edit Dashboard' : 'New Dashboard' }}</v-card-title>
        <v-card-text>
          <v-text-field v-model="dashboardName" label="Name" required />

          <v-text-field v-model="dashboardDescription" label="Description" />

          <!-- Workspace and Tab selection (required for API) -->
          <v-select
            :items="workspaceOptions"
            item-title="name"
            item-value="id"
            v-model="selectedWorkspaceId"
            label="Workspace"
            :disabled="workspaceStore.loading"
            :loading="workspaceStore.loading"
            required
            class="mt-2"
          />
          <v-select
            :items="tabOptions"
            item-title="title"
            item-value="id"
            v-model="selectedTabId"
            label="Tab"
            :disabled="!selectedWorkspaceId"
            required
            class="mt-2"
          />
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="resetDialog">Cancel</v-btn>
          <v-btn
            color="primary"
            :disabled="!canSave || saving"
            :loading="saving"
            @click="saveDashboard"
          >
            {{ editingDashboard ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="400px">
      <v-card>
        <v-card-title>Delete Dashboard</v-card-title>
        <v-card-text>
          Are you sure you want to delete <strong>{{ dashboardToDelete?.name }}</strong>?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showDeleteDialog = false">Cancel</v-btn>
          <v-btn color="error" :loading="deleting" @click="confirmDeleteDashboard">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Loading -->
    <v-progress-linear indeterminate v-if="loading" />

    <!-- List -->
    <v-list v-if="!loading && filteredDashboards.length">
      <v-list-item
        v-for="dash in filteredDashboards"
        :key="dash.id"
        @click="openDashboard(dash)"
      >
        <v-list-item-title class="d-flex align-center">
          <span class="mr-2">{{ dash.name }}</span>
          <v-chip
            v-if="dash.published"
            size="small"
            variant="tonal"
            color="success"
            class="ml-2"
          >
            Published
          </v-chip>
        </v-list-item-title>

        <v-list-item-subtitle>
          {{ dash.blocks?.length || 0 }} block{{ (dash.blocks?.length || 0) === 1 ? '' : 's' }}
        </v-list-item-subtitle>

        <template #append>
          <v-btn icon class="mr-2 mt-2" @click.stop="openDashboard(dash)" :title="`Open ${dash.name}`">
            <v-icon>mdi-open-in-new</v-icon>
          </v-btn>

          <v-btn icon class="mt-2" @click.stop="deleteDashboard(dash)" :title="`Delete ${dash.name}`">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>

    <!-- Empty -->
    <v-alert type="info" v-else-if="!loading">
      No dashboards found.
    </v-alert>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useDashboardStore } from '@/stores/dashboardStore'
import { useWorkspaceStore } from '@/stores/workspaceStore'
import type { Tab } from '@/interfaces/tab'
import type { Workspace } from '@/interfaces/workspace'

const router = useRouter()
const dashboardStore = useDashboardStore()
const workspaceStore = useWorkspaceStore()

interface Dashboard {
  id: string
  name: string
  description?: string
  published?: boolean
  blocks?: any[]
  workspaceId?: string
  tabId?: string
}

const dashboards = ref<Dashboard[]>([])
const searchQuery = ref('')
const loading = ref(false)

const showDialog = ref(false)
const dashboardName = ref('')
const dashboardDescription = ref('')
const editingDashboard = ref<Dashboard | null>(null)
const saving = ref(false)

const showDeleteDialog = ref(false)
const dashboardToDelete = ref<Dashboard | null>(null)
const deleting = ref(false)

/** Workspace + Tab selection for create/edit */
const selectedWorkspaceId = ref<string | null>(null)
const selectedTabId = ref<string | null>(null)

const workspaceOptions = computed<Workspace[]>(() => workspaceStore.workspaces)
const tabOptions = computed<Tab[]>(() => {
  if (!selectedWorkspaceId.value) return []
  const ws = workspaceStore.workspaces.find(w => w.id === selectedWorkspaceId.value)
  return ws?.tabs ?? []
})

const canSave = computed(() =>
  !!dashboardName.value.trim() && !!selectedWorkspaceId.value && !!selectedTabId.value
)

const filteredDashboards = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return dashboards.value
  return dashboards.value.filter(d => (d.name || '').toLowerCase().includes(q))
})

async function loadDashboards() {
  loading.value = true
  try {
    dashboards.value = await dashboardStore.list()
  } catch (err) {
    console.error('Failed to load dashboards:', err)
  } finally {
    loading.value = false
  }
}

function openDashboard(dashboard: Dashboard) {
  dashboardStore.load({
    ...dashboard,
    blocks: dashboard.blocks ?? []
  })
  router.push(`/dashboard/${dashboard.id}`)
}

function deleteDashboard(dashboard: Dashboard) {
  dashboardToDelete.value = dashboard
  showDeleteDialog.value = true
}

async function confirmDeleteDashboard() {
  if (!dashboardToDelete.value) return
  deleting.value = true
  try {
    await dashboardStore.remove(dashboardToDelete.value.id)
    await loadDashboards()
    showDeleteDialog.value = false
    dashboardToDelete.value = null
  } catch (err) {
    console.error('Failed to delete dashboard:', err)
  } finally {
    deleting.value = false
  }
}

function openCreateDialog() {
  editingDashboard.value = null
  dashboardName.value = ''
  dashboardDescription.value = ''
  // default workspace -> first available
  selectedWorkspaceId.value = workspaceStore.workspaces[0]?.id ?? null
  // default tab -> first of that workspace
  selectedTabId.value = tabOptions.value[0]?.id ?? null
  showDialog.value = true
}

function resetDialog() {
  editingDashboard.value = null
  dashboardName.value = ''
  dashboardDescription.value = ''
  selectedWorkspaceId.value = null
  selectedTabId.value = null
  showDialog.value = false
  saving.value = false
}

async function saveDashboard() {
  if (!canSave.value) return
  saving.value = true
  try {
    const payload = {
      name: dashboardName.value.trim(),
      description: dashboardDescription.value.trim(),
      blocks: editingDashboard.value?.blocks ?? [],
      workspaceId: selectedWorkspaceId.value as string,
      tabId: selectedTabId.value as string
    }

    // Your store should decide create vs update by presence of id
    if (editingDashboard.value?.id) {
      const updatePayload = { ...payload, id: editingDashboard.value.id }
      await dashboardStore.save(updatePayload)
    } else {
      await dashboardStore.save(payload)
    }

    resetDialog()
    await loadDashboards()
  } catch (err) {
    console.error('Failed to save dashboard:', err)
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  await workspaceStore.fetchWorkspaces()
  await loadDashboards()
})

/** If workspace changes, auto-pick first tab so the form stays valid */
watch(selectedWorkspaceId, () => {
  if (!selectedWorkspaceId.value) {
    selectedTabId.value = null
    return
  }
  const firstTab = tabOptions.value[0]
  if (firstTab && (!selectedTabId.value || !tabOptions.value.some(t => t.id === selectedTabId.value))) {
    selectedTabId.value = firstTab.id
  }
})
</script>
