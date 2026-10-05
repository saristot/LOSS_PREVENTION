<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-checkbox-group v-model="selectedSources" @change="handleCheckboxChange">
          <v-checkbox label="Daily" value="daily"></v-checkbox>
          <v-checkbox label="Weekly" value="weekly"></v-checkbox>
          <v-checkbox label="Monthly" value="monthly"></v-checkbox>
        </v-checkbox-group>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-date-picker v-model="scheduleDate" @change="handleScheduleChange"></v-date-picker>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-time-picker v-model="scheduleTime" format="24hr" @change="handleScheduleChange"></v-time-picker>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue';
import useDataIngestionStore from '@/store/useDataIngestionStore';

export default defineComponent({
  setup() {
    const store = useDataIngestionStore();
    
    const selectedSources = ref([] as string[]);
    const scheduleDate = ref<Date | null>(null);
    const scheduleTime = ref<string | null>(null);

    const handleCheckboxChange = (values: string[]) => {
      store.setSelectedSources(values);
    };

    const handleScheduleChange = () => {
      store.setSchedule({ date: scheduleDate.value, time: scheduleTime.value });
    };

    return {
      selectedSources,
      scheduleDate,
      scheduleTime,
      handleCheckboxChange,
      handleScheduleChange
    };
  }
});
</script>
