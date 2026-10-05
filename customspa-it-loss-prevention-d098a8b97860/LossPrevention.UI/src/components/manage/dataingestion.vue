<template>
    <v-container class="data-ingestion-container">
        <h2>Data Ingestion</h2>

        <v-card outlined class="mt-4">
            <v-card-title>Data Source Configuration</v-card-title>
            <v-card-text>
                <v-select v-model="selectedSource"
                          :items="sourceOptions"
                          item-title="title"
                          item-value="value"
                          label="Source"
                          outlined
                          hide-details
                          :error-messages="sourceError"
                          class="mb-4" />
                <v-select v-model="store.selectedFileType"
                          label="File Type"
                          outlined
                          :items="fileTypeOptions"
                          item-title="title"
                          item-value="value"
                          hide-details
                          :error-messages="fileTypeError" />

                <div v-if="selectedSource === 'sftp'" class="settings-container mt-4">
                    <v-divider class="mb-4"></v-divider>
                    <div class="text-subtitle-2 mb-3">SFTP Configuration</div>
                    <v-text-field v-model="store.sftpHost"
                                  label="Host"
                                  outlined
                                  :error-messages="sftpHostError" />
                    <v-text-field v-model.number="store.sftpPort"
                                  type="number"
                                  label="Port"
                                  outlined
                                  :error-messages="sftpPortError" />
                    <v-text-field v-model="store.sftpUsername"
                                  label="Username"
                                  outlined
                                  :error-messages="sftpUsernameError" />
                    <v-text-field v-model="store.sftpPassword"
                                  :type="showSftpPassword ? 'text' : 'password'"
                                  label="Password"
                                  outlined
                                  :error-messages="sftpPasswordError">
                        <template v-slot:append-inner>
                            <v-icon @click="showSftpPassword = !showSftpPassword"
                                    style="cursor: pointer;">
                                {{ showSftpPassword ? 'mdi-eye-off' : 'mdi-eye' }}
                            </v-icon>
                        </template>
                    </v-text-field>
                    <v-text-field v-model="store.sftpRemoteDirectory"
                                  label="Remote Directory"
                                  outlined
                                  :error-messages="sftpRemoteDirectoryError" />
                </div>

                <div v-else-if="selectedSource === 'filesystem'" class="settings-container mt-4">
                    <v-divider class="mb-4"></v-divider>
                    <div class="text-subtitle-2 mb-3">File System Configuration</div>
                    <v-text-field v-model="store.fileSystemPath"
                                  label="Path"
                                  outlined
                                  :error-messages="fileSystemPathError" />
                </div>
            </v-card-text>
        </v-card>

        <v-card outlined class="mt-4">
            <v-card-title>Field Mapping Configuration</v-card-title>
            <v-card-text>
                <v-switch v-model="store.useMappings"
                          label="Use Field Mappings"
                          color="primary"
                          hide-details />
                <div v-if="store.useMappings" class="text-caption text-medium-emphasis mt-2">
                    Field mappings will be applied during data ingestion
                </div>
            </v-card-text>
        </v-card>

        <!-- NEW: Manual Load Toggle -->
        <v-card outlined class="mt-4">
            <v-card-title>Load Configuration</v-card-title>
            <v-card-text>
                <v-switch v-model="manualLoad"
                          label="Manual Load Only"
                          color="primary"
                          hide-details />
                <div class="text-caption text-medium-emphasis mt-2">
                    {{
 manualLoad
            ? 'Schedule configuration is optional. Use the RUN button to load data manually.'
            : 'Schedule is required. Data will be loaded automatically based on the schedule.'
                    }}
                </div>
            </v-card-text>
        </v-card>

        <!-- Schedule section - conditionally shown/styled based on manualLoad -->
        <div class="schedule-display mt-4">
            <v-card outlined :class="{ 'optional-section': manualLoad }">
                <v-card-title>
                    Schedule Configuration
                    <span v-if="manualLoad" class="text-caption text-medium-emphasis ml-2">(Optional)</span>
                </v-card-title>
                <v-card-text>
                    <div class="d-flex align-center justify-space-between">
                        <div>
                            <div class="text-subtitle-2">Schedule Status</div>
                            <div class="text-body-1">{{ store.formattedSchedule }}</div>
                            <div v-if="scheduleError" class="text-error text-caption mt-1">{{ scheduleError }}</div>
                        </div>
                        <v-btn color="primary" @click="store.toggleScheduleMenu()" class="mr-2">
                            {{ hasSchedule ? 'Edit Schedule' : 'Set Schedule' }}
                        </v-btn>
                    </div>
                </v-card-text>
            </v-card>
        </div>

        <v-dialog v-model="store.scheduleMenuVisible" max-width="700px">
            <v-card>
                <v-card-title>Schedule Data Ingestion</v-card-title>
                <v-card-text>
                    <v-radio-group v-model="tempScheduleType" label="Schedule Type">
                        <v-radio label="One-time" value="one-time"></v-radio>
                        <v-radio label="Recurring" value="recurring"></v-radio>
                    </v-radio-group>

                    <v-row v-if="tempScheduleType === 'one-time'">
                        <v-col cols="12" md="6">
                            <div class="text-subtitle-2 mb-2">Select Date</div>
                            <v-date-picker v-model="tempScheduleDate"
                                           full-width
                                           :min="new Date().toISOString().substr(0, 10)" />
                        </v-col>
                        <v-col cols="12" md="6">
                            <div class="text-subtitle-2 mb-2">Select Time</div>
                            <v-text-field v-model="tempScheduleTime"
                                          type="time"
                                          label="Time"
                                          outlined
                                          hide-details />
                        </v-col>
                    </v-row>

                    <div v-else-if="tempScheduleType === 'recurring'">
                        <v-row>
                            <v-col cols="12">
                                <div class="text-subtitle-2 mb-2">Recurrence Pattern</div>
                                <v-select v-model="tempRecurrence"
                                          :items="store.recurrenceOptions"
                                          item-title="title"
                                          item-value="value"
                                          outlined
                                          hide-details />
                            </v-col>

                            <v-col cols="12" v-if="tempRecurrence === 'weekly'">
                                <div class="text-subtitle-2 mb-2">Select Days of Week</div>
                                <v-chip-group v-model="tempSelectedDaysOfWeek" column multiple>
                                    <v-chip v-for="day in store.daysOfWeek"
                                            :key="day.value"
                                            :value="day.value"
                                            filter
                                            outlined>
                                        {{ day.title }}
                                    </v-chip>
                                </v-chip-group>
                            </v-col>

                            <v-col cols="12" md="6">
                                <div class="text-subtitle-2 mb-2">Select Time</div>
                                <v-text-field v-model="tempScheduleTime"
                                              type="time"
                                              label="Time"
                                              outlined
                                              hide-details />
                            </v-col>
                        </v-row>
                    </div>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="cancelSchedule">Cancel</v-btn>
                    <v-btn color="primary" :disabled="!isScheduleValid" @click="confirmSchedule">
                        Set Schedule
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <div class="action-buttons mt-4">
            <v-btn :loading="isSaving" :disabled="!isFormValid || ingestionRunning" @click="save" color="primary" class="mr-2">Save</v-btn>
            <v-btn :disabled="!isFormValid || ingestionRunning" @click="run" color="success">Run</v-btn>
        </div>

        <v-alert v-if="ingestionRunning" type="info" variant="tonal" class="mt-4" prominent>
            <div class="d-flex align-center">
                <v-progress-circular indeterminate size="20" width="2" class="mr-3" />
                <div>
                    <strong>Data ingestion is running in the background.</strong>
                    <div class="text-body-2">You can navigate away — results will be shown when complete.</div>
                </div>
            </div>
        </v-alert>
    </v-container>
</template>

<script lang="ts">
    import { ref, computed, watch, onMounted, inject } from 'vue';
    import { useDataIngestionStore } from '@/stores/dataIngestionStore';
    import api from '@/api/api';

    export default {
        setup() {
            const store = useDataIngestionStore();

            // Inject the global toast function from App.vue
            // If not available, create a fallback
            const toast = inject<(text: string, color?: string) => void>('toast',
                (text: string, color = 'primary') => {
                    console.log(`[Toast ${color}]:`, text);
                }
            );

            const selectedSource = ref('');
            const manualLoad = ref(true); // Default true — schedule optional unless explicitly required
            const tempScheduleType = ref<'one-time' | 'recurring'>('one-time');
            const tempScheduleDate = ref<any>(null);
            const tempScheduleTime = ref<string>('');
            const tempRecurrence = ref('daily');
            const tempSelectedDaysOfWeek = ref<string[]>([]);
            const validationTriggered = ref(false);

            const sourceOptions = computed(() => {
                return store.dataSources;
            });

            const fileTypeOptions = computed(() => {
                return store.fileTypes;
            });

            const hasSchedule = computed(() => {
                return !!store.scheduleTime;
            });

            const isScheduleValid = computed(() => {
                if (!tempScheduleTime.value) return false;

                if (tempScheduleType.value === 'one-time' && !tempScheduleDate.value) {
                    return false;
                }

                if (tempScheduleType.value === 'recurring' &&
                    tempRecurrence.value === 'weekly' &&
                    tempSelectedDaysOfWeek.value.length === 0) {
                    return false;
                }

                return true;
            });

            const sourceError = computed(() => {
                if (!validationTriggered.value) return '';
                return !selectedSource.value ? 'Source is required' : '';
            });

            const fileTypeError = computed(() => {
                if (!validationTriggered.value) return '';
                return !store.selectedFileType ? 'File type is required' : '';
            });

            const sftpHostError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'sftp') return '';
                return !store.sftpHost ? 'Host is required' : '';
            });

            const sftpPortError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'sftp') return '';
                if (!store.sftpPort) return 'Port is required';
                if (store.sftpPort < 1 || store.sftpPort > 65535) return 'Port must be between 1 and 65535';
                return '';
            });

            const sftpUsernameError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'sftp') return '';
                return !store.sftpUsername ? 'Username is required' : '';
            });

            const sftpPasswordError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'sftp') return '';
                return !store.sftpPassword ? 'Password is required' : '';
            });

            const sftpRemoteDirectoryError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'sftp') return '';
                return !store.sftpRemoteDirectory ? 'Remote directory is required' : '';
            });

            const fileSystemPathError = computed(() => {
                if (!validationTriggered.value || selectedSource.value !== 'filesystem') return '';
                return !store.fileSystemPath ? 'Path is required' : '';
            });

            const mappingError = computed(() => {
                // No validation needed for boolean toggle
                return '';
            });

            // MODIFIED: Skip schedule validation when manualLoad is true
            const scheduleError = computed(() => {
                if (!validationTriggered.value) return '';

                // Skip schedule validation if Manual Load is enabled
                if (manualLoad.value) return '';

                if (!store.scheduleTime) return 'Schedule is required';

                if (store.scheduleType === 'one-time' && !store.scheduleDate) {
                    return 'Date is required for one-time schedule';
                }

                if (store.scheduleType === 'recurring' && store.recurrence === 'weekly' && store.selectedDaysOfWeek.length === 0) {
                    return 'At least one day must be selected for weekly recurrence';
                }

                return '';
            });

            // MODIFIED: Skip schedule validation when manualLoad is true
            const isFormValid = computed(() => {
                // Check source and file type
                if (!selectedSource.value || !store.selectedFileType) return false;

                // Check source-specific fields
                if (selectedSource.value === 'sftp') {
                    if (!store.sftpHost || !store.sftpPort || !store.sftpUsername ||
                        !store.sftpPassword || !store.sftpRemoteDirectory) {
                        return false;
                    }
                    if (store.sftpPort < 1 || store.sftpPort > 65535) return false;
                } else if (selectedSource.value === 'filesystem') {
                    if (!store.fileSystemPath) return false;
                }

                // No validation needed for useMappings boolean

                // Check schedule ONLY if Manual Load is disabled
                if (!manualLoad.value) {
                    if (!store.scheduleTime) return false;

                    if (store.scheduleType === 'one-time' && !store.scheduleDate) {
                        return false;
                    }

                    if (store.scheduleType === 'recurring' && store.recurrence === 'weekly' &&
                        store.selectedDaysOfWeek.length === 0) {
                        return false;
                    }
                }

                return true;
            });

            const confirmSchedule = () => {
                store.setSchedule({
                    scheduleType: tempScheduleType.value,
                    date: tempScheduleType.value === 'one-time' ? tempScheduleDate.value : null,
                    time: tempScheduleTime.value,
                    recurrence: tempScheduleType.value === 'recurring' ? tempRecurrence.value : 'daily',
                    selectedDaysOfWeek: tempScheduleType.value === 'recurring' && tempRecurrence.value === 'weekly'
                        ? tempSelectedDaysOfWeek.value
                        : []
                });
            };

            const cancelSchedule = () => {
                store.scheduleMenuVisible = false;
            };

            const clearSchedule = () => {
                store.clearSchedule();
            };

            // MODIFIED: Include manualLoad in save
            const save = async () => {
                validationTriggered.value = true;

                if (!isFormValid.value) {
                    console.warn('Form validation failed');
                    toast('Please fix validation errors before saving', 'error');
                    return;
                }

                try {
                    await store.save({
                        selectedSources: [selectedSource.value],
                        selectedFileType: store.selectedFileType,
                        sftpHost: store.sftpHost,
                        sftpPort: store.sftpPort,
                        sftpUsername: store.sftpUsername,
                        sftpPassword: store.sftpPassword,
                        sftpRemoteDirectory: store.sftpRemoteDirectory,
                        fileSystemPath: store.fileSystemPath,
                        useMappings: store.useMappings,
                        manualLoad: manualLoad.value, // NEW: Include manualLoad
                        scheduleType: store.scheduleType,
                        scheduleDate: store.scheduleDate,
                        scheduleTime: store.scheduleTime,
                        recurrence: store.recurrence,
                        selectedDaysOfWeek: store.selectedDaysOfWeek
                    });
                    validationTriggered.value = false;
                    toast('Data ingestion configuration saved successfully', 'success');
                } catch (error) {
                    console.error('Failed to save:', error);
                    toast('Failed to save configuration. Please try again.', 'error');
                }
            };

            const run = async () => {
                validationTriggered.value = true;
                if (!isFormValid.value) {
                    toast('Please fix validation errors before running', 'error');
                    return;
                }

                try {
                    await store.save({
                        selectedSources: [selectedSource.value],
                        selectedFileType: store.selectedFileType,
                        sftpHost: store.sftpHost,
                        sftpPort: store.sftpPort,
                        sftpUsername: store.sftpUsername,
                        sftpPassword: store.sftpPassword,
                        sftpRemoteDirectory: store.sftpRemoteDirectory,
                        fileSystemPath: store.fileSystemPath,
                        useMappings: store.useMappings,
                        manualLoad: manualLoad.value,
                        scheduleType: store.scheduleType,
                        scheduleDate: store.scheduleDate,
                        scheduleTime: store.scheduleTime,
                        recurrence: store.recurrence,
                        selectedDaysOfWeek: store.selectedDaysOfWeek
                    });
                } catch (error) {
                    toast('Failed to save configuration before running', 'error');
                    return;
                }

                ingestionRunning.value = true;
                toast('Data ingestion started — running in the background', 'info');

                api.post('/api/data-ingestion/run')
                    .then((response) => {
                        const result = response.data;
                        if (result.success) {
                            toast(`Ingestion complete — ${result.filesProcessed} files, ${result.recordsInserted} records in ${result.durationSeconds.toFixed(2)}s`, 'success');
                        } else {
                            const errorMsg = result.errors?.length
                                ? result.errors.join(', ')
                                : result.message || 'Data ingestion failed';
                            toast(`Ingestion finished with errors: ${errorMsg}`, 'error');
                        }
                    })
                    .catch((error: any) => {
                        console.error('Data ingestion failed:', error);
                        toast(`Data ingestion failed: ${error.message}`, 'error');
                    })
                    .finally(() => {
                        ingestionRunning.value = false;
                    });
            };

            // Initialize component
            onMounted(async () => {
                try {
                    // Load previous settings from backend
                    await store.load();

                    // Set selectedSource from loaded data
                    if (store.selectedSources.length > 0) {
                        selectedSource.value = store.selectedSources[0];
                    } else if (store.dataSources.length > 0) {
                        // Fallback to first available data source if nothing was saved
                        selectedSource.value = store.dataSources[0].value;
                    }

                    manualLoad.value = store.manualLoad ?? true;
                } catch (error) {
                    console.error('Failed to load data ingestion configuration:', error);
                    toast('Failed to load configuration', 'warning');
                }
            });

            // Return a single validation function (ValidationRule) instead of an array
            const required = (label: string): ((v: any) => string | boolean) => {
                return (v: any) => !!v || `${label} is required.`;
            };

            const showSftpPassword = ref(false);
            const isSaving = computed(() => (store as any).isSaving ?? false);
            const isRunning = computed(() => (store as any).isRunning ?? false);
            const ingestionRunning = ref(false);

            return {
                store,
                selectedSource,
                manualLoad, // NEW: Expose manualLoad
                sourceOptions,
                fileTypeOptions,
                hasSchedule,
                tempScheduleType,
                tempScheduleDate,
                tempScheduleTime,
                tempRecurrence,
                tempSelectedDaysOfWeek,
                isScheduleValid,
                confirmSchedule,
                cancelSchedule,
                clearSchedule,
                save,
                run,
                required,
                showSftpPassword,
                isSaving,
                isRunning,
                ingestionRunning,
                isFormValid,
                sourceError,
                fileTypeError,
                sftpHostError,
                sftpPortError,
                sftpUsernameError,
                sftpPasswordError,
                sftpRemoteDirectoryError,
                fileSystemPathError,
                scheduleError,
                mappingError
            };
        },
    };
</script>

<style scoped>
    .data-ingestion-container {
        padding: 20px;
    }

    .schedule-display {
        margin-top: 20px;
    }

    .action-buttons {
        display: flex;
        gap: 8px;
    }
    /* NEW: Optional visual styling for schedule when it's optional */
    .optional-section {
        opacity: 0.85;
        border-style: dashed !important;
    }
</style>