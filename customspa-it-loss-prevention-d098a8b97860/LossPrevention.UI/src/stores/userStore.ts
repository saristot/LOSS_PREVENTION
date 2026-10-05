// stores/userStore.ts
import { defineStore } from 'pinia'
import api from '../api/api'
import axios from 'axios'

export const useUserStore = defineStore('userStore', {
  state: () => ({
    users: [] as any[],
    selectedUser: null as any,
    loading: false,
    error: null as string | null
  }),

  actions: {
    async fetchUsers() {
      this.loading = true
      this.error = null
      try {
        const res = await api.get('/users')
        this.users = res.data
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Fetch users failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Fetch users failed'
        }
      } finally {
        this.loading = false
      }
    },

    async getUserById(id: string) {
      this.error = null
      try {
        const res = await api.get(`/users/id/${id}`)
        this.selectedUser = res.data
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Get user failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Get user failed'
        }
      }
    },

    async getUserByUsername(username: string) {
      this.error = null
      try {
        const res = await api.get(`/users/username/${username}`)
        this.selectedUser = res.data
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Get user by username failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Get user by username failed'
        }
      }
    },

    async createUser(userData: any) {
      this.error = null
      try {
        await api.post('/users/create', userData)
        await this.fetchUsers()
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Create user failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Create user failed'
        }
      }
    },

    async updateUser(userData: any) {
      this.error = null
      try {
        await api.post('/users/update', userData)
        await this.fetchUsers()
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Update user failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Update user failed'
        }
      }
    },

    async deleteUser(id: string) {
      this.error = null
      try {
        await api.delete(`/users/id/${id}`)
        await this.fetchUsers()
      } catch (err) {
        if (axios.isAxiosError(err)) {
          this.error = err.response?.data?.message || err.message || 'Delete user failed'
        } else if (err instanceof Error) {
          this.error = err.message
        } else {
          this.error = 'Delete user failed'
        }
      }
    }
  }
})
