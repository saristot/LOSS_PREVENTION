<template>
  <v-dialog v-model="model" max-width="500px" persistent>
    <v-card>
      <v-card-title>
        <span class="text-h6">Configure Table View</span>
      </v-card-title>

      <v-card-text>
        <v-form ref="form">
          <v-text-field
            label="Title"
            v-model="config.title"
            :rules="[requiredRule]"
          />
          <v-textarea
            label="Description"
            v-model="config.description"
            rows="3"
          />
        </v-form>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn color="primary" @click="apply">Apply</v-btn>
        <v-btn text @click="cancel">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';

const props = defineProps({
  modelValue: Boolean,
  selectedTab: Object
});
const emit = defineEmits(['update:modelValue', 'save', 'cancel']);

const model = ref(props.modelValue);
const form = ref(null);
const config = reactive({ title: '', description: '' });

const requiredRule = v => !!v || 'This field is required';

watch(() => props.modelValue, (v) => {
  model.value = v;
  if (v && props.selectedTab) {
    // Deep clone title/description so user changes don't affect original tab
    config.title = props.selectedTab.title || '';
    config.description = props.selectedTab.description || '';
  }
});

watch(model, v => emit('update:modelValue', v));

const apply = async () => {
  const result = await form.value?.validate();
  if (result?.valid) {
    emit('save', { ...config });
    model.value = false;
  }
};

const cancel = () => {
  emit('cancel');
  model.value = false;
};
</script>

<style scoped>
.v-card-text > * {
  margin-bottom: 1rem;
}
</style>
