<template>
  <v-dialog v-model="model" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="text-h6">Configure Image</span>
      </v-card-title>

      <v-card-text>
        <v-text-field label="Image URL" v-model="config.url" />
        <v-text-field label="Alt Text" v-model="config.alt" />
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn color="primary" @click="apply">Apply</v-btn>
        <v-btn text @click="model = false">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';

const props = defineProps({
  modelValue: Boolean,
  initialConfig: Object
});
const emit = defineEmits(['update:modelValue', 'save']);

const model = ref(props.modelValue);
const config = reactive({ ...props.initialConfig });

watch(() => props.modelValue, v => model.value = v);
watch(model, v => emit('update:modelValue', v));

const apply = () => {
  emit('save', { ...config });
  model.value = false;
};
</script>

<style scoped>
.v-card-text > * {
  margin-bottom: 1rem;
}
</style>
