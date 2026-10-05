<template>
  <div class="dashboard-grid-container">
    <GridLayout
      v-model:layout="layout"
      :col-num="12"
      :row-height="30"
      :is-draggable="inDesignMode"
      :is-resizable="inDesignMode"
      :vertical-compact="true"
      :margin="[10, 10]"
      :use-css-transforms="true"
      @layout-updated="onLayoutUpdated"
    >
      <GridItem
        v-for="item in layout"
        :key="item.i"
        :x="item.x"
        :y="item.y"
        :w="item.w"
        :h="item.h"
        :i="item.i"
        :is-draggable="inDesignMode"
        :is-resizable="inDesignMode"
      >
        <!-- Render per type so we can pass the right props -->

        <!-- Chart -->
        <ChartBlock
          v-if="item.type === 'chart'"
          :block="item"
          :inDesignMode="inDesignMode"
          @edit="$emit('editBlock', item)"
          @delete="$emit('deleteBlock', item)"
          @config-updated="updateBlockData($event, item.i)"
        />

        <!-- Text -->
        <TextBlock
          v-else-if="item.type === 'text'"
          :block="item"
          :inDesignMode="inDesignMode"
          @edit="$emit('editBlock', item)"
          @delete="$emit('deleteBlock', item)"
          @config-updated="updateBlockData($event, item.i)"
        />

        <!-- Image -->
        <ImageBlock
          v-else-if="item.type === 'image'"
          :block="item"
          :inDesignMode="inDesignMode"
          @edit="$emit('editBlock', item)"
          @delete="$emit('deleteBlock', item)"
          @config-updated="updateBlockData($event, item.i)"
        />

        <!-- Tabular (needs the current tab as a fallback) -->
        <TabularBlock
          v-else-if="item.type === 'tabular'"
          :block="item"
          :inDesignMode="inDesignMode"
          :selectedTab="selectedTab"
          @edit="$emit('editBlock', item)"
          @delete="$emit('deleteBlock', item)"
          @config-updated="updateBlockData($event, item.i)"
        />

        <!-- Fallback to text if type is unknown -->
        <TextBlock
          v-else
          :block="item"
          :inDesignMode="inDesignMode"
          @edit="$emit('editBlock', item)"
          @delete="$emit('deleteBlock', item)"
          @config-updated="updateBlockData($event, item.i)"
        />
      </GridItem>
    </GridLayout>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import { GridLayout, GridItem } from 'vue-grid-layout-v3'
import ChartBlock from './chartBlock.vue'
import TextBlock from './textblock.vue'
import ImageBlock from './imageblock.vue'
import TabularBlock from './tabularBlock.vue'

const props = defineProps({
  blocks: { type: Array, required: true },
  inDesignMode: { type: Boolean, default: false },
  // NEW: pass current tab from the page so Tabular can build a pipeline even
  // when block.data.tabId is empty or workspaces load late
  selectedTab: { type: Object, default: null }
})

const emit = defineEmits(['updateBlocks', 'editBlock', 'deleteBlock'])

const layout = ref([])
let syncing = false
let updating = false

// keep layout in sync with parent blocks
watch(
  () => props.blocks,
  (newBlocks) => {
    if (updating) return
    syncing = true
    layout.value = (newBlocks || []).map(b => ({
      i: b.i,
      x: b.x ?? 0,
      y: b.y ?? 0,
      w: b.w ?? 3,
      h: b.h ?? 2,
      type: b.type || 'text',
      data: b.data || {}
    }))
    nextTick(() => (syncing = false))
  },
  { immediate: true, deep: true }
)

// when user drags/resizes, emit full updated blocks back up
const onLayoutUpdated = (newLayout) => {
  if (syncing) return
  updating = true

  const updatedBlocks = newLayout.map(item => {
    const original = (props.blocks || []).find(b => b.i === item.i) || {}
    return {
      ...original,
      i: item.i,
      x: item.x,
      y: item.y,
      w: item.w,
      h: item.h,
      type: original.type || 'text',
      data: original.data || {}
    }
  })

  emit('updateBlocks', updatedBlocks)
  updating = false
}

// child blocks will send back their new config; store it under .data
const updateBlockData = (newConfig, blockId) => {
  const updatedBlocks = (props.blocks || []).map(b =>
    b.i === blockId ? { ...b, data: newConfig || {} } : b
  )
  emit('updateBlocks', updatedBlocks)
}
</script>

<style scoped>
.dashboard-grid-container { width: 100%; }
</style>
