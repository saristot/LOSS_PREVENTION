<template>
  <v-dialog v-model="model" max-width="600px" persistent>
    <v-card>
      <v-card-title>
        <span class="text-h6">Configure Chart</span>
      </v-card-title>

      <v-card-text>
        <v-form ref="form">
          <v-text-field label="Chart Title" v-model="config.title" :rules="[requiredRule]" />
          <v-select label="Chart Type" :items="chartTypes" v-model="config.chartType" />
          <v-select label="X Axis Field" :items="availableFields" v-model="config.xField" :rules="[requiredRule]" />
          <v-text-field label="X Axis Label" v-model="config.xLabel" />
          <v-select label="Y Axis Field" :items="availableFields" v-model="config.yField" :rules="[requiredRule]" />
          <v-text-field label="Y Axis Label" v-model="config.yLabel" />
          <v-text-field label="Series Label" v-model="config.label" :rules="[requiredRule]" />
          <v-select label="Aggregation" :items="['', 'sum', 'avg', 'min', 'max', 'count']"
            v-model="config.aggregation" />
          <v-color-picker v-model="config.color" flat hide-canvas />
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
import { ref, watch, reactive } from 'vue';

const props = defineProps({
  modelValue: Boolean,
  initialConfig: Object,
  availableFields: Array
});
const emit = defineEmits(['update:modelValue', 'save', 'cancel']);

const model = ref(props.modelValue);
const config = reactive({});
const form = ref(null);

watch(() => props.modelValue, (v) => {
  model.value = v;
  if (v) {
    const defaults = {
      title: '',
      chartType: 'bar',
      xField: '',
      xLabel: '',
      yField: '',
      yLabel: '',
      label: '',
      aggregation: '',
      color: '#42A5F5'
    };
    Object.assign(config, defaults, props.initialConfig || {});
  }
});

watch(model, v => emit('update:modelValue', v));

const chartTypes = ['bar', 'line', 'pie', 'doughnut', 'radar'];
const requiredRule = v => !!v || 'This field is required';

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
.v-card-text>* {
  margin-bottom: 1rem;
}
</style>
