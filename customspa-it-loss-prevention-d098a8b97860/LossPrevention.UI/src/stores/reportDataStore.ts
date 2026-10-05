import type { Distance } from '@/interfaces/distance'
import { defineStore } from 'pinia'
import api from '../api/api'

export const useReportDataStore = defineStore('reportData', {
  state: () => ({
    data: [] as Array<Record<string, any>>,
    distanceResults: [] as any[],
    total: 0,
    loading: false,
    error: null as string | null,
    duration: 0
  }),
  actions: {
    async fetchReportData(queryPipeline: any[], take = 50, skip = 0) {
      this.loading = true
      this.error = null
      this.duration = 0

      const start = performance.now()

      console.log(`📡 API Call: /data/report/query - Take: ${take}, Skip: ${skip}, Pipeline stages: ${queryPipeline.length}`);

      try {
        const res = await api.post('/data/report/query', {
          QueryPipeline: queryPipeline,
          Take: take,
          Skip: skip
        })

        const end = performance.now()

        this.data = res.data.data
        this.total = res.data.total
        this.duration = Math.round(end - start)
        
        console.log(`✅ API Response: ${this.data?.length || 0} records in ${this.duration}ms`);
      } catch (err: any) {
        console.error('❌ API Error:', err.response?.data || err.message);
        this.error = err.message
      } finally {
        this.loading = false
      }
    },

    async fetchDistanceAnalysis(payload: Distance) {
      // Filter out invalid fields (empty or whitespace-only strings)
      const validFields = payload.fields.filter(field => 
        field && field.trim() !== ''
      );

      // Validation
      if (validFields.length < 3 || !payload.startDateField || !payload.endDateField) {
        this.error = 'Distance analysis requires at least 3 fields and both date fields to be selected.';
        this.distanceResults = [];
        return;
      }

      if (!payload.id) {
        this.error = 'Source document ID is required for distance analysis.';
        this.distanceResults = [];
        return;
      }

      if (!payload.keyField) {
        this.error = 'Please select a field to use for legend labels.';
        this.distanceResults = [];
        return;
      }

      this.loading = true;
      this.error = null;
      this.duration = 0;
      this.distanceResults = [];
      
      const start = performance.now();

      try {
        // Use the filtered fields in the request
        const cleanedPayload = {
          ...payload,
          fields: validFields
        };
        
        const res = await api.post('/distance', cleanedPayload);
        
        if (!res.data || res.data.length === 0) {
          this.error = 'No similar records found in the specified date range. Try expanding the date range or selecting different fields.';
          this.distanceResults = [];
        } else {
          this.distanceResults = res.data;
        }
        
        this.duration = Math.round(performance.now() - start);
      } catch (err: any) {
        this.error = err.response?.data?.message || err.message || 'Failed to perform distance analysis.';
        this.distanceResults = [];
      } finally {
        this.loading = false;
      }
    }

  }
})
