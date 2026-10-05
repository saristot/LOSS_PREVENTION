import { defineStore } from 'pinia';
import api from '../api/api';

export const useRoleStore = defineStore('roleStore', {
  state: () => ({
    roles: [] as Array<Record<string, any>>,
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchAllRoles() {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/roles/all');
        this.roles = Array.isArray(res.data) ? res.data : [];
      } catch (err: any) {
        this.error = err.message;
        this.roles = [];
      } finally {
        this.loading = false;
      }
    },

    async addRole(payload: { RoleName: string; Description: string; Permissions: any }) {
      this.loading = true;
      this.error = null;
      try {
        await api.post('/roles', payload);
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async getRoleById(roleId: string) {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/roles/by-id', {
          params: { RoleId: roleId }
        });
        return res.data;
      } catch (err: any) {
        this.error = err.message;
        return null;
      } finally {
        this.loading = false;
      }
    },

    async getRoleByName(roleName: string) {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/roles/by-name', {
          params: { RoleName: roleName }
        });
        return res.data;
      } catch (err: any) {
        this.error = err.message;
        return null;
      } finally {
        this.loading = false;
      }
    },

    async addUserToRole(payload: { UserId: string; RoleId: string }) {
      this.loading = true;
      this.error = null;
      try {
        await api.post('/roles/user', payload);
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async removeRoleFromUser(payload: { UserId: string; RoleId: string }) {
      this.loading = true;
      this.error = null;
      try {
        await api.delete('/roles/remove-role', { data: payload });
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async deleteRole(payload: { RoleId: string }) {
      this.loading = true;
      this.error = null;
      try {
        await api.delete('/roles/delete', { data: payload });
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async updateRole(payload: any) {
      this.loading = true;
      this.error = null;
      try {
        await api.put('/roles/update', payload);
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async userIsInRole(userId: string, roleName: string): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/roles/is-in-role', {
          params: { UserId: userId, RoleName: roleName }
        });
        return res.data === true;
      } catch (err: any) {
        this.error = err.message;
        return false;
      } finally {
        this.loading = false;
      }
    }
  }
});
