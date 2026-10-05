<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center">
        <span>Permissions</span>
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
        <v-btn color="primary" class="mr-2" @click="openDialog()">Add Permission</v-btn>
        <v-btn color="primary" variant="outlined" class="mr-2" @click="permissionStore.fetchAllPermissions()">
          Refresh
        </v-btn>
      </v-card-title>

      <v-data-table
        class="bold-headers"
        :items="permissions"
        :headers="headers"
        :loading="loading"
        :search="search"
        item-key="_id"
      >
        <template #item.actions="{ item }">
          <v-icon size="small" class="mr-2" @click="openDialog(item)">mdi-pencil</v-icon>
          <v-icon size="small" color="red" @click="confirmDelete(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialog" max-width="500">
      <v-card>
        <v-card-title>
          {{ editedPermission._id ? 'Edit Permission' : 'Add Permission' }}
        </v-card-title>
        <v-card-text>
          <v-text-field label="Permission Name" v-model="editedPermission.permissionName" />
          <v-text-field label="Permission Text" v-model="editedPermission.permissionText" />
          <v-text-field label="Description" v-model="editedPermission.description" />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn class="mr-2" @click="closeDialog()">Cancel</v-btn>
          <v-btn color="primary" @click="savePermission()">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the permission
          <strong>{{ permissionToDelete?.permissionName }}</strong>?
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
import { ref, onMounted } from 'vue';
import { usePermissionStore } from '@/stores/permissionStore';
import { storeToRefs } from 'pinia';

const permissionStore = usePermissionStore();
const { permissions, loading } = storeToRefs(permissionStore);

const dialog = ref(false);
const deleteDialog = ref(false);
const search = ref(''); // search model

const editedPermission = ref({
  _id: null as string | null,
  permissionName: '',
  permissionText: '',
  description: ''
});
const permissionToDelete = ref<any>(null);

// Vuetify 3 headers use title/key
const headers = [
  { title: 'Permission Name', key: 'permissionName' },
  { title: 'Permission Text', key: 'permissionText' },
  { title: 'Description',     key: 'description' },
  { title: 'Actions',         key: 'actions', sortable: false, align: 'end' as const }
] as const;

onMounted(() => {
  permissionStore.fetchAllPermissions();
});

function openDialog(permission: any = null) {
  editedPermission.value = permission
    ? { ...permission }
    : { _id: null, permissionName: '', permissionText: '', description: '' };
  dialog.value = true;
}

function closeDialog() {
  dialog.value = false;
}

async function savePermission() {
  if (editedPermission.value._id) {
    await permissionStore.updatePermission(editedPermission.value);
  } else {
    await permissionStore.createPermission({
      permissionName: editedPermission.value.permissionName,
      permissionText: editedPermission.value.permissionText,
      description: editedPermission.value.description
    });
  }
  await permissionStore.fetchAllPermissions();
  closeDialog();
}

function confirmDelete(permission: any) {
  permissionToDelete.value = permission;
  deleteDialog.value = true;
}

async function performDelete() {
  if (permissionToDelete.value) {
    await permissionStore.deletePermission(permissionToDelete.value._id);
    await permissionStore.fetchAllPermissions();
  }
  deleteDialog.value = false;
  permissionToDelete.value = null;
}
</script>
<style scoped>
.bold-headers :deep(thead th),
.bold-headers :deep(.v-table__th),
.bold-headers :deep(.v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>