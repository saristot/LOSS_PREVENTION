<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center">
        <span>Mappings</span>
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
        <v-btn color="success" class="mr-2" size="small" @click="openMappingDialog">Add Mapping</v-btn>
        <v-btn color="primary" class="mr-2" @click="loadMappings">Refresh</v-btn>
      </v-card-title>

      <v-card-text>
        <v-data-table
          class="bold-headers"
          :headers="headers"
          :items="mappings"
          :loading="loading"
          :search="search"
          item-key="_id"
        >
          <template #item.actions="{ item }">
            <v-icon size="small" class="mr-2" @click="editMapping(item)">mdi-pencil</v-icon>
            <v-icon size="small" color="red" @click="confirmDelete(item)">mdi-delete</v-icon>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Dialog -->
    <v-dialog v-model="dialog" max-width="600px">
      <v-card>
        <v-card-title>{{ editedMapping._id ? 'Edit' : 'Create' }} Mapping</v-card-title>
        <v-card-text>
          <v-form ref="mappingForm" v-model="formValid">
            <v-text-field v-model="editedMapping.name" label="Name" :rules="[required]" />
            <v-text-field v-model="editedMapping.alias" label="Alias" :rules="[required]" />
            <v-select
              v-model="editedMapping.dataType"
              :items="['String', 'Integer', 'Decimal', 'Date', 'Boolean']"
              label="Data Type"
            />
            <v-text-field v-model="editedMapping.collectionName" label="Collection Name" :rules="[required]" />
            <v-text-field v-model="editedMapping.longestLength" label="Longest Length" type="number" />
            <v-switch v-model="editedMapping.isVisible" label="Is Visible" />
            <v-switch v-model="editedMapping.isCalculated" label="Is Calculated" />
            <v-switch v-model="editedMapping.isLookup" label="Is Lookup" />
            <v-switch v-model="editedMapping.isArray" label="Is Array" />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-btn class="mr-2" @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" :disabled="!formValid" @click="saveMapping">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete mapping <strong>{{ mappingToDelete?.name }}</strong>?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn class="mr-2" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="red" @click="performDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, computed } from 'vue'
import { useMappingStore } from '@/stores/mappingStore'

const mappingStore = useMappingStore()

const mappings = computed(() =>
  (mappingStore.mappings || []).map((m: any) => ({
    _id: m._id,
    name: m.name,
    alias: m.alias,
    dataType: m.dataType,
    isVisible: m.isVisible ?? true,
    isArray: m.isArray ?? false,
    isCalculated: m.isCalculated ?? false,
    isLookup: m.isLookup ?? false,
    collectionName: m.collectionName ?? 'Mappings',
    longestLength: m.longestLength ?? null
  }))
)

const loading = computed(() => mappingStore.loading)

/* Vuetify 3 headers use title/key */
const headers = [
  { title: 'Name',        key: 'name' },
  { title: 'Alias',       key: 'alias' },
  { title: 'Data Type',   key: 'dataType' },
  { title: 'Collection',  key: 'collectionName' },
  { title: 'Visible',     key: 'isVisible' },
  { title: 'Calculated',  key: 'isCalculated' },
  { title: 'Lookup',      key: 'isLookup' },
  { title: 'Array',       key: 'isArray' },
  { title: 'Length',      key: 'longestLength' },
  { title: 'Actions',     key: 'actions', sortable: false, align: 'end' as const }
]

/* search */
const search = ref('')

const required = (v: string) => !!v || 'This field is required'

const dialog = ref(false)
const deleteDialog = ref(false)
const mappingForm = ref()
const formValid = ref(false)

const emptyMapping = () => ({
  _id: '',
  name: '',
  alias: '',
  dataType: 'String',
  isVisible: true,
  isArray: false,
  isCalculated: false,
  isLookup: false,
  collectionName: 'Mappings',
  longestLength: null
})

const editedMapping = ref(emptyMapping())
const mappingToDelete = ref<any>(null)

const loadMappings = async () => {
  await mappingStore.fetchMappings()
}

const openMappingDialog = () => {
  editedMapping.value = emptyMapping()
  dialog.value = true
  nextTick(() => {
    mappingForm.value?.resetValidation()
  })
}

const editMapping = (mapping: any) => {
  editedMapping.value = { ...mapping }
  dialog.value = true
  nextTick(() => {
    mappingForm.value?.resetValidation()
  })
}

const saveMapping = async () => {
  const valid = await mappingForm.value?.validate()
  if (!valid) return

  const payload = {
    Name: editedMapping.value.name,
    Alias: editedMapping.value.alias,
    DataType: editedMapping.value.dataType,
    IsVisible: editedMapping.value.isVisible,
    IsArray: editedMapping.value.isArray,
    IsCalculated: editedMapping.value.isCalculated,
    IsLookup: editedMapping.value.isLookup,
    CollectionName: editedMapping.value.collectionName,
    LongestLength: editedMapping.value.longestLength
  }

  if (editedMapping.value._id) {
    await mappingStore.updateMapping(editedMapping.value._id, payload)
  } else {
    await mappingStore.createMapping(payload)
  }

  dialog.value = false
  await loadMappings()
}

const confirmDelete = (mapping: any) => {
  mappingToDelete.value = mapping
  deleteDialog.value = true
}

const performDelete = async () => {
  if (mappingToDelete.value?._id) {
    await mappingStore.deleteMapping(mappingToDelete.value._id)
    deleteDialog.value = false
    mappingToDelete.value = null
    await loadMappings()
  }
}

onMounted(loadMappings)
</script>
<style scoped>
.bold-headers :deep(thead th),
.bold-headers :deep(.v-table__th),
.bold-headers :deep(.v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>
