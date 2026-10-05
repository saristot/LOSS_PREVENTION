import { defineStore } from 'pinia'
import api from '@/api/api'

interface Group {
  id: string
  name: string
  description: string
  members: string[]
  createdAt: string
  updatedAt: string
}

interface GroupState {
  groups: Group[]
  loading: boolean
  error: string | null
}

export const useGroupStore = defineStore('group', {
  state: (): GroupState => ({
    groups: [],
    loading: false,
    error: null
  }),

  getters: {
    getGroupById: (state) => (id: string) => {
      return state.groups.find(g => g.id === id)
    },
    
    getGroupsByUserId: (state) => (userId: string) => {
      return state.groups.filter(g => g.members.includes(userId))
    },

    getUsersInGroup: (state) => (groupId: string) => {
      const group = state.groups.find(g => g.id === groupId)
      return group?.members || []
    },

    getAllGroupMembers: (state) => (groupIds: string[]) => {
      const memberSet = new Set<string>()
      groupIds.forEach(groupId => {
        const group = state.groups.find(g => g.id === groupId)
        if (group) {
          group.members.forEach(member => memberSet.add(member))
        }
      })
      return Array.from(memberSet)
    }
  },

  actions: {
    async fetchGroups() {
      this.loading = true
      this.error = null
      try {
        console.log('Fetching groups from /groups...')
        const { data } = await api.get('/groups')
        console.log('Groups fetched successfully:', data)
        this.groups = data || []
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch groups'
        console.error('Failed to fetch groups:', error)
        console.error('Error response:', error.response)
      } finally {
        this.loading = false
      }
    },

    async createGroup(group: Omit<Group, 'id' | 'createdAt' | 'updatedAt'>) {
      this.loading = true
      this.error = null
      try {
        console.log('Creating group:', group)
        const { data } = await api.post('/groups', group)
        console.log('Group created successfully:', data)
        // Don't push here - let the component reload to avoid duplicates
        return data
      } catch (error: any) {
        this.error = error.message || 'Failed to create group'
        console.error('Failed to create group:', error)
        console.error('Error response:', error.response)
        throw error
      } finally {
        this.loading = false
      }
    },

    async updateGroup(id: string, group: Partial<Group>) {
      this.loading = true
      this.error = null
      try {
        const { data } = await api.put(`/groups/${id}`, group)
        const index = this.groups.findIndex(g => g.id === id)
        if (index !== -1) {
          this.groups[index] = data
        }
        return data
      } catch (error: any) {
        this.error = error.message || 'Failed to update group'
        console.error('Failed to update group:', error)
        throw error
      } finally {
        this.loading = false
      }
    },

    async deleteGroup(id: string) {
      this.loading = true
      this.error = null
      try {
        await api.delete(`/groups/${id}`)
        this.groups = this.groups.filter(g => g.id !== id)
      } catch (error: any) {
        this.error = error.message || 'Failed to delete group'
        console.error('Failed to delete group:', error)
        throw error
      } finally {
        this.loading = false
      }
    },

    async addMemberToGroup(groupId: string, userId: string) {
      const group = this.getGroupById(groupId)
      if (!group) {
        throw new Error('Group not found')
      }

      if (!group.members.includes(userId)) {
        const updatedMembers = [...group.members, userId]
        await this.updateGroup(groupId, { members: updatedMembers })
      }
    },

    async removeMemberFromGroup(groupId: string, userId: string) {
      const group = this.getGroupById(groupId)
      if (!group) {
        throw new Error('Group not found')
      }

      const updatedMembers = group.members.filter(m => m !== userId)
      await this.updateGroup(groupId, { members: updatedMembers })
    }
  }
})
