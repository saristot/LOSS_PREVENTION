import { defineStore } from 'pinia';
import api from '../api/api';

export const usePermissionStore = defineStore('permissionStore', {
  state: () => ({
    permissions: [] as Array<Record<string, any>>,
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchAllPermissions() {
      this.loading = true;
      this.error = null;
      try {
        const res = await api.get('/permissions/all');
        this.permissions = Array.isArray(res.data) ? res.data : [];
      } catch (err: any) {
        this.error = err.message;
        this.permissions = [];
      } finally {
        this.loading = false;
      }
    },

    async createPermission(permission: { permissionName: string; permissionText: string; description: string }) {
      this.loading = true;
      this.error = null;
      try {
        await api.post('/permissions/create', permission);
        await this.fetchAllPermissions();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async deletePermission(permissionId: string) {
      this.loading = true;
      this.error = null;
      try {
        await api.delete('/permissions/delete', {
          data: { PermissionId: permissionId }
        });
        await this.fetchAllPermissions();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async updatePermission(permission: any) {
      this.loading = true;
      this.error = null;
      try {
        await api.put('/permissions/update', permission);
        await this.fetchAllPermissions();
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async getPermissionsForRole(roleId: string) {
      try {
        const res = await api.get('/permissions', { params: { RoleId: roleId } });
        return res.data;
      } catch (err: any) {
        this.error = err.message;
        return [];
      }
    },

    async addPermissionToRole(roleId: string, permissionId: string) {
      try {
        await api.post('/permissions/add-permission', {
          RoleId: roleId,
          PermissionId: permissionId
        });
      } catch (err: any) {
        this.error = err.message;
      }
    },

    async removePermissionFromRole(roleId: string, permissionId: string) {
      try {
        await api.delete('/permissions', {
          data: {
            RoleId: roleId,
            PermissionId: permissionId
          }
        });
      } catch (err: any) {
        this.error = err.message;
      }
    }
  }
});
