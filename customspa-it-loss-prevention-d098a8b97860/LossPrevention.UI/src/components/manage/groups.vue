<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center gap-3">
        Groups
        <v-spacer></v-spacer>

        <!-- Search -->
        <v-text-field
          v-model="search"
          placeholder="Search groups..."
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          style="max-width: 260px"
          class="mr-2"
        />

        <v-btn color="success" class="mr-2" small @click="openGroupDialog">Add Group</v-btn>
        <v-btn color="primary" @click="loadGroups">Refresh</v-btn>
      </v-card-title>

      <v-card-text>
        <v-data-table
          class="bold-headers"
          :headers="headers"
          :items="filteredGroups"
          :loading="groupStore.loading"
          item-value="id"
        >
          <template #item.memberCount="{ item }">
            <v-chip size="small" color="primary">{{ item.members?.length || 0 }}</v-chip>
          </template>

          <template #item.createdAt="{ item }">
            {{ formatDate(item.createdAt) }}
          </template>

          <template #item.actions="{ item }">
            <v-icon small class="mr-2" @click="editGroup(item)">mdi-pencil</v-icon>
            <v-icon small color="red" @click="confirmDelete(item)">mdi-delete</v-icon>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Add/Edit Group Dialog -->
    <v-dialog v-model="dialog" max-width="700px">
      <v-card>
        <v-card-title>{{ editedGroup.id ? 'Edit' : 'Create' }} Group</v-card-title>
        <v-card-text>
          <v-form ref="groupForm" v-model="formValid">
            <v-text-field 
              v-model="editedGroup.name" 
              label="Group Name" 
              :rules="[required]" 
            />
            
            <v-textarea 
              v-model="editedGroup.description" 
              label="Description" 
              rows="3"
            />

            <v-divider class="my-4" />

            <div class="text-subtitle-2 mb-2">Group Members</div>
            
            <v-autocomplete
              v-model="editedGroup.members"
              :items="availableUsers"
              item-title="displayName"
              item-value="id"
              multiple
              chips
              closable-chips
              label="Add Users to Group"
              :rules="[v => v?.length > 0 || 'At least one member is required']"
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
          <v-btn variant="text" @click="closeDialog">Cancel</v-btn>
          <v-btn color="primary" @click="saveGroup" :disabled="!formValid">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400px">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete the group "{{ groupToDelete?.name }}"?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteGroup">Delete</v-btn>
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
const dialog = ref(false)
const deleteDialog = ref(false)
const formValid = ref(false)
const groupForm = ref(null)

const users = ref<any[]>([])
const groupToDelete = ref<any>(null)

const editedGroup = ref<{
  id: string | null
  name: string
  description: string
  members: string[]
  createdAt: string | null
  updatedAt: string | null
}>({
  id: null,
  name: '',
  description: '',
  members: [],
  createdAt: null,
  updatedAt: null
})

const defaultGroup = {
  id: null,
  name: '',
  description: '',
  members: [],
  createdAt: null,
  updatedAt: null
}

const headers = [
  { title: 'Name', key: 'name', sortable: true },
  { title: 'Description', key: 'description', sortable: false },
  { title: 'Members', key: 'memberCount', sortable: true },
  { title: 'Created', key: 'createdAt', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const }
]

const filteredGroups = computed(() => {
  if (!search.value) return groupStore.groups
  const s = search.value.toLowerCase()
  return groupStore.groups.filter(g => 
    g.name?.toLowerCase().includes(s) ||
    g.description?.toLowerCase().includes(s)
  )
})

const availableUsers = computed(() => {
  return users.value.map(u => ({
    id: u.id || u._id,
    displayName: `${u.firstName} ${u.lastName} (${u.email})`
  }))
})

const required = (v: any) => !!v || 'This field is required'

const formatDate = (date: string) => {
  if (!date) return 'N/A'
  return new Date(date).toLocaleDateString()
}

onMounted(async () => {
  await loadGroups()
  await loadUsers()
})

async function loadGroups() {
  try {
    await groupStore.fetchGroups()
  } catch (err: any) {
    notificationStore.showNotification('Failed to load groups: ' + err.message, 'error')
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

function openGroupDialog() {
  editedGroup.value = { ...defaultGroup }
  dialog.value = true
}

function editGroup(group: any) {
  editedGroup.value = { 
    ...group,
    members: group.members || []
  }
  dialog.value = true
}

function closeDialog() {
  dialog.value = false
  editedGroup.value = { ...defaultGroup }
}

async function saveGroup() {
  if (!formValid.value) return

  try {
    if (editedGroup.value.id) {
      const { id, createdAt, updatedAt, ...updateData } = editedGroup.value
      await groupStore.updateGroup(id, updateData)
      notificationStore.showNotification('Group updated successfully', 'success')
    } else {
      const { id, createdAt, updatedAt, ...createData } = editedGroup.value
      await groupStore.createGroup(createData)
      notificationStore.showNotification('Group created successfully', 'success')
    }
    closeDialog()
    await loadGroups() // Reload from server to ensure consistency
  } catch (err: any) {
    notificationStore.showNotification('Failed to save group: ' + err.message, 'error')
  }
}

function confirmDelete(group: any) {
  groupToDelete.value = group
  deleteDialog.value = true
}

async function deleteGroup() {
  if (!groupToDelete.value) return

  try {
    await groupStore.deleteGroup(groupToDelete.value.id)
    notificationStore.showNotification('Group deleted successfully', 'success')
    deleteDialog.value = false
    groupToDelete.value = null
  } catch (err: any) {
    notificationStore.showNotification('Failed to delete group: ' + err.message, 'error')
  }
}
</script>

<style scoped>
.bold-headers :deep(th) {
  font-weight: bold !important;
}
</style>
