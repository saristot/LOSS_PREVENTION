import { Tab, FieldDefinition } from '@/interfaces/tab'

export function createTab(id: string, title = '', description = '', p0: any): Tab {
  return {
    id,
    title,
    description,
    selectedFields: [],
    groupByField: '',
    query: {
      type: 'AND',
      conditions: [],
      id: ''
    },
    designMode: true
  };
}

export function normalizeField(field: any, mapping: any): FieldDefinition {
  return {
    name: field.name || mapping.name || '',
    alias: field.alias || mapping.alias || '',
    dataType: field.dataType || mapping.dataType || '',
    groupBy: field.groupBy ?? false,
    aggregation: field.aggregation || '',
    isCalculated: field.isCalculated ?? false,
    expression: field.expression || '',
    prefix: field.prefix || '',
    suffix: field.suffix || ''
  };
}
