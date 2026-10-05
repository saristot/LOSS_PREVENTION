export interface RuleConfiguration {
  id?: string;
  ruleName: string;
  ruleDescription?: string;
  fieldPath: string;
  valueToCheck?: string;
  allowRangeCheck: boolean;
  minValue?: string;
  maxValue?: string;
  enabled: boolean;
  sumValues: boolean;
}
