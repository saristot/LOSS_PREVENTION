import { defineStore } from 'pinia';
import api from '../api/api';
import type { RuleConfiguration } from '@/interfaces/ruletypes'; // adjust if needed

export const useRuleStore = defineStore('ruleStore', {
  state: () => ({
    rules: [] as RuleConfiguration[],
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchAllRules() {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/rules');
        this.rules = Array.isArray(res.data) ? res.data : [];
      } catch (err: any) {
        this.error = err.message;
        this.rules = [];
      } finally {
        this.loading = false;
      }
    },

    async getRuleById(id: string) {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get(`/rules/${id}`);
        return res.data as RuleConfiguration;
      } catch (err: any) {
        this.error = err.message;
        return null;
      } finally {
        this.loading = false;
      }
    },

    async addRule(payload: RuleConfiguration) {
      this.loading = true;
      this.error = null;
      try {
        await api.post('/rules', payload);
        await this.fetchAllRules();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async updateRule(payload: RuleConfiguration) {
      this.loading = true;
      this.error = null;
      try {
        await api.put('/rules', payload);
        await this.fetchAllRules();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async deleteRule(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await api.delete(`/rules/${id}`);
        await this.fetchAllRules();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async applyRules() {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/rules/apply');
        return res.data;
      } catch (err: any) {
        this.error = err.message;
        return null;
      } finally {
        this.loading = false;
      }
    }
  }
});
