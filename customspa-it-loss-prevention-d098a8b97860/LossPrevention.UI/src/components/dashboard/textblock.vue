<template>
  <div class="text-block border rounded shadow-md bg-white h-full w-full p-6 relative">
    <div class="text-toolbar" v-if="inDesignMode">
      <v-btn icon size="small" @click="$emit('edit')">
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
      <v-btn icon size="small" @click="$emit('delete')">
        <v-icon>mdi-delete</v-icon>
      </v-btn>
    </div>
    <div class="flex flex-col h-full justify-start">
      <h3 class="font-bold text-lg mb-2">{{ block.data.title }}</h3>
      <div class="text-sm text-gray-600 whitespace-pre-line">{{ block.data.description }}</div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  block: Object,
  inDesignMode: Boolean
});
const emit = defineEmits(['edit', 'delete']);
</script>

<style scoped>
.text-block {
  box-sizing: border-box;
  height: 100%;
  padding: 20px;
}

.text-toolbar {
  position: absolute;
  top: 4px;
  right: 4px;
  z-index: 10;
  display: flex;
  gap: 4px;
}
</style>
