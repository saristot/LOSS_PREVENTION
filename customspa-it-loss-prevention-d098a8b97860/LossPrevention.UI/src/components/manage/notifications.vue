<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center gap-3">
        Notifications
        <v-spacer></v-spacer>

        <!-- Search -->
        <v-text-field
          v-model="search"
          placeholder="Search notifications..."
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          style="max-width: 260px"
          class="mr-2"
        />

        <v-btn color="success" class="mr-2" @click="openSendDialog" :disabled="!notificationStore.canManageNotifications">
          Send Notification
        </v-btn>
        <v-btn color="primary" @click="loadNotifications">Refresh</v-btn>
      </v-card-title>

      <v-card-text>
        <v-data-table
          class="bold-headers"
          :headers="headers"
          :items="filteredNotifications"
          :loading="notificationStore.loading"
          item-value="id"
        >
          <template #item.isRead="{ item }">
            <v-chip 
              size="small" 
              :color="item.isRead ? 'grey' : 'primary'"
            >
              {{ item.isRead ? 'Read' : 'Unread' }}
            </v-chip>
          </template>

          <template #item.type="{ item }">
            <v-chip size="small" :color="getTypeColor(item.type)">
              {{ item.type || 'message' }}
            </v-chip>
          </template>

          <template #item.createdAt="{ item }">
            {{ formatDate(item.createdAt) }}
          </template>

          <template #item.actions="{ item }">
            <v-btn icon size="small" variant="text" class="mr-1" @click="openNotification(item)">
              <v-icon size="small">mdi-eye</v-icon>
            </v-btn>
            <v-btn 
              v-if="!item.isRead" 
              icon 
              size="small" 
              variant="text"
              class="mr-1" 
              @click="markAsRead(item.id)"
              :disabled="!notificationStore.canManageNotifications"
            >
              <v-icon size="small">mdi-check</v-icon>
            </v-btn>
            <v-btn 
              icon 
              size="small" 
              variant="text"
              class="mr-1" 
              color="primary" 
              @click="openReplyDialog(item)"
              :disabled="!notificationStore.canManageNotifications"
            >
              <v-icon size="small">mdi-reply</v-icon>
            </v-btn>
            <v-btn 
              icon 
              size="small" 
              variant="text"
              color="red" 
              @click="confirmDelete(item)"
              :disabled="!notificationStore.canManageNotifications"
            >
              <v-icon size="small">mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- View Notification Dialog -->
    <v-dialog v-model="viewDialog" max-width="600px">
      <v-card v-if="selectedNotification">
        <v-card-title class="d-flex align-center">
          <v-icon class="mr-2" :color="getTypeColor(selectedNotification.type)">
            {{ getTypeIcon(selectedNotification.type) }}
          </v-icon>
          {{ selectedNotification.title || 'Notification' }}
          <v-spacer></v-spacer>
          <v-chip 
            size="small" 
            :color="selectedNotification.isRead ? 'grey' : 'primary'"
          >
            {{ selectedNotification.isRead ? 'Read' : 'Unread' }}
          </v-chip>
        </v-card-title>
        
        <v-card-text>
          <div class="mb-4">
            <div class="text-caption text-grey mb-2">Message</div>
            <div class="text-body-1">{{ selectedNotification.message }}</div>
          </div>

          <v-divider class="my-4" />

          <div class="d-flex flex-wrap gap-4">
            <div v-if="selectedNotification.from">
              <div class="text-caption text-grey">From</div>
              <div class="text-body-2">{{ selectedNotification.fromName || selectedNotification.from }}</div>
            </div>

            <div v-if="selectedNotification.to">
              <div class="text-caption text-grey">To</div>
              <div class="text-body-2">{{ selectedNotification.to }}</div>
            </div>

            <div>
              <div class="text-caption text-grey">Type</div>
              <div class="text-body-2">{{ selectedNotification.toType || 'user' }}</div>
            </div>

            <div>
              <div class="text-caption text-grey">Date</div>
              <div class="text-body-2">{{ formatDate(selectedNotification.createdAt) }}</div>
            </div>
          </div>
        </v-card-text>

        <v-card-actions>
          <v-btn 
            v-if="!selectedNotification.isRead" 
            color="primary" 
            variant="text" 
            @click="markAsReadAndClose"
            :disabled="!notificationStore.canManageNotifications"
          >
            Mark as Read
          </v-btn>
          <v-btn 
            color="primary" 
            variant="text" 
            @click="openReplyFromView"
            :disabled="!notificationStore.canManageNotifications"
          >
            Reply
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="viewDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Send Notification Dialog -->
    <v-dialog v-model="sendDialog" max-width="700px">
      <v-card>
        <v-card-title>Send Notification</v-card-title>
        <v-card-text>
          <v-form ref="sendForm" v-model="sendFormValid">
            <v-select
              v-model="newNotification.type"
              :items="notificationTypes"
              label="Type"
              :rules="[required]"
            />

            <v-text-field 
              v-model="newNotification.title" 
              label="Title" 
              :rules="[required]" 
            />
            
            <v-textarea 
              v-model="newNotification.message" 
              label="Message" 
              rows="4"
              :rules="[required]"
            />

            <v-divider class="my-4" />

            <v-select
              v-model="newNotification.toType"
              :items="recipientTypes"
              label="Send To"
              :rules="[required]"
            />

            <v-autocomplete
              v-if="newNotification.toType === 'user'"
              v-model="newNotification.to"
              :items="availableUsers"
              item-title="displayName"
              item-value="id"
              multiple
              chips
              closable-chips
              label="Select Users"
              :rules="[v => v?.length > 0 || 'At least one recipient is required']"
            >
              <template #chip="{ item, props }">
                <v-chip v-bind="props" color="primary" size="small">
                  {{ item.title }}
                </v-chip>
              </template>
            </v-autocomplete>

            <v-autocomplete
              v-if="newNotification.toType === 'group'"
              v-model="newNotification.to"
              :items="availableGroups"
              item-title="name"
              item-value="id"
              multiple
              chips
              closable-chips
              label="Select Groups"
              :rules="[v => v?.length > 0 || 'At least one group is required']"
            >
              <template #chip="{ item, props }">
                <v-chip v-bind="props" color="primary" size="small">
                  {{ item.title }}
                </v-chip>
              </template>
            </v-autocomplete>

            <v-autocomplete
              v-if="newNotification.toType === 'role'"
              v-model="newNotification.to"
              :items="availableRoles"
              item-title="name"
              item-value="id"
              multiple
              chips
              closable-chips
              label="Select Roles"
              :rules="[v => v?.length > 0 || 'At least one role is required']"
            >
              <template #chip="{ item, props }">
                <v-chip v-bind="props" color="primary" size="small">
                  {{ item.title }}
                </v-chip>
              </template>
            </v-autocomplete>
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeSendDialog">Cancel</v-btn>
          <v-btn color="primary" @click="sendNotification" :disabled="!sendFormValid">Send</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Reply Dialog -->
    <v-dialog v-model="replyDialog" max-width="600px">
      <v-card>
        <v-card-title>Reply to Notification</v-card-title>
        <v-card-text>
          <div v-if="replyToNotification" class="mb-4 pa-3" style="background-color: #f5f5f5; border-radius: 4px;">
            <div class="text-caption text-grey">Original Message</div>
            <div class="text-body-2">{{ replyToNotification.message }}</div>
          </div>

          <v-form ref="replyForm" v-model="replyFormValid">
            <v-textarea 
              v-model="replyMessage" 
              label="Your Reply" 
              rows="4"
              :rules="[required]"
            />
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeReplyDialog">Cancel</v-btn>
          <v-btn color="primary" @click="sendReply" :disabled="!replyFormValid">Send Reply</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400px">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this notification?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteNotification">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useNotificationStore } from '@/stores/notificationStore'
import { useGroupStore } from '@/stores/groupStore'
import api from '@/api/api'

const notificationStore = useNotificationStore()
const groupStore = useGroupStore()

const search = ref('')
const viewDialog = ref(false)
const sendDialog = ref(false)
const replyDialog = ref(false)
const deleteDialog = ref(false)
const sendFormValid = ref(false)
const replyFormValid = ref(false)
const sendForm = ref(null)
const replyForm = ref(null)

const selectedNotification = ref<any>(null)
const notificationToDelete = ref<any>(null)
const replyToNotification = ref<any>(null)
const replyMessage = ref('')

const users = ref<any[]>([])
const roles = ref<any[]>([])

const newNotification = ref({
  type: 'message' as 'message' | 'alert',
  title: '',
  message: '',
  to: [] as string[],
  toType: 'user' as 'user' | 'role' | 'group',
  from: localStorage.getItem('username') || '',
  fromName: localStorage.getItem('username') || ''
})

const defaultNotification = {
  type: 'message' as 'message' | 'alert',
  title: '',
  message: '',
  to: [] as string[],
  toType: 'user' as 'user' | 'role' | 'group',
  from: localStorage.getItem('username') || '',
  fromName: localStorage.getItem('username') || ''
}

const notificationTypes = [
  { title: 'Message', value: 'message' },
  { title: 'Alert', value: 'alert' }
]

const recipientTypes = [
  { title: 'Users', value: 'user' },
  { title: 'Groups', value: 'group' },
  { title: 'Roles', value: 'role' }
]

const headers = [
  { title: 'Status', key: 'isRead', sortable: true },
  { title: 'Type', key: 'type', sortable: true },
  { title: 'Title', key: 'title', sortable: true },
  { title: 'Message', key: 'message', sortable: false },
  { title: 'From', key: 'fromName', sortable: true },
  { title: 'Date', key: 'createdAt', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const }
]

const filteredNotifications = computed(() => {
  if (!search.value) return notificationStore.notifications
  const s = search.value.toLowerCase()
  return notificationStore.notifications.filter(n => 
    n.title?.toLowerCase().includes(s) ||
    n.message?.toLowerCase().includes(s) ||
    n.fromName?.toLowerCase().includes(s) ||
    n.from?.toLowerCase().includes(s)
  )
})

const availableUsers = computed(() => {
  return users.value.map(u => ({
    id: u.id || u._id,
    displayName: `${u.firstName} ${u.lastName} (${u.email})`
  }))
})

const availableGroups = computed(() => {
  return groupStore.groups
})

const availableRoles = computed(() => {
  return roles.value.map(r => ({
    id: r.id || r._id,
    name: r.name || r.roleName || r.RoleName
  }))
})

const required = (v: any) => !!v || 'This field is required'

const formatDate = (date: string) => {
  if (!date) return 'N/A'
  return new Date(date).toLocaleString()
}

const getTypeColor = (type?: string) => {
  switch (type) {
    case 'alert': return 'error'
    case 'reply': return 'info'
    default: return 'primary'
  }
}

const getTypeIcon = (type?: string) => {
  switch (type) {
    case 'alert': return 'mdi-alert'
    case 'reply': return 'mdi-reply'
    default: return 'mdi-email'
  }
}

onMounted(async () => {
  await loadNotifications()
  await loadUsers()
  await loadRoles()
  await groupStore.fetchGroups()
})

async function loadNotifications() {
  try {
    await notificationStore.fetchNotifications()
  } catch (err: any) {
    notificationStore.showNotification('Failed to load notifications: ' + err.message, 'error')
  }
}

async function loadUsers() {
  try {
    const { data } = await api.get('/users')
    users.value = data
  } catch (err: any) {
    notificationStore.showNotification('Failed to load users: ' + err.message, 'error')
  }
}

async function loadRoles() {
  try {
    const { data } = await api.get('/roles/all')
    roles.value = data
  } catch (err: any) {
    notificationStore.showNotification('Failed to load roles: ' + err.message, 'error')
  }
}

function openNotification(notification: any) {
  selectedNotification.value = notification
  viewDialog.value = true
}

function openSendDialog() {
  newNotification.value = { ...defaultNotification }
  sendDialog.value = true
}

function closeSendDialog() {
  sendDialog.value = false
  newNotification.value = { ...defaultNotification }
}

async function sendNotification() {
  if (!sendFormValid.value) return

  try {
    await notificationStore.sendNotification(newNotification.value)
    closeSendDialog()
  } catch (err: any) {
    // Error is already handled by the store
  }
}

function openReplyDialog(notification: any) {
  replyToNotification.value = notification
  replyMessage.value = ''
  replyDialog.value = true
}

function openReplyFromView() {
  const notif = selectedNotification.value
  viewDialog.value = false
  openReplyDialog(notif)
}

function closeReplyDialog() {
  replyDialog.value = false
  replyToNotification.value = null
  replyMessage.value = ''
}

async function sendReply() {
  if (!replyFormValid.value || !replyToNotification.value) return

  try {
    await notificationStore.replyToNotification(replyToNotification.value.id, replyMessage.value)
    closeReplyDialog()
  } catch (err: any) {
    // Error is already handled by the store
  }
}

async function markAsRead(notificationId: string) {
  try {
    await notificationStore.markAsRead(notificationId)
  } catch (err: any) {
    notificationStore.showNotification('Failed to mark as read: ' + err.message, 'error')
  }
}

async function markAsReadAndClose() {
  if (selectedNotification.value) {
    await markAsRead(selectedNotification.value.id)
    viewDialog.value = false
  }
}

function confirmDelete(notification: any) {
  notificationToDelete.value = notification
  deleteDialog.value = true
}

async function deleteNotification() {
  if (!notificationToDelete.value) return

  try {
    await notificationStore.deleteNotification(notificationToDelete.value.id)
    deleteDialog.value = false
    notificationToDelete.value = null
    if (viewDialog.value) {
      viewDialog.value = false
    }
  } catch (err: any) {
    // Error is already handled by the store
  }
}
</script>

<style scoped>
.bold-headers :deep(th) {
  font-weight: bold !important;
}
</style>
