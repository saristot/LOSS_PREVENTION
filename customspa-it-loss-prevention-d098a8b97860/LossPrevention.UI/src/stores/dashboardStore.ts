// stores/dashboardStore.ts
import { defineStore } from 'pinia'
import api from '@/api/api'

export const useDashboardStore = defineStore('dash', {
  state: () => ({
    current: null as null | { id?: string; name: string; blocks: any[]; published?: boolean; [k: string]: any },
    blocks: [] as any[],
    saving: false,
    isDirty: false,
    snapshot: '' as string,
    autosave: false
  }),
  getters: {
    statusText: (s) => (s.saving ? 'Saving…' : s.isDirty ? 'Unsaved' : 'Saved'),
  },
  actions: {
    setBlocks(blocks: any[]) {
      this.blocks = blocks ?? []
      if (!this.current) this.current = { name: '', blocks: [] }
      this.current.blocks = this.blocks
      this.isDirty = true
    },

    setMeta(meta: Partial<{ name: string; published: boolean; id?: string }>) {
      if (!this.current) this.current = { name: '', blocks: [] }
      this.current = { ...this.current, ...meta }
      this.isDirty = true
    },

    load(model: { id?: string; name: string; blocks: any[]; [k: string]: any }) {
      this.current = { ...model }
      this.blocks = Array.isArray(model.blocks) ? [...model.blocks] : []
      this.isDirty = false
      this.snapshot = JSON.stringify(this.current)
    },

    async get(id: string) {
      const { data } = await api.get(`/dashboards/${id}`)
      return data
    },

     async save(payload: { id?: string; name: string; blocks: any[]; workspaceId?: string; tabId?: string }) {
      this.saving = true
      try {
        console.log("save id " + payload.id);
        if (payload.id) {
          const { data } = await api.put(`/dashboards/${payload.id}`, payload)
          this.current = { ...data }
          this.blocks = Array.isArray(data.blocks) ? [...data.blocks] : []
        } else {
          const { data } = await api.post(`/dashboards`, payload)
          this.current = { ...data }
          this.blocks = Array.isArray(data.blocks) ? [...data.blocks] : []
        }
        this.isDirty = false
        this.snapshot = JSON.stringify(this.current)
      } finally {
        this.saving = false
      }
    },

    async list(workspaceId?: string, tabId?: string) {
      const params = new URLSearchParams()
      if (workspaceId) params.append('workspaceId', workspaceId)
      if (tabId) params.append('tabId', tabId)
      const { data } = await api.get(`/dashboards?${params.toString()}`)
      return data
    },

    async revert() {
      if (!this.snapshot) return
      const snap = JSON.parse(this.snapshot)
      this.current = { ...snap }
      this.blocks = Array.isArray(snap.blocks) ? [...snap.blocks] : []
      this.isDirty = false
    },

    async remove(id: string) {
      await api.delete(`/dashboards/${id}`)
    },

    clearCurrent() {
      this.current = { name: '', blocks: [] }
      this.blocks = []
      this.isDirty = false
      this.snapshot = ''
    },
  },
})
