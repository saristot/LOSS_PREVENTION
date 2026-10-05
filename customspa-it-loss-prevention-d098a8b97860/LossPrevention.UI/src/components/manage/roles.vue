<template> 
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center">
        <span>Roles</span>
        <v-spacer></v-spacer>

        <!-- Search -->
        <v-text-field
          v-model="search"
          placeholder="Search…"
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          style="max-width: 260px"
          class="mr-2"
        />

        <!-- Actions -->
        <v-btn color="primary" class="mr-2" @click="openDialog()">Add Role</v-btn>
        <v-btn color="primary" variant="outlined" class="mr-2" @click="roleStore.fetchAllRoles()">
          Refresh
        </v-btn>
      </v-card-title>

      <v-data-table
        class="bold-headers"
        :items="roles"
        :headers="headers"
        :loading="loading"
        :search="search"
        item-key="_id"
      >
        <template #item.actions="{ item }">
          <v-icon size="small" class="mr-2" @click="openDialog(item)">mdi-pencil</v-icon>
          <v-icon
            size="small"
            color="red"
            @click="confirmDelete(item)"
            :disabled="item.roleName === 'Admin'"
            :class="{ 'v-icon--disabled': item.roleName === 'Admin' }"
          >
            mdi-delete
          </v-icon>
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialog" max-width="600">
      <v-card>
        <v-card-title>
          {{ editedRole._id ? 'Edit Role' : 'Add Role' }}
        </v-card-title>
        <v-card-text>
          <v-text-field label="Role Name" v-model="editedRole.roleName" />
          <v-text-field label="Description" v-model="editedRole.description" />
          <v-autocomplete
            label="Permissions"
            v-model="editedRole.permissions"
            :items="permissions"
            item-title="permissionText"
            item-value="_id"
            multiple
            chips
            clearable
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn class="mr-2" @click="closeDialog()">Cancel</v-btn>
          <v-btn color="primary" @click="saveRole()">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the role <strong>{{ roleToDelete?.roleName }}</strong>?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn class="mr-2" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="red" @click="performDelete()">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoleStore } from '@/stores/roleStore';
import { usePermissionStore } from '@/stores/permissionStore';
import { storeToRefs } from 'pinia';

const roleStore = useRoleStore();
const permissionStore = usePermissionStore();

const { roles, loading } = storeToRefs(roleStore);
const { permissions } = storeToRefs(permissionStore);

const dialog = ref(false);
const deleteDialog = ref(false);
const search = ref('');

const editedRole = ref({
  _id: null as string | null,
  roleName: '',
  description: '',
  permissions: [] as string[]
});
const originalPermissions = ref<string[]>([]);
const roleToDelete = ref<any>(null);

// Vuetify 3 headers (title/key)
const headers = [
  { title: 'Role Name',  key: 'roleName' },
  { title: 'Description', key: 'description' },
  { title: 'Actions',     key: 'actions', sortable: false, align: 'end' as const }
] as const;

onMounted(async () => {
  await roleStore.fetchAllRoles();
  await permissionStore.fetchAllPermissions();
});

async function openDialog(role: any = null) {
  if (role && role._id) {
    const permissionsForRole = await permissionStore.getPermissionsForRole(role._id);
    editedRole.value = {
      ...role,
      permissions: permissionsForRole.map((p: any) => p._id)
    };
    originalPermissions.value = [...editedRole.value.permissions];
  } else {
    editedRole.value = {
      _id: null,
      roleName: '',
      description: '',
      permissions: []
    };
    originalPermissions.value = [];
  }
  dialog.value = true;
}

function closeDialog() {
  dialog.value = false;
}

async function saveRole() {
  if (editedRole.value._id) {
    await roleStore.updateRole(editedRole.value);

    const added = editedRole.value.permissions.filter(
      p => !originalPermissions.value.includes(p)
    );
    const removed = originalPermissions.value.filter(
      p => !editedRole.value.permissions.includes(p)
    );

    for (const permissionId of added) {
      await permissionStore.addPermissionToRole(editedRole.value._id, permissionId);
    }
    for (const permissionId of removed) {
      await permissionStore.removePermissionFromRole(editedRole.value._id, permissionId);
    }
  } else {
    await roleStore.addRole({
      RoleName: editedRole.value.roleName,
      Description: editedRole.value.description,
      Permissions: []
    });

    const created = await roleStore.getRoleByName(editedRole.value.roleName);
    if (created && created._id) {
      for (const permissionId of editedRole.value.permissions) {
        await permissionStore.addPermissionToRole(created._id, permissionId);
      }
    }
  }

  await roleStore.fetchAllRoles();
  closeDialog();
}

function confirmDelete(role: any) {
  if (role.roleName === 'Admin') return;
  roleToDelete.value = role;
  deleteDialog.value = true;
}

async function performDelete() {
  if (roleToDelete.value && roleToDelete.value.roleName !== 'Admin') {
    await roleStore.deleteRole({ RoleId: roleToDelete.value._id });
    await roleStore.fetchAllRoles();
  }
  deleteDialog.value = false;
  roleToDelete.value = null;
}
</script>
<style scoped>
.bold-headers :deep(thead th),
.bold-headers :deep(.v-table__th),
.bold-headers :deep(.v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>