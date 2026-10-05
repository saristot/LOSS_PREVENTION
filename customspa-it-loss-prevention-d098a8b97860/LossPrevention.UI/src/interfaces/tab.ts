export type LogicalOperator = 'AND' | 'OR' | 'NOR'

export interface Condition {
  id: string
  field: string
  operator: string
  value: any
}

export interface ConditionGroup {
  id: string
  type: LogicalOperator
  conditions: (Condition | ConditionGroup)[]
  formattingRules?: FormattingRule[]
}

export interface FormattingRule {
  id: string
  field: string
  operator: string
  value: any
  color: string
}

export interface Tab {
  id: string
  title: string
  description: string
  selectedFields: any[]
  groupByField: string
  query: ConditionGroup
  designMode: boolean
}

export interface FieldDefinition {
  name: string;
  alias: string;
  dataType: string;
  groupBy: boolean;
  aggregation: string;
  isCalculated: boolean;
  expression: string;
  prefix: string;
  suffix: string;
  visible?: boolean; // Added for visibility toggle feature
}

