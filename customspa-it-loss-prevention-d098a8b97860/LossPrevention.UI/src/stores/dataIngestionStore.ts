import { defineStore } from 'pinia';
import api from '@/api/api';

// API base for data ingestion endpoints. Backend will implement these routes.
const API_BASE = '/api/data-ingestion';

export const useDataIngestionStore = defineStore('data-ingestion', {
  state: () => ({
    dataSources: [
      { title: 'SFTP', value: 'sftp' },
      { title: 'File System', value: 'filesystem' }
    ] as { title: string; value: string }[],
    fileTypes: [
      { title: 'CSV', value: 'csv' },
      { title: 'XML', value: 'xml' },
      { title: 'JSON', value: 'json' }
    ] as { title: string; value: string }[],
    selectedSources: [] as string[],
    selectedFileType: '' as string,
    sftpHost: '',
    sftpPort: 22,
    sftpUsername: '',
    sftpPassword: '',
    sftpRemoteDirectory: '',
    fileSystemPath: '',
    scheduleType: 'one-time' as 'one-time' | 'recurring',
    scheduleDate: null as Date | null,
    scheduleTime: null as string | null,
    recurrenceOptions: [
      { title: 'Daily', value: 'daily' },
      { title: 'Weekly', value: 'weekly' }
    ] as { title: string; value: string }[],
    daysOfWeek: [
      { title: 'Monday', value: 'monday', short: 'Mon' },
      { title: 'Tuesday', value: 'tuesday', short: 'Tue' },
      { title: 'Wednesday', value: 'wednesday', short: 'Wed' },
      { title: 'Thursday', value: 'thursday', short: 'Thu' },
      { title: 'Friday', value: 'friday', short: 'Fri' },
      { title: 'Saturday', value: 'saturday', short: 'Sat' },
      { title: 'Sunday', value: 'sunday', short: 'Sun' }
    ] as { title: string; value: string; short: string }[],
    scheduleMenuVisible: false,
    recurrence: 'daily' as string,
    selectedDaysOfWeek: [] as string[],
    
    // Mapping configuration
    useMappings: false,

    // Manual load flag - when true, schedule is optional
    manualLoad: true,
  }),
  getters: {
    statusText: (state) => {
      if (state.scheduleType === 'one-time' && state.scheduleDate && state.scheduleTime) {
        return 'Scheduled';
      }
      if (state.scheduleType === 'recurring' && state.scheduleTime) {
        if (state.recurrence === 'daily') return 'Scheduled';
        if (state.recurrence === 'weekly' && state.selectedDaysOfWeek.length > 0) return 'Scheduled';
      }
      return 'Not Scheduled';
    },
    formattedSchedule: (state) => {
      if (!state.scheduleTime) return 'No schedule set';
      
      if (state.scheduleType === 'one-time') {
        if (!state.scheduleDate) return 'No schedule set';
        const date = new Date(state.scheduleDate);
        const dateStr = date.toLocaleDateString();
        return `One-time: ${dateStr} at ${state.scheduleTime}`;
      } else {
        // Recurring schedule
        if (state.recurrence === 'daily') {
          return `Daily at ${state.scheduleTime}`;
        } else if (state.recurrence === 'weekly' && state.selectedDaysOfWeek.length > 0) {
          const dayNames = state.selectedDaysOfWeek
            .map(day => state.daysOfWeek.find(d => d.value === day)?.short)
            .filter(Boolean)
            .join(', ');
          return `Weekly on ${dayNames} at ${state.scheduleTime}`;
        }
        return 'No schedule set';
      }
    },
    isScheduleValid: (state) => {
      if (!state.scheduleTime) return false;
      
      if (state.scheduleType === 'one-time') {
        return !!state.scheduleDate;
      } else {
        if (state.recurrence === 'daily') return true;
        if (state.recurrence === 'weekly') {
          return state.selectedDaysOfWeek.length > 0;
        }
      }
      return false;
    },
  },
  actions: {
    setSelectedSources(sources: string[]) {
      this.selectedSources = sources;
      // Persist selected sources to backend (non-blocking)
      api.patch(`${API_BASE}/sources`, { selectedSources: this.selectedSources })
        .catch((error: any) => console.error('Error updating selected sources:', error));
    },

    setScheduleDate(date: Date | null) {
      this.scheduleDate = date;
      // Update schedule date on backend (non-blocking)
      api.patch(`${API_BASE}/schedule`, { scheduleDate: this.scheduleDate, scheduleTime: this.scheduleTime, recurrence: this.recurrence })
        .catch((error: any) => console.error('Error updating schedule date:', error));
    },

    setScheduleTime(time: string | null) {
      this.scheduleTime = time;
      // Update schedule time on backend (non-blocking)
      api.patch(`${API_BASE}/schedule`, { scheduleDate: this.scheduleDate, scheduleTime: this.scheduleTime, recurrence: this.recurrence })
        .catch((error: any) => console.error('Error updating schedule time:', error));
    },

    setRecurrenceOptions(options: { title: string; value: string }[]) {
      this.recurrenceOptions = options;
      // Persist recurrence options to backend (non-blocking)
      api.put(`${API_BASE}/recurrence-options`, { recurrenceOptions: this.recurrenceOptions })
        .catch((error: any) => console.error('Error saving recurrence options:', error));
    },

    toggleScheduleMenu() {
      this.scheduleMenuVisible = !this.scheduleMenuVisible;
    },

    setSchedule(schedule: any) {
      this.scheduleType = schedule.scheduleType;
      // Only set date for one-time schedules
      this.scheduleDate = schedule.scheduleType === 'one-time' ? schedule.date : null;
      this.scheduleTime = schedule.time;
      // Only set recurrence for recurring schedules
      this.recurrence = schedule.scheduleType === 'recurring' ? schedule.recurrence : 'daily';
      this.selectedDaysOfWeek = schedule.scheduleType === 'recurring' && schedule.recurrence === 'weekly' 
        ? (schedule.selectedDaysOfWeek || []) 
        : [];
      this.scheduleMenuVisible = false;
      // Save full schedule to backend (non-blocking)
      api.patch(`${API_BASE}/schedule`, {
        scheduleType: this.scheduleType,
        scheduleDate: this.scheduleDate,
        scheduleTime: this.scheduleTime,
        recurrence: this.recurrence,
        selectedDaysOfWeek: this.selectedDaysOfWeek,
      }).catch((error: any) => console.error('Error saving schedule:', error));
    },

    clearSchedule() {
      this.scheduleType = 'one-time';
      this.scheduleDate = null;
      this.scheduleTime = null;
      this.recurrence = 'daily';
      this.selectedDaysOfWeek = [];
      // Clear schedule on backend (non-blocking)
      api.delete(`${API_BASE}/schedule`)
        .catch((error: any) => console.error('Error clearing schedule:', error));
    },

    async save(payload: any) {
      // Validate payload before saving
      const errors: string[] = [];
      
      if (!payload.selectedSources || payload.selectedSources.length === 0) {
        errors.push('At least one source must be selected');
      }
      
      if (!payload.selectedFileType) {
        errors.push('File type is required');
      }
      
      const source = payload.selectedSources?.[0];
      if (source === 'sftp') {
        if (!payload.sftpHost) errors.push('SFTP host is required');
        if (!payload.sftpPort) errors.push('SFTP port is required');
        if (payload.sftpPort < 1 || payload.sftpPort > 65535) errors.push('SFTP port must be between 1 and 65535');
        if (!payload.sftpUsername) errors.push('SFTP username is required');
        if (!payload.sftpPassword) errors.push('SFTP password is required');
        if (!payload.sftpRemoteDirectory) errors.push('SFTP remote directory is required');
      } else if (source === 'filesystem') {
        if (!payload.fileSystemPath) errors.push('File system path is required');
      }
      
      if (!payload.manualLoad) {
        if (!payload.scheduleTime) {
          errors.push('Schedule time is required');
        }

        if (payload.scheduleType === 'one-time' && !payload.scheduleDate) {
          errors.push('Schedule date is required for one-time schedule');
        }

        if (payload.scheduleType === 'recurring' && payload.recurrence === 'weekly' &&
          (!payload.selectedDaysOfWeek || payload.selectedDaysOfWeek.length === 0)) {
          errors.push('At least one day must be selected for weekly recurrence');
        }
      }
      
      if (errors.length > 0) {
        console.error('Validation errors:', errors);
        throw new Error(`Validation failed: ${errors.join(', ')}`);
      }

      try {
        // Save the entire data ingestion configuration including useMappings
        const { data } = await api.put(`${API_BASE}`, payload);
        this.selectedSources = data.selectedSources || [];
        this.selectedFileType = data.selectedFileType || '';
        this.sftpHost = data.sftpHost || '';
        this.sftpPort = data.sftpPort || 22;
        this.sftpUsername = data.sftpUsername || '';
        this.sftpRemoteDirectory = data.sftpRemoteDirectory || '';
        this.fileSystemPath = data.fileSystemPath || '';
        this.scheduleType = data.scheduleType || 'one-time';
        this.scheduleDate = data.scheduleDate;
        this.scheduleTime = data.scheduleTime;
        this.recurrence = data.recurrence || 'daily';
        this.selectedDaysOfWeek = data.selectedDaysOfWeek || [];
        this.useMappings = data.useMappings ?? false;
      } catch (error) {
        console.error('Error saving data ingestion:', error);
        throw error;
      }
    },

    async load() {
      try {
        // Load the data ingestion configuration from backend
        const { data } = await api.get(`${API_BASE}`);
        this.selectedSources = data.selectedSources || [];
        this.selectedFileType = data.selectedFileType || '';
        this.sftpHost = data.sftpHost || '';
        this.sftpPort = data.sftpPort || 22;
        this.sftpUsername = data.sftpUsername || '';
        this.sftpPassword = ''; // Don't load password from backend for security
        this.sftpRemoteDirectory = data.sftpRemoteDirectory || '';
        this.fileSystemPath = data.fileSystemPath || '';
        this.scheduleType = data.scheduleType || 'one-time';
        this.scheduleDate = data.scheduleDate;
        this.scheduleTime = data.scheduleTime;
        this.recurrence = data.recurrence || 'daily';
        this.selectedDaysOfWeek = data.selectedDaysOfWeek || [];
        this.useMappings = data.useMappings ?? false;
        this.manualLoad = data.manualLoad ?? true;
      } catch (error) {
        console.error('Error loading data ingestion:', error);
        throw error;
      }
    },

    async clear() {
      this.selectedSources = [];
      this.selectedFileType = '';
      this.sftpHost = '';
      this.sftpPort = 22;
      this.sftpUsername = '';
      this.sftpPassword = '';
      this.sftpRemoteDirectory = '';
      this.fileSystemPath = '';
      this.scheduleType = 'one-time';
      this.scheduleDate = null;
      this.scheduleTime = null;
      this.recurrence = 'daily';
      this.selectedDaysOfWeek = [];
      this.useMappings = false;
      // Also clear on the backend (non-blocking)
      api.delete(`${API_BASE}`)
        .catch((error: any) => console.error('Error clearing data ingestion on backend:', error));
    },
  },
});
