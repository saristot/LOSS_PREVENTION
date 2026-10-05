<template>
  <v-dialog v-model="dialogOpen" max-width="1200px" scrollable persistent>
    <v-card>
      <v-card-title class="d-flex align-center justify-space-between bg-warning pa-4">
        <div class="d-flex align-center">
          <v-icon class="mr-2">mdi-cog</v-icon>
          <span>Fraud Detection Settings</span>
          <v-chip v-if="fraudStore.lastSavedAt" size="small" class="ml-3" color="success" variant="outlined">
            <v-icon size="x-small" class="mr-1">mdi-check</v-icon>
            Saved {{ formatLastSaved(fraudStore.lastSavedAt) }}
          </v-chip>
        </div>
        <v-btn icon variant="text" @click="closeDialog">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>

      <v-card-text class="pa-4" style="max-height: 70vh;">
        <v-alert type="info" variant="tonal" class="mb-4">
          <strong>Configure fraud detection thresholds</strong><br>
          Adjust these settings to fine-tune fraud detection sensitivity. Click "Save to Server" to persist changes.
        </v-alert>

        <v-tabs v-model="activeTab" color="warning">
          <v-tab v-for="category in fraudStore.thresholdCategories" :key="category.category" :value="category.category">
            {{ category.category }}
          </v-tab>
        </v-tabs>

        <v-window v-model="activeTab" class="mt-4">
          <v-window-item v-for="category in fraudStore.thresholdCategories" :key="category.category" :value="category.category">
            <v-expansion-panels multiple>
              <v-expansion-panel v-for="item in category.items" :key="item.key">
                <v-expansion-panel-title>
                  <div class="d-flex align-center justify-space-between w-100">
                    <div>
                      <v-icon class="mr-2" size="small">mdi-shield-alert</v-icon>
                      <strong>{{ item.label }}</strong>
                    </div>
                    <v-btn 
                      size="x-small" 
                      variant="text" 
                      color="primary" 
                      @click.stop="resetThreshold(item.key)"
                      class="mr-2"
                    >
                      <v-icon size="small">mdi-restore</v-icon>
                      Reset
                    </v-btn>
                  </div>
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                  <v-card variant="outlined" class="mb-3">
                    <v-card-text>
                      <div class="mb-2">
                        <strong>Description:</strong> {{ item.config.description }}
                      </div>
                      <div class="text-error">
                        <strong>Detects:</strong> {{ item.config.detects }}
                      </div>
                    </v-card-text>
                  </v-card>

                  <!-- Dynamic threshold inputs based on configuration -->
                  <v-row>
                    <v-col v-if="item.config.percentile !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.percentile"
                        label="Percentile Threshold"
                        type="number"
                        min="0"
                        max="1"
                        step="0.01"
                        density="compact"
                        variant="outlined"
                        hint="0.95 = top 5%"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.stdDevMultiplier !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.stdDevMultiplier"
                        label="Std Dev Multiplier"
                        type="number"
                        min="0"
                        step="0.1"
                        density="compact"
                        variant="outlined"
                        hint="Standard deviations above mean"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minimumValue !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minimumValue"
                        label="Minimum Value"
                        type="number"
                        min="0"
                        density="compact"
                        variant="outlined"
                        hint="Absolute minimum to flag"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.multiplier !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.multiplier"
                        label="Multiplier"
                        type="number"
                        min="1"
                        step="0.1"
                        density="compact"
                        variant="outlined"
                        hint="Multiplication factor"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.meanMultiplier !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.meanMultiplier"
                        label="Mean Multiplier"
                        type="number"
                        min="0"
                        step="0.1"
                        density="compact"
                        variant="outlined"
                        hint="Factor of mean value"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minMultiplier !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minMultiplier"
                        label="Min Multiplier"
                        type="number"
                        min="0"
                        step="0.1"
                        density="compact"
                        variant="outlined"
                        hint="Factor of minimum value"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minimum !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minimum"
                        label="Minimum Count"
                        type="number"
                        min="0"
                        density="compact"
                        variant="outlined"
                        hint="Minimum occurrences"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.windowMinutes !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.windowMinutes"
                        label="Time Window (minutes)"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Time period to analyze"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minTransactions !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minTransactions"
                        label="Min Transactions"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Minimum transaction count"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.offHoursStart !== undefined" cols="12" md="6">
                      <v-text-field
                        v-model.number="item.config.offHoursStart"
                        label="Off-Hours Start (24h)"
                        type="number"
                        min="0"
                        max="23"
                        density="compact"
                        variant="outlined"
                        hint="Hour when off-hours begin (e.g., 22 = 10 PM)"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.offHoursEnd !== undefined" cols="12" md="6">
                      <v-text-field
                        v-model.number="item.config.offHoursEnd"
                        label="Off-Hours End (24h)"
                        type="number"
                        min="0"
                        max="23"
                        density="compact"
                        variant="outlined"
                        hint="Hour when off-hours end (e.g., 6 = 6 AM)"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.weekendPenalty !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.weekendPenalty"
                        label="Weekend Penalty"
                        type="number"
                        min="0"
                        max="1"
                        step="0.1"
                        density="compact"
                        variant="outlined"
                        hint="Severity reduction for weekends"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.thresholdProximity !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.thresholdProximity"
                        label="Threshold Proximity"
                        type="number"
                        min="0"
                        max="1"
                        step="0.01"
                        density="compact"
                        variant="outlined"
                        hint="% proximity to threshold (0.1 = 10%)"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minSplits !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minSplits"
                        label="Min Splits"
                        type="number"
                        min="2"
                        density="compact"
                        variant="outlined"
                        hint="Minimum transactions for split detection"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.minOccurrences !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.minOccurrences"
                        label="Min Occurrences"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Minimum pattern occurrences"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.discountPercentile !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.discountPercentile"
                        label="Discount Percentile"
                        type="number"
                        min="0"
                        max="1"
                        step="0.01"
                        density="compact"
                        variant="outlined"
                        hint="Discount threshold percentile"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.timeWindowDays !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.timeWindowDays"
                        label="Time Window (days)"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Lookback period in days"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.returnRatePercentile !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.returnRatePercentile"
                        label="Return Rate Percentile"
                        type="number"
                        min="0"
                        max="1"
                        step="0.01"
                        density="compact"
                        variant="outlined"
                        hint="Return rate threshold"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.returnAmountRatio !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.returnAmountRatio"
                        label="Return Amount Ratio"
                        type="number"
                        min="0"
                        max="1"
                        step="0.05"
                        density="compact"
                        variant="outlined"
                        hint="Max return/purchase ratio (0.5 = 50%)"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.frequentReturner !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.frequentReturner"
                        label="Frequent Returner Threshold"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Number of returns to flag"
                        persistent-hint
                      />
                    </v-col>

                    <v-col v-if="item.config.windowDays !== undefined" cols="12" md="4">
                      <v-text-field
                        v-model.number="item.config.windowDays"
                        label="Window (days)"
                        type="number"
                        min="1"
                        density="compact"
                        variant="outlined"
                        hint="Analysis window in days"
                        persistent-hint
                      />
                    </v-col>

                    <!-- Common thresholds array (special handling) -->
                    <v-col v-if="item.config.commonThresholds !== undefined" cols="12">
                      <v-text-field
                        :model-value="item.config.commonThresholds.join(', ')"
                        @update:model-value="updateCommonThresholds(item.key, $event)"
                        label="Common Thresholds"
                        density="compact"
                        variant="outlined"
                        hint="Comma-separated values (e.g., 50, 100, 200, 500)"
                        persistent-hint
                      />
                    </v-col>
                  </v-row>
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </v-window-item>
        </v-window>
      </v-card-text>

      <v-card-actions class="pa-4">
        <v-btn color="error" variant="outlined" @click="resetAllThresholds">
          <v-icon left>mdi-restore</v-icon>
          Reset All to Defaults
        </v-btn>
        <v-spacer />
        <v-btn color="primary" variant="outlined" @click="exportThresholds">
          <v-icon left>mdi-export</v-icon>
          Export
        </v-btn>
        <v-btn color="primary" variant="outlined" @click="importThresholds">
          <v-icon left>mdi-import</v-icon>
          Import
        </v-btn>
        <v-btn 
          color="success" 
          variant="outlined" 
          @click="saveToServer"
          :loading="fraudStore.savingSettings"
        >
          <v-icon left>mdi-cloud-upload</v-icon>
          Save
        </v-btn>
        <v-btn color="primary" @click="saveAndClose">
          <v-icon left>mdi-check</v-icon>
          Apply & Close
        </v-btn>
      </v-card-actions>
    </v-card>

    <!-- Import dialog -->
    <v-dialog v-model="importDialog" max-width="600px">
      <v-card>
        <v-card-title>Import Thresholds</v-card-title>
        <v-card-text>
          <v-textarea
            v-model="importJson"
            label="Paste JSON configuration"
            rows="10"
            variant="outlined"
            placeholder='{ "highValue": { "percentile": 0.95, ... }, ... }'
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="importDialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="doImport">Import</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar for feedback -->
    <v-snackbar v-model="snackbar" :color="snackbarColor" timeout="3000">
      {{ snackbarMessage }}
    </v-snackbar>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useFraudDetectionStore } from '@/stores/fraudDetectionStore';

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:open', value: boolean): void;
}>();

const fraudStore = useFraudDetectionStore();

const activeTab = ref('Transaction Values');
const importDialog = ref(false);
const importJson = ref('');
const snackbar = ref(false);
const snackbarMessage = ref('');
const snackbarColor = ref('success');

const dialogOpen = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value)
});

// Load settings from API when component mounts
onMounted(async () => {
  await loadSettingsFromServer();
});

async function loadSettingsFromServer() {
  try {
    await fraudStore.loadSettings();
  } catch (error) {
    console.error('Failed to load settings from server:', error);
    // Silently fail - already using defaults
  }
}

async function saveToServer() {
  try {
    const success = await fraudStore.saveSettings();
    if (success) {
      showSnackbar('Settings saved to server successfully', 'success');
    } else {
      showSnackbar('Failed to save settings to server', 'error');
    }
  } catch (error: any) {
    showSnackbar(error.message || 'Failed to save settings to server', 'error');
  }
}

function formatLastSaved(date: Date): string {
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  return `${days}d ago`;
}

function closeDialog() {
  dialogOpen.value = false;
}

function resetThreshold(key: string) {
  fraudStore.resetThreshold(key as any);
  showSnackbar('Threshold reset to default', 'info');
}

function resetAllThresholds() {
  if (confirm('Are you sure you want to reset all thresholds to their default values?')) {
    fraudStore.resetToDefaults();
    showSnackbar('All thresholds reset to defaults', 'success');
  }
}

function updateCommonThresholds(key: string, value: string) {
  try {
    const thresholds = value.split(',').map(v => parseFloat(v.trim())).filter(v => !isNaN(v));
    if (thresholds.length > 0) {
      fraudStore.updateThreshold(key as any, { commonThresholds: thresholds });
    }
  } catch (error) {
    console.error('Failed to parse common thresholds:', error);
  }
}

function exportThresholds() {
  const json = fraudStore.exportThresholds();
  const blob = new Blob([json], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `fraud-thresholds-${new Date().toISOString().split('T')[0]}.json`;
  a.click();
  URL.revokeObjectURL(url);
  showSnackbar('Thresholds exported successfully', 'success');
}

function importThresholds() {
  importDialog.value = true;
}

function doImport() {
  if (fraudStore.importThresholds(importJson.value)) {
    showSnackbar('Thresholds imported successfully. Remember to save to server.', 'success');
    importDialog.value = false;
    importJson.value = '';
  } else {
    showSnackbar('Failed to import thresholds. Check JSON format.', 'error');
  }
}

async function saveAndClose() {
  showSnackbar('Settings applied. Re-run fraud detection for changes to take effect.', 'success');
  setTimeout(() => {
    closeDialog();
  }, 1000);
}

function showSnackbar(message: string, color: string = 'success') {
  snackbarMessage.value = message;
  snackbarColor.value = color;
  snackbar.value = true;
}
</script>

<style scoped>
.w-100 {
  width: 100%;
}

:deep(.v-expansion-panel) {
  background-color: #ffffff !important;
  color: #212121 !important;
}

:deep(.v-expansion-panel-title) {
  background-color: #f5f5f5 !important;
  color: #212121 !important;
}

:deep(.v-expansion-panel-title:hover) {
  background-color: #eeeeee !important;
}

:deep(.v-expansion-panel-text__wrapper) {
  background-color: #ffffff !important;
  color: #212121 !important;
}

:deep(.v-expansion-panel-title .v-icon) {
  color: #616161 !important;
}
</style>
