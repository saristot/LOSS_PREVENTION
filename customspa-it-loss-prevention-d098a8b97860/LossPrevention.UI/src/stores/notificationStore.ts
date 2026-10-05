import { defineStore } from 'pinia'
import api from '@/api/api'
import { useGroupStore } from './groupStore'

interface Notification {
  id: string
  type?: 'message' | 'alert' | 'reply'
  title?: string
  message: string
  from?: string
  fromName?: string
  to?: string | string[]
  toType?: 'user' | 'role' | 'group'
  replyTo?: string
  isRead: boolean
  createdAt: string
}

interface NotificationState {
  notifications: Notification[]
  loading: boolean
  error: string | null
  snackbar: {
    show: boolean
    message: string
    color: string
  }
}

export const useNotificationStore = defineStore('notification', {
  state: (): NotificationState => ({
    notifications: [],
    loading: false,
    error: null,
    snackbar: {
      show: false,
      message: '',
      color: 'success'
    }
  }),

  getters: {
    unreadCount: (state) => state.notifications.filter(n => !n.isRead).length,
    latestNotifications: (state) => state.notifications.slice(0, 5),
    unreadNotifications: (state) => state.notifications.filter(n => !n.isRead),
    
    canManageNotifications: (): boolean => {
      try {
        const token = localStorage.getItem('token')
        if (!token) return false
        
        const payload = JSON.parse(atob(token.split('.')[1]))
        const permissions = payload.permissions || payload.Permission || payload.Permissions || payload.scope || payload.scopes || []
        
        if (Array.isArray(permissions)) {
          return permissions.includes('CAN_MANAGE_NOTIFICATIONS')
        } else if (typeof permissions === 'string') {
          return permissions.split(' ').includes('CAN_MANAGE_NOTIFICATIONS')
        }
        
        return false
      } catch (error) {
        console.error('Error checking notification permissions:', error)
        return false
      }
    }
  },

  actions: {
    async fetchNotifications() {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to view notifications'
        console.warn('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
        return
      }

      this.loading = true
      this.error = null
      try {
        const response = await api.get('/notifications')
        // Map _id to id and normalize property names for consistency
        this.notifications = (response.data || []).map((n: any) => ({
          ...n,
          id: n.id || n._id,
          isRead: n.isRead ?? n.IsRead ?? false
        })).sort((a: Notification, b: Notification) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        )
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch notifications'
        console.error('Failed to fetch notifications:', error)
      } finally {
        this.loading = false
      }
    },

    async sendNotification(notification: {
      type?: 'message' | 'alert'
      title?: string
      message: string
      to: string | string[]
      toType: 'user' | 'role' | 'group'
      from?: string
      fromName?: string
    }) {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to send notifications'
        this.showNotification('Permission denied: Cannot send notifications', 'error')
        throw new Error('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
      }

      this.loading = true
      this.error = null
      try {
        // If sending to groups, expand group members
        if (notification.toType === 'group') {
          const groupStore = useGroupStore()
          const groupIds = Array.isArray(notification.to) ? notification.to : [notification.to]
          const memberIds = groupStore.getAllGroupMembers(groupIds)
          
          console.log('Sending notification to group members:', memberIds)
          
          // Send individual notifications to each member
          for (const userId of memberIds) {
            await api.post('/notifications', {
              ...notification,
              to: userId,
              toType: 'user'
            })
          }
          
          this.showNotification(`Notification sent to ${memberIds.length} group member(s)`, 'success')
        } else {
          // Send directly to users or roles
          console.log('Sending notification:', notification)
          const response = await api.post('/notifications', notification)
          console.log('Notification response:', response)
          this.showNotification('Notification sent successfully', 'success')
        }
        
        await this.fetchNotifications()
      } catch (error: any) {
        console.error('Error sending notification:', error)
        console.error('Error response:', error.response)
        this.error = error.message || 'Failed to send notification'
        this.showNotification('Failed to send notification', 'error')
        throw error
      } finally {
        this.loading = false
      }
    },

    async markAsRead(notificationId: string) {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to mark notifications as read'
        console.warn('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
        return
      }

      try {
        await api.put(`/notifications/${notificationId}/read`, {})
        const notification = this.notifications.find(n => n.id === notificationId)
        if (notification) {
          notification.isRead = true
        }
      } catch (error: any) {
        this.error = error.message || 'Failed to mark notification as read'
        console.error('Failed to mark notification as read:', error)
      }
    },

    getNotificationById(notificationId: string): Notification | undefined {
      return this.notifications.find(n => n.id === notificationId)
    },

    async deleteNotification(notificationId: string) {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to delete notifications'
        this.showNotification('Permission denied: Cannot delete notifications', 'error')
        throw new Error('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
      }

      this.loading = true
      this.error = null
      try {
        await api.delete(`/notifications/${notificationId}`)
        this.notifications = this.notifications.filter(n => n.id !== notificationId)
        this.showNotification('Notification deleted', 'success')
      } catch (error: any) {
        this.error = error.message || 'Failed to delete notification'
        this.showNotification('Failed to delete notification', 'error')
        throw error
      } finally {
        this.loading = false
      }
    },

    async replyToNotification(notificationId: string, message: string) {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to reply to notifications'
        this.showNotification('Permission denied: Cannot reply to notifications', 'error')
        throw new Error('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
      }

      this.loading = true
      this.error = null
      try {
        const originalNotif = this.notifications.find(n => n.id === notificationId)
        if (!originalNotif) throw new Error('Original notification not found')

        await api.post(`/notifications/${notificationId}/reply`, {
          message,
          from: localStorage.getItem('username') || 'current-user'
        })
        
        this.showNotification('Reply sent successfully', 'success')
        await this.fetchNotifications()
      } catch (error: any) {
        this.error = error.message || 'Failed to send reply'
        this.showNotification('Failed to send reply', 'error')
        throw error
      } finally {
        this.loading = false
      }
    },

    async markAllAsRead() {
      if (!this.canManageNotifications) {
        this.error = 'You do not have permission to mark notifications as read'
        console.warn('Permission denied: CAN_MANAGE_NOTIFICATIONS required')
        return
      }

      try {
        const unread = this.notifications.filter(n => !n.isRead)
        for (const notif of unread) {
          await this.markAsRead(notif.id)
        }
        this.showNotification('All notifications marked as read', 'success')
      } catch (error: any) {
        this.error = error.message || 'Failed to mark all as read'
        console.error('Failed to mark all as read:', error)
      }
    },

    showNotification(message: string, color: 'success' | 'error' | 'warning' | 'info' = 'success') {
      this.snackbar = {
        show: true,
        message,
        color
      }
    },

    hideNotification() {
      this.snackbar.show = false
    }
  }
})

