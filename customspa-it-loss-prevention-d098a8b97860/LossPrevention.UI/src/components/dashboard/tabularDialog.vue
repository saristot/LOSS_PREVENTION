<template>
  <v-dialog v-model="model" max-width="800px" persistent>
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
          <v-text-field
            label="Rows per Page"
            v-model.number="config.pageSize"
            type="number"
            :rules="[requiredRule, minRule]"
            hint="Number of rows to display (minimum 1)"
          />

          <v-divider class="my-4" />
          
          <div class="text-subtitle-2 mb-2">Additional Filters</div>
          <p class="text-caption text-grey mb-3">
            Add extra conditions to filter the data (these will be combined with the tab's existing query)
          </p>
          
          <ConditionGroup 
            v-if="config.filters"
            :group="config.filters" 
            @update="updateFilters"
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
import { ref, reactive, watch, onMounted } from 'vue';
import ConditionGroup from '../reports/conditionGroup.vue';
import { useMappingStore } from '@/stores/mappingStore';

const props = defineProps({
  modelValue: Boolean,
  selectedTab: Object,
  initialConfig: Object 
});
const emit = defineEmits(['update:modelValue', 'save', 'cancel']);

const mappingStore = useMappingStore();

const model = ref(props.modelValue);
const form = ref(null);
const config = reactive({
  title: '',
  description: '',
  pageSize: 50,
  filters: { type: 'AND', conditions: [] }
});

const requiredRule = v => !!v || 'This field is required';
const minRule = v => v >= 1 || 'Must be at least 1';

onMounted(async () => {
  await mappingStore.fetchMappings();
});

watch(() => props.modelValue, (v) => {
  model.value = v;
  if (v && props.selectedTab) {
    config.title = props.selectedTab.title || '';
    config.description = props.selectedTab.description || '';
  }
  if (v && props.initialConfig) {
    config.pageSize = props.initialConfig.pageSize || 50;
    config.filters = props.initialConfig.filters || { type: 'AND', conditions: [] };
  }
});

watch(model, v => emit('update:modelValue', v));

const updateFilters = (newFilters) => {
  config.filters = newFilters;
};

const apply = async () => {
  const result = await form.value?.validate();
  if (result?.valid) {
    emit('save', {
      ...config,
      tabId: props.selectedTab?.id 
    });
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
