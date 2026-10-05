<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center gap-3">
        Users
        <v-spacer></v-spacer>

        <!-- Search -->
        <v-text-field
          v-model="search"
          placeholder="Search users..."
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          style="max-width: 260px"
          class="mr-2"
        />

        <v-btn color="success" class="mr-2" small @click="openUserDialog">Add User</v-btn>
        <v-btn color="primary" @click="loadUsers">Refresh</v-btn>
      </v-card-title>

      <v-card-text>
        <v-data-table
          class="bold-headers"
          :headers="headers"
          :items="filteredUsers"
          :loading="loading"
          item-value="id"
        >
          <template #item.name="{ item }">
            {{ item.firstName }} {{ item.lastName }}
          </template>

          <template #item.actions="{ item }">
            <v-icon small class="mr-2" @click="editUser(item)">mdi-pencil</v-icon>
            <v-icon
              small
              color="red"
              @click="confirmDelete(item)"
              :disabled="item.username === 'admin'"
              >mdi-delete</v-icon
            >
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Add/Edit User Dialog -->
    <v-dialog v-model="dialog" max-width="600px">
      <v-card>
        <v-card-title>{{ editedUser.id ? 'Edit' : 'Create' }} User</v-card-title>
        <v-card-text>
          <v-form ref="userForm" v-model="formValid">
            <v-text-field v-model="editedUser.username" label="Username" :rules="[required]" />
            <v-text-field v-model="editedUser.email" label="Email" :rules="[required, emailRule]" />
            <v-text-field v-model="editedUser.firstName" label="First Name" :rules="[required]" />
            <v-text-field v-model="editedUser.lastName" label="Last Name" :rules="[required]" />
            <v-text-field
              v-model="editedUser.registrationDate"
              label="Registration Date"
              type="datetime-local"
              :rules="[required]"
            />
            <v-switch v-model="editedUser.isActive" label="Active" color="green" />

            <v-combobox
              v-model="editedUser.roles"
              :items="allRoles"
              item-title="roleName"
              item-value="_id"
              multiple
              label="Roles"
              :rules="[v => v?.length > 0 || 'At least one role is required']"
            >
              <template #selection="{ item, index }">
                <v-chip :key="index" class="ma-1" size="small" color="primary" label>
                  {{ allRoles.find(x => x._id == item.value)?.roleName || item }}
                </v-chip>
              </template>
            </v-combobox>

            <template v-if="!editedUser.id">
              <v-text-field v-model="editedUser.password" label="Password" type="password" />
            </template>

            <!-- FIXED: use objects {title,value}; save canonical name -->
            <v-select
              v-model="editedUser.lockField"
              :items="mappingFieldOptions"
              item-title="title"
              item-value="value"
              label="Lock User By Field (optional)"
              clearable
            />

            <v-text-field
              v-if="editedUser.lockField"
              v-model="editedUser.lockValue"
              :label="`Value for ${editedUser.lockField}`"
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-btn @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" :disabled="!formValid" @click="saveUser">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete user <strong>{{ userToDelete?.username }}</strong>?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="red" @click="performDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import { useUserStore } from '@/stores/userStore'
import { useRoleStore } from '@/stores/roleStore'
import { useMappingStore } from '@/stores/mappingStore'
import { User } from '@/interfaces/user'

const required = (v: string) => !!v || 'This field is required'
const emailRule = (v: string) => /.+@.+\..+/.test(v) || 'E-mail must be valid'

const userStore = useUserStore()
const roleStore = useRoleStore()
const mappingsStore = useMappingStore()

const allRoles = ref<Record<string, any>[]>([])
const users = ref<User[]>([])
const loading = ref(false)

/* ----------------------------- table headers ----------------------------- */
const headers = [
  { title: 'Username', key: 'username' },
  { title: 'Email',   key: 'email' },
  { title: 'Name',    key: 'name' },
  { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
]

/* -------------------------------- search -------------------------------- */
const search = ref('')

// Client-side filter over common fields (username, email, first/last name)
const filteredUsers = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return users.value
  return users.value.filter(u => {
    const bag = [
      u.username,
      u.email,
      u.firstName,
      u.lastName,
    ]
    return bag.some(v => (v ?? '').toString().toLowerCase().includes(q))
  })
})

/* ---------------------------- dialogs/validation ------------------------- */
const dialog = ref(false)
const deleteDialog = ref(false)
const userToDelete = ref<User | null>(null)
const userForm = ref()
const formValid = ref(false)

/** Show alias in the dropdown, save canonical Name field */
const mappingFieldOptions = computed(() =>
  (mappingsStore.mappings || []).map((m: any) => ({
    title: m.alias ?? m.Alias ?? m.name ?? m.Name,
    value: m.name ?? m.Name
  }))
)

const normalize = (s?: string) => (s ?? '').toString().trim().toLowerCase()
const toCanonicalFieldName = (val?: string) => {
  if (!val) return ''
  const m = (mappingsStore.mappings || []).find(
    (mm: any) =>
      normalize(mm.name ?? mm.Name) === normalize(val) ||
      normalize(mm.alias ?? mm.Alias) === normalize(val)
  )
  return m ? (m.name ?? m.Name) : val
}

const emptyUser = (): User => ({
  username: '',
  password: '',
  email: '',
  firstName: '',
  lastName: '',
  registrationDate: new Date().toISOString().slice(0, 16),
  isActive: true,
  roles: [],
  lockField: '',
  lockValue: ''
})

const editedUser = ref<User>(emptyUser())

const updateUsers = () => {
  users.value = userStore.users.map((u: any) => ({
    id: u._id,
    username: u.username,
    password: '',
    email: u.email,
    firstName: u.firstName,
    lastName: u.lastName,
    registrationDate: u.registrationDate?.slice(0, 16),
    isActive: u.isActive ?? true,
    roles: u.roles ?? [],
    // normalize stored value to canonical
    lockField: toCanonicalFieldName(u.lockField) || '',
    lockValue: u.lockValue ?? ''
  }))
}

const loadUsers = async () => {
  loading.value = true
  try {
    await roleStore.fetchAllRoles()
    await userStore.fetchUsers()
    allRoles.value = roleStore.roles
    updateUsers()
  } finally {
    loading.value = false
  }
}

const openUserDialog = () => {
  editedUser.value = emptyUser()
  dialog.value = true
  nextTick(() => userForm.value?.resetValidation())
}

const editUser = (user: User) => {
  editedUser.value = {
    ...user,
    password: '',
    registrationDate: user.registrationDate?.slice(0, 16) || '',
    isActive: user.isActive ?? true,
    roles: [...(user.roles || [])],
    lockField: toCanonicalFieldName(user.lockField) || '',
    lockValue: user.lockValue || ''
  }
  dialog.value = true
  nextTick(() => userForm.value?.resetValidation())
}

const saveUser = async () => {
  const isValid = await userForm.value?.validate()
  if (!isValid) return

  const canonicalLockField = toCanonicalFieldName(editedUser.value.lockField)

  const payload = {
    ...editedUser.value,
    roles: editedUser.value.roles.map((r: any) => (typeof r === 'string' ? r : r._id)),
    lockField: canonicalLockField || undefined,
    lockValue: editedUser.value.lockValue || undefined
  }

  if (editedUser.value.id) {
    await userStore.updateUser(payload)
  } else {
    await userStore.createUser(payload)
  }

  await loadUsers()
  dialog.value = false
}

const confirmDelete = (user: User) => {
  if (user.username === 'admin') return
  userToDelete.value = user
  deleteDialog.value = true
}

const performDelete = async () => {
  if (userToDelete.value?.id) {
    await userStore.deleteUser(userToDelete.value.id)
    deleteDialog.value = false
    userToDelete.value = null
    await loadUsers()
  }
}

onMounted(async () => {
  await mappingsStore.fetchMappings()
  await loadUsers()
})
</script>
<style scoped>
.bold-headers :deep(thead th),
.bold-headers :deep(.v-table__th),
.bold-headers :deep(.v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>