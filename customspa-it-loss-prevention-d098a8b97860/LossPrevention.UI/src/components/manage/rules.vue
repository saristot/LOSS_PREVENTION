<template>
  <v-container>
    <v-card>
      <v-card-title class="d-flex align-center">
        <span>Rules</span>
        <v-spacer />

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

        <v-btn class="mr-2" color="secondary" @click="openDialog()">Add Rule</v-btn>
        <v-btn class="mr-2" color="secondary" @click="applyRulesToAll">Apply Rules</v-btn>
        <v-btn class="mr-2" color="secondary" variant="outlined" @click="ruleStore.fetchAllRules()">Refresh</v-btn>
      </v-card-title>

      <!-- Apply results dialog -->
      <v-dialog v-model="resultDialog" max-width="800">
        <v-card>
          <v-card-title class="text-h6">Rules Applied</v-card-title>
          <v-card-text>
            <div v-if="applyResult">
              <p>Total Documents: {{ applyResult.TotalDocuments }}</p>
              <v-expansion-panels>
                <v-expansion-panel v-for="(doc, index) in applyResult.Results" :key="index">
                  <v-expansion-panel-title>{{ doc._id }}</v-expansion-panel-title>
                  <v-expansion-panel-text>
                    <ul>
                      <li v-for="rule in doc.RuleResults" :key="rule.RuleName">
                        {{ rule.RuleName }}: {{ rule.Matched ? '✅ Match' : '❌ No Match' }}
                      </li>
                    </ul>
                  </v-expansion-panel-text>
                </v-expansion-panel>
              </v-expansion-panels>
            </div>
          </v-card-text>
          <v-card-actions>
            <v-spacer />
            <v-btn @click="resultDialog = false">Close</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-data-table
        class="bold-headers"
        :items="rules"
        :headers="headers"
        :loading="loading"
        :search="search"
        item-key="id"
      >
        <template #item.actions="{ item }">
          <v-icon size="small" class="mr-2" @click="openDialog(item)">mdi-pencil</v-icon>
          <v-icon size="small" color="red" @click="confirmDelete(item)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card>

    <!-- Add / Edit dialog -->
    <v-dialog v-model="dialog" max-width="700">
      <v-card>
        <v-card-title>{{ editedRule.id ? 'Edit Rule' : 'Add Rule' }}</v-card-title>
        <v-card-text>

          <!-- Plain-language summary -->
          <v-alert
            v-if="ruleSummary"
            type="info"
            variant="tonal"
            class="mb-5"
          >
            {{ ruleSummary }}
          </v-alert>

          <v-form ref="form">
            <v-text-field
              label="Rule Name"
              v-model="editedRule.ruleName"
              :rules="[required]"
              class="mb-2"
            />
            <v-text-field
              label="Description"
              v-model="editedRule.ruleDescription"
              class="mb-2"
            />
            <v-text-field
              label="Document Field"
              v-model="editedRule.fieldPath"
              :rules="[required]"
              hint="The field in the transaction document to evaluate (e.g. Type, TotalAmount, TransactionType)"
              persistent-hint
              class="mb-4"
            />

            <v-switch
              label="Range Check"
              v-model="editedRule.allowRangeCheck"
              hint="Enable to flag records where the field value falls within a numeric range instead of matching an exact value"
              persistent-hint
              class="mb-4"
              @update:model-value="onRangeCheckToggle"
            />

            <!-- Exact match — shown when range check is off -->
            <v-text-field
              v-if="!editedRule.allowRangeCheck"
              label="Match Value"
              v-model="editedRule.valueToCheck"
              :rules="[required]"
              hint="The exact value the field must equal to trigger this rule"
              persistent-hint
              class="mb-2"
            />

            <!-- Range bounds — shown when range check is on -->
            <template v-if="editedRule.allowRangeCheck">
              <v-text-field
                label="Min Value"
                v-model="editedRule.minValue"
                :rules="[required, numeric]"
                hint="Minimum value (inclusive)"
                persistent-hint
                class="mb-2"
              />
              <v-text-field
                label="Max Value"
                v-model="editedRule.maxValue"
                :rules="[required, numeric]"
                hint="Maximum value (inclusive)"
                persistent-hint
                class="mb-2"
              />
            </template>

            <v-switch
              label="Enabled"
              v-model="editedRule.enabled"
              class="mb-1"
            />
            <v-switch
              label="Sum Values"
              v-model="editedRule.sumValues"
              hint="Sum all values across array fields before comparing (e.g. total quantity across multiple line items)"
              persistent-hint
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn class="mr-2" @click="closeDialog()">Cancel</v-btn>
          <v-btn color="primary" @click="saveRule()">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete confirmation dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the rule <strong>{{ ruleToDelete?.ruleName }}</strong>?
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
import { computed, onMounted, ref, nextTick } from 'vue';
import { useRuleStore } from '@/stores/ruleStore';
import { storeToRefs } from 'pinia';
import type { RuleConfiguration } from '@/interfaces/ruletypes';

const ruleStore = useRuleStore();
const { rules, loading } = storeToRefs(ruleStore);

const dialog = ref(false);
const deleteDialog = ref(false);
const search = ref('');
const form = ref<any>(null);

const emptyRule = (): RuleConfiguration => ({
  id: undefined,
  ruleName: '',
  ruleDescription: '',
  fieldPath: '',
  valueToCheck: '',
  allowRangeCheck: false,
  minValue: '',
  maxValue: '',
  enabled: true,
  sumValues: false
});

const editedRule = ref<RuleConfiguration>(emptyRule());
const ruleToDelete = ref<RuleConfiguration | null>(null);

const headers = [
  { title: 'Rule Name',      key: 'ruleName' },
  { title: 'Document Field', key: 'fieldPath' },
  { title: 'Enabled',        key: 'enabled' },
  { title: 'Actions',        key: 'actions', sortable: false, align: 'end' as const }
] as const;

// Validation
const required = (v: string) => !!v?.trim() || 'This field is required';
const numeric  = (v: string) => /^-?\d*\.?\d+$/.test(v?.trim() ?? '') || 'Must be a valid number';

// Plain-language summary
const ruleSummary = computed(() => {
  const field = editedRule.value.fieldPath?.trim();
  if (!field) return '';

  if (editedRule.value.allowRangeCheck) {
    const min = editedRule.value.minValue?.trim();
    const max = editedRule.value.maxValue?.trim();
    if (min && max) return `Flags records where "${field}" is between ${min} and ${max} (inclusive)`;
    if (min)        return `Flags records where "${field}" is ${min} or greater`;
    if (max)        return `Flags records where "${field}" is ${max} or less`;
    return `Flags records where "${field}" falls within a numeric range`;
  }

  const value = editedRule.value.valueToCheck?.trim();
  if (value) return `Flags records where "${field}" equals "${value}"`;
  return `Flags records by the "${field}" field`;
});

// Clear irrelevant fields when toggling range check mode
function onRangeCheckToggle(rangeEnabled: boolean) {
  if (rangeEnabled) {
    editedRule.value.valueToCheck = '';
  } else {
    editedRule.value.minValue = '';
    editedRule.value.maxValue = '';
  }
}

onMounted(async () => {
  await ruleStore.fetchAllRules();
});

function openDialog(rule: RuleConfiguration | null = null) {
  editedRule.value = rule ? { ...rule } : emptyRule();
  dialog.value = true;
  nextTick(() => form.value?.resetValidation());
}

function closeDialog() {
  dialog.value = false;
}

async function saveRule() {
  const { valid } = await form.value.validate();
  if (!valid) return;

  if (editedRule.value.id) {
    await ruleStore.updateRule(editedRule.value);
  } else {
    await ruleStore.addRule(editedRule.value);
  }
  await ruleStore.fetchAllRules();
  closeDialog();
}

function confirmDelete(rule: RuleConfiguration) {
  ruleToDelete.value = rule;
  deleteDialog.value = true;
}

async function performDelete() {
  if (ruleToDelete.value?.id) {
    await ruleStore.deleteRule(ruleToDelete.value.id);
    await ruleStore.fetchAllRules();
  }
  deleteDialog.value = false;
  ruleToDelete.value = null;
}

const resultDialog = ref(false);
const applyResult = ref<any>(null);

async function applyRulesToAll() {
  const result = await ruleStore.applyRules();
  if (result) {
    applyResult.value = result;
    resultDialog.value = true;
  } else {
    console.error('Error applying rules:', ruleStore.error);
  }
}
</script>

<style scoped>
.bold-headers :deep(thead th),
.bold-headers :deep(.v-table__th),
.bold-headers :deep(.v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>
