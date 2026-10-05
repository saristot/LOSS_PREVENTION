<template>
  <div>
    <!-- Editor bar (Design Mode only) -->
    <div v-if="isDesignMode" class="px-4 mt-2">
      <DashboardEditorBar
        :name="dashStore.current?.name || ''"
        :dirty="dashStore.isDirty"
        :saving="dashStore.saving"
        :canDelete="!!dashStore.current?.id"
        :canRevert="dashStore.isDirty"
        @rename="val => dashStore.setMeta({ name: val })"
        @save="onSave"
        @revert="onRevert"
        @delete="onDelete"
        @add="showAddDashboardDialog = true"
        @new="createNew"
      />
    </div>

    <v-progress-linear indeterminate v-if="isLoadingDashboard" />

    <!-- Header -->
    <DashboardHeader
      :workspaces="workspaceStore.workspaces.map(ws => ({ ...ws, title: ws.name }))"
      :selectedWorkspace="selectedWorkspace"
      :selectedTab="selectedTab"
      :isDesignMode="isDesignMode"
      @toggleMode="isDesignMode = !isDesignMode"
      @workspaceChange="handleWorkspaceChange"
      @tabChange="loadTab"
    />

    <!-- Add block buttons -->
    <div v-if="isDesignMode">
      <v-btn class="ma-4 bg-primary" @click="addBlock('chart')">
        <v-icon start>mdi-chart-bar</v-icon>
        Add Chart
      </v-btn>
      <v-btn class="ma-4 bg-primary" @click="addBlock('text')">
        <v-icon start>mdi-text</v-icon>
        Add Text
      </v-btn>
      <v-btn class="ma-4 bg-primary" @click="addBlock('image')">
        <v-icon start>mdi-image</v-icon>
        Add Image
      </v-btn>
      <v-btn class="ma-4 bg-primary" @click="addBlock('tabular')">
        <v-icon start>mdi-table</v-icon>
        Add Tabular
      </v-btn>
    </div>

    <!-- Grid --> 
    <DashboardGrid
      :blocks="currentBlocks"
      :inDesignMode="isDesignMode"
      :selectedTab="selectedTab" 
      @updateBlocks="updateBlocks"
      @editBlock="openEditor"
      @deleteBlock="deleteBlock"
    />

    

    <!-- Config dialogs -->
    <ChartConfigDialog
      v-model="showChartDialog"
      :initialConfig="editingBlock?.data || {}"
      :availableFields="selectedTab?.selectedFields?.map((f: SelectedField) => f.alias) || []"
      @save="saveConfig"
    />
    <TextConfigDialog
      v-model="showTextDialog"
      :initialConfig="editingBlock?.data || {}"
      @save="saveConfig"
    />
    <ImageConfigDialog
      v-model="showImageDialog"
      :initialConfig="editingBlock?.data || {}"
      @save="saveConfig"
    />
    <TabularDialog
      v-model="showTabularDialog"
      :initialConfig="editingBlock?.data || {}"
      :selectedTab="selectedTab"
      @save="saveConfig"
    />

    <!-- First-save Name Modal -->
    <v-dialog v-model="showSaveDialog" max-width="480">
      <v-card>
        <v-card-title>Save Dashboard</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="saveName"
            label="Name"
            autofocus
            @keyup.enter="confirmSave"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showSaveDialog=false">Cancel</v-btn>
          <v-btn color="primary" :disabled="!saveName?.trim()" @click="confirmSave">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import DashboardHeader from './dashboardheader.vue'
import DashboardEditorBar from './dashboardEditorBar.vue'
import DashboardGrid from './dashboardGrid.vue'
import ChartConfigDialog from './chartConfigDialog.vue'
import TextConfigDialog from './textConfigDialog.vue'
import ImageConfigDialog from './imageConfigDialog.vue'
import TabularDialog from './tabularDialog.vue'

import { useWorkspaceStore } from '@/stores/workspaceStore'
import { useDashboardStore } from '@/stores/dashboardStore'

interface SelectedField {
  alias: string
}

const route = useRoute()
const router = useRouter()

const workspaceStore = useWorkspaceStore()
const dashStore = useDashboardStore()

/* ===== State ===== */
const isDesignMode = ref(true)
const selectedWorkspace = ref<any|null>(null)
const selectedTab = ref<any|null>(null)
const currentBlocks = ref<any[]>([])

const editingBlock = ref<any|null>(null)
const pendingBlockType = ref<string|null>(null)

const showChartDialog = ref(false)
const showTextDialog = ref(false)
const showImageDialog = ref(false)
const showTabularDialog = ref(false)

/* avoid undefined from EditorBar @add */
const showAddDashboardDialog = ref(false)

/* ===== Save dialog ===== */
const showSaveDialog = ref(false)
const saveName = ref('')

/* ===== Route/dashboard loading guards ===== */
const isLoadingDashboard = ref(false)
const hasLoadedFromRoute = ref(false)

/* ===== Helpers ===== */
const findNextAvailablePosition = (blocks: any[]) => {
  if (!blocks || blocks.length === 0) return 0
  return Math.max(...blocks.map((b: any) => (b.y ?? 0) + (b.h ?? 0)))
}

function updateBlocks(blocks: any[]) {
  if (currentBlocks.value !== blocks) currentBlocks.value = blocks
  dashStore.setBlocks(blocks)
  if (selectedTab.value && selectedTab.value.dashboardBlocks !== blocks) {
    selectedTab.value.dashboardBlocks = blocks
  }
}

/* ===== Add/Edit/Delete Blocks ===== */
const addBlock = (type: 'chart'|'text'|'image'|'tabular') => {
  editingBlock.value = null
  pendingBlockType.value = type

  const nextY = findNextAvailablePosition(currentBlocks.value)

  const baseBlock: any = {
    i: crypto.randomUUID(),
    x: 0,
    y: nextY,
    w: 6,
    h: 8,
    type,
    static: false,
    data: {}
  }

  if (type === 'tabular') {
    baseBlock.data = {
      title: selectedTab.value?.title || '',
      description: selectedTab.value?.description || '',
      tabId: selectedTab.value?.id
    }
  }

  editingBlock.value = baseBlock

  if (type === 'chart') showChartDialog.value = true
  if (type === 'text') showTextDialog.value = true
  if (type === 'image') showImageDialog.value = true
  if (type === 'tabular') showTabularDialog.value = true
}

function openEditor(block: any) {
  editingBlock.value = { ...block }
  const t = block?.type
  if (t === 'chart') showChartDialog.value = true
  else if (t === 'text') showTextDialog.value = true
  else if (t === 'image') showImageDialog.value = true
  else if (t === 'tabular') showTabularDialog.value = true
}

function saveConfig(cfg: any) {
  const existingIdx = currentBlocks.value.findIndex(b => b.i === editingBlock.value?.i)

  if (existingIdx >= 0) {
    currentBlocks.value[existingIdx] = { ...currentBlocks.value[existingIdx], data: cfg }
  } else {
    const type = pendingBlockType.value || editingBlock.value?.type || 'text'
    const nextY = findNextAvailablePosition(currentBlocks.value)
    const newBlock = {
      ...(editingBlock.value ?? {}),
      i: editingBlock.value?.i ?? crypto.randomUUID(),
      x: editingBlock.value?.x ?? 0,
      y: editingBlock.value?.y ?? nextY,
      w: editingBlock.value?.w ?? 6,
      h: editingBlock.value?.h ?? 8,
      type,
      static: false,
      data: cfg
    }
    currentBlocks.value.push(newBlock)
  }

  dashStore.setBlocks(currentBlocks.value)
  if (selectedTab.value && selectedTab.value.dashboardBlocks !== currentBlocks.value) {
    selectedTab.value.dashboardBlocks = currentBlocks.value
  }

  cancelConfig()
}

function cancelConfig() {
  editingBlock.value = null
  pendingBlockType.value = null
  showChartDialog.value = false
  showTextDialog.value = false
  showImageDialog.value = false
  showTabularDialog.value = false
}

function deleteBlock(block: any) {
  const idx = currentBlocks.value.findIndex(b => b.i === block.i)
  if (idx !== -1) {
    currentBlocks.value.splice(idx, 1)
    dashStore.setBlocks(currentBlocks.value)
    if (selectedTab.value && selectedTab.value.dashboardBlocks !== currentBlocks.value) {
      selectedTab.value.dashboardBlocks = currentBlocks.value
    }
  }
}

/* ===== Load dashboard by route ===== */
async function loadDashboardFromRoute() {
  const id = route.params.id as string | undefined
  if (!id) return

  isLoadingDashboard.value = true
  try {
    // requires dashStore.get(id) action
    const doc = await dashStore.get(id)
    if (!doc) {
      console.warn('Dashboard not found:', id)
      router.push('/dashboards')
      return
    }

    dashStore.load(doc)

    const ws = workspaceStore.workspaces.find((w: any) => w.id === doc.workspaceId) || null
    selectedWorkspace.value = ws

    const tab = ws?.tabs?.find((t: any) => t.id === doc.tabId) || null
    selectedTab.value = tab

    currentBlocks.value = Array.isArray(doc.blocks) ? doc.blocks : []

    hasLoadedFromRoute.value = true
  } catch (e) {
    console.error('Failed to load dashboard:', e)
  } finally {
    isLoadingDashboard.value = false
  }
}

/* ===== Persistence wiring (store) ===== */
watch(
  () => selectedTab.value?.id,
  () => {
    if (hasLoadedFromRoute.value) return
    const wsId = selectedWorkspace.value?.id
    const tabId = selectedTab.value?.id
    if (!wsId || !tabId) return

    const existing = (selectedTab.value as any)?.dashboardMeta?.savedDashboard
    const model = existing ?? {
      name: 'Untitled Dashboard',
      published: false,
      workspaceId: wsId,
      tabId,
      blocks: currentBlocks.value ?? []
    }
    dashStore.load(model)
  },
  { immediate: false }
)

watch(currentBlocks, (blocks) => {
  dashStore.setBlocks(blocks)
}, { deep: true })

/* ===== Save/Revert/Delete ===== */
async function onSave() {
  const name = dashStore.current?.name?.trim()
  if (!name) {
    saveName.value = name || ''
    showSaveDialog.value = true
    return
  }

  await dashStore.save({
    name,
    blocks: currentBlocks.value,
    workspaceId: selectedWorkspace.value?.id,
    tabId: selectedTab.value?.id
  })
}

async function confirmSave() {
  const name = (saveName.value || '').trim()
  if (!name) return

  dashStore.setMeta({ name })

  await dashStore.save({
    name,
    blocks: currentBlocks.value,
    workspaceId: selectedWorkspace.value?.id,
    tabId: selectedTab.value?.id
  })

  showSaveDialog.value = false
}

async function onRevert() {
  await dashStore.revert()
  if (Array.isArray(dashStore.blocks)) {
    currentBlocks.value = dashStore.blocks
  }
}

async function onDelete() {
  if (!dashStore.current?.id) return
  await dashStore.remove(dashStore.current.id)
  await dashStore.clearCurrent()
  currentBlocks.value = []
  router.push('/dashboards')
}

function createNew() {
  dashStore.clearCurrent()
  currentBlocks.value = []
}

/* ===== Workspace/Tab navigation ===== */
function setBlocksFromTab(tab: any) {
  const incoming = tab?.dashboardBlocks ?? []
  if (currentBlocks.value !== incoming &&
      JSON.stringify(currentBlocks.value) !== JSON.stringify(incoming)) {
    currentBlocks.value = incoming
    dashStore.setBlocks(currentBlocks.value)
  }
}

const handleWorkspaceChange = (workspaceId: string) => {
  if (selectedWorkspace.value?.id === workspaceId) return

  const ws = workspaceStore.workspaces.find((w: any) => w.id === workspaceId)
  if (!ws) {
    selectedWorkspace.value = null
    selectedTab.value = null
    currentBlocks.value = []
    dashStore.clearCurrent()
    return
  }

  selectedWorkspace.value = ws

  const keepTab = ws.tabs?.find((t: any) => t.id === selectedTab.value?.id)
  const firstTab = keepTab ?? ws.tabs?.[0] ?? null

  if (firstTab) {
    if (selectedTab.value?.id !== firstTab.id) {
      selectedTab.value = firstTab
    }
    setBlocksFromTab(firstTab)
  } else {
    selectedTab.value = null
    currentBlocks.value = []
    dashStore.clearCurrent()
  }
}

const loadTab = (tabId: string) => {
  if (!selectedWorkspace.value) return
  const tab = selectedWorkspace.value.tabs?.find((t: any) => t.id === tabId) || null
  selectedTab.value = tab
  setBlocksFromTab(tab)
}

/* ===== Dirty-leave protection ===== */
function beforeUnload(e: BeforeUnloadEvent) {
  if (dashStore.isDirty) {
    e.preventDefault()
    e.returnValue = ''
  }
}

/* ===== Bootstrapping ===== */
onMounted(async () => {
  await workspaceStore.fetchWorkspaces()
  await loadDashboardFromRoute()

  watch(() => route.params.id, async () => {
    hasLoadedFromRoute.value = false
    await loadDashboardFromRoute()
  })

  window.addEventListener('beforeunload', beforeUnload)
})

onBeforeUnmount(() => window.removeEventListener('beforeunload', beforeUnload))
onUnmounted(() => window.removeEventListener('beforeunload', beforeUnload))
</script>
