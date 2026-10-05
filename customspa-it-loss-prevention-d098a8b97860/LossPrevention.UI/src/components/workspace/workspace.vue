<template>
  <v-container>
    <v-row align="center" class="mb-4">
      <v-col cols="8">
        <v-text-field v-model="searchQuery" label="Search Workspaces" @input="onSearch" />
      </v-col>
      <v-col cols="4" class="text-end">
        <v-btn color="primary" @click="showDialog = true">Add Workspace</v-btn>
      </v-col>
    </v-row>

    <v-progress-linear indeterminate v-if="workspaceStore.loading" />

    <v-list v-if="workspaceStore.workspaces.length">
      <v-list-item v-for="workspace in workspaceStore.workspaces" :key="workspace.id" @click="goToWorkspace(workspace)">
        <v-list-item-title>{{ workspace.name }}</v-list-item-title>
        <v-list-item-subtitle>
          Reports: {{workspace.tabs.length > 0 ? workspace.tabs.map((tab: Tab) => tab.title).join(', ') : 'None'}}
        </v-list-item-subtitle>
        <template #append>
          <v-btn icon class="mr-4 mt-2" @click.stop="editWorkspace(workspace)">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
          <v-btn icon @click.stop="deleteWorkspace(workspace.id)">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>

    <v-alert type="info" v-else-if="!workspaceStore.loading">
      No workspaces found.
    </v-alert>

    <!-- Add/Edit Dialog -->
    <v-dialog v-model="showDialog" max-width="400px">
      <v-card>
        <v-card-title>{{ editingWorkspace ? 'Edit Workspace' : 'New Workspace' }}</v-card-title>
        <v-card-text>
          <v-text-field v-model="workspaceName" label="Name" required />
          <v-text-field v-model="workspaceDescription" label="Description" />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="resetDialog">Cancel</v-btn>
          <v-btn color="primary" :disabled="!workspaceName || saving" :loading="saving" @click="saveWorkspace">
            {{ editingWorkspace ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkspaceStore } from '@/stores/workspaceStore'
import type { Workspace } from '@/interfaces/workspace'
import type { Tab } from '@/interfaces/tab'

const router = useRouter()
const workspaceStore = useWorkspaceStore()

const searchQuery = ref('')
const showDialog = ref(false)
const editingWorkspace = ref<Workspace | null>(null)
const workspaceName = ref('')
const workspaceDescription = ref('')
const saving = ref(false)

onMounted(() => {
  workspaceStore.fetchWorkspaces()
})

function onSearch() {
  workspaceStore.fetchWorkspaces(searchQuery.value)
}

function goToWorkspace(ws: Workspace) {
  workspaceStore.setActiveWorkspace(ws)
  router.push('/query')
}

function editWorkspace(ws: Workspace) {
  editingWorkspace.value = ws
  workspaceName.value = ws.name
  workspaceDescription.value = ws.description || ''
  showDialog.value = true
}

function resetDialog() {
  editingWorkspace.value = null
  workspaceName.value = ''
  workspaceDescription.value = ''
  showDialog.value = false
  saving.value = false
}

async function saveWorkspace() {
  if (!workspaceName.value.trim()) return

  saving.value = true

  if (editingWorkspace.value) {
    await workspaceStore.updateWorkspace(editingWorkspace.value.id, {
      name: workspaceName.value.trim(),
      description: workspaceDescription.value.trim()
    })
  } else {
    await workspaceStore.addWorkspace({
      name: workspaceName.value.trim(),
      description: workspaceDescription.value.trim(),
      tabs: []
    })
  }
  resetDialog()
}

async function deleteWorkspace(id: string) {
  try {
    await workspaceStore.deleteWorkspace(id)
  } catch (error) {
    console.error('Error deleting workspace:', error)
    // You might want to show a user-friendly error message here
  }
}
</script>