// stores/mappingStore.ts
import { defineStore } from 'pinia'
import api from '../api/api'

export const useMappingStore = defineStore('mappingStore', {
  state: () => ({
    mappings: [] as any[],
    loading: false as boolean,
    error: null as string | null,
  }),
  actions: {
    async fetchMappings() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/data/mappings')
        this.mappings = (response.data || []).sort((a: any, b: any) =>
          (a.alias || '').localeCompare(b.alias || '')
        )
      } catch (err: any) {
        this.error = err?.response?.data?.message || 'Failed to fetch mappings.'
      } finally {
        this.loading = false
      }
    },

    async createMapping(mapping: any) {
      this.loading = true
      this.error = null
      try {
        await api.post('/data/mappings', mapping)
        await this.fetchMappings()
      } catch (err: any) {
        this.error = err?.response?.data?.message || 'Failed to create mapping.'
      } finally {
        this.loading = false
      }
    },

    async updateMapping(id: string, mapping: any) {
      this.loading = true
      this.error = null
      try {
        await api.put(`/data/mappings/${id}`, mapping)
        await this.fetchMappings()
      } catch (err: any) {
        this.error = err?.response?.data?.message || 'Failed to update mapping.'
      } finally {
        this.loading = false
      }
    },

    async deleteMapping(id: string) {
      this.loading = true
      this.error = null
      try {
        await api.delete(`/data/mappings/${id}`)
        await this.fetchMappings()
      } catch (err: any) {
        this.error = err?.response?.data?.message || 'Failed to delete mapping.'
      } finally {
        this.loading = false
      }
    },

    clear() {
      this.mappings = []
      this.error = null
    }
  }
})
