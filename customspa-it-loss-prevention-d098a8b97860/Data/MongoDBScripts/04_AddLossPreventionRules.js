// =========================================================================
// MongoDB Script: Add Retail Loss Prevention Rules
// =========================================================================
// Purpose: Fixes broken existing rules and inserts standard retail LP rules.
// Run with: mongosh "mongodb://localhost:27017" --file 04_AddLossPreventionRules.js
// =========================================================================

var db = db.getSiblingDB('LossPrevention');

// -------------------------------------------------------------------------
// Fix existing rules that reference wrong field paths
// -------------------------------------------------------------------------
// 'Total' does not exist — correct field is 'TotalAmount'
db.Rules.updateMany(
  { FieldPath: 'Total' },
  { $set: { FieldPath: 'TotalAmount' } }
);

// Transaction documents store payment type as a top-level 'Type' field, not a nested 'Tender.Type'.
// Fix any rules pointing at the wrong path.
db.Rules.updateMany(
  { FieldPath: { $in: ['TenderType', 'Tender.Type'] } },
  { $set: { FieldPath: 'Type' } }
);

// 'IsGiftCard' is superseded by 'GiftCardTender' which has a clearer name and description.
db.Rules.deleteOne({ RuleName: 'IsGiftCard' });

print('Existing rule field paths corrected.');

// -------------------------------------------------------------------------
// New retail loss prevention rules
// -------------------------------------------------------------------------
var newRules = [
  // --- Transaction type flags ---
  {
    RuleName:        'ReturnTransaction',
    RuleDescription: 'Flags all return transactions. Returns are a common vector for refund fraud and wardrobing.',
    FieldPath:       'TransactionType',
    ValueToCheck:    'Return',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'RefundTransaction',
    RuleDescription: 'Flags all refund transactions. Unauthorised refunds are one of the most common forms of cashier fraud.',
    FieldPath:       'TransactionType',
    ValueToCheck:    'Refund',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'VoidTransaction',
    RuleDescription: 'Flags voided transactions. Repeated voids can indicate sweethearting or register manipulation.',
    FieldPath:       'TransactionType',
    ValueToCheck:    'Void',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },

  // --- Cashier behaviour indicators ---
  {
    RuleName:        'ExcessiveVoids',
    RuleDescription: 'Transaction contains 3 or more item voids. May indicate sweethearting where items are voided after being passed through.',
    FieldPath:       'VoidsCount',
    ValueToCheck:    '',
    AllowRangeCheck: true,
    MinValue:        NumberDecimal('3'),
    MaxValue:        NumberDecimal('9999'),
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'MultiCouponTransaction',
    RuleDescription: 'Transaction has 2 or more coupons applied. Multiple coupons on a single transaction can indicate coupon fraud.',
    FieldPath:       'CouponCount',
    ValueToCheck:    '',
    AllowRangeCheck: true,
    MinValue:        NumberDecimal('2'),
    MaxValue:        NumberDecimal('9999'),
    Enabled:         true,
    SumValues:       false
  },

  // --- Payment method risk flags ---
  {
    RuleName:        'KeyedCardEntry',
    RuleDescription: 'Card number was manually keyed rather than physically swiped, dipped, or tapped. Significantly higher card-not-present fraud risk.',
    FieldPath:       'EntryMethod',
    ValueToCheck:    'Keyed',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'CashTransaction',
    RuleDescription: 'Transaction paid entirely in cash. Cash is untraceable and is the primary medium for internal theft and till skimming.',
    FieldPath:       'Type',
    ValueToCheck:    'Cash',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'GiftCardTender',
    RuleDescription: 'Transaction includes a gift card tender. Gift cards used in returns or for high-value purchases can indicate fraud or money laundering.',
    FieldPath:       'Type',
    ValueToCheck:    'GiftCard',
    AllowRangeCheck: false,
    MinValue:        null,
    MaxValue:        null,
    Enabled:         true,
    SumValues:       false
  },

  // --- Item-level risk flags ---
  {
    RuleName:        'BulkItemPurchase',
    RuleDescription: 'A single line item has a quantity of 5 or more units. Bulk purchasing of a single item can indicate organised retail crime.',
    FieldPath:       'LineItem.Quantity',
    ValueToCheck:    '',
    AllowRangeCheck: true,
    MinValue:        NumberDecimal('5'),
    MaxValue:        NumberDecimal('9999'),
    Enabled:         true,
    SumValues:       false
  },
  {
    RuleName:        'HighValueItem',
    RuleDescription: 'A line item has a unit price over $200. High-value individual items are frequent shoplifting targets and warrant closer scrutiny.',
    FieldPath:       'LineItem.UnitPrice',
    ValueToCheck:    '',
    AllowRangeCheck: true,
    MinValue:        NumberDecimal('200'),
    MaxValue:        NumberDecimal('99999'),
    Enabled:         true,
    SumValues:       false
  },

  // --- Transaction total anomalies ---
  {
    RuleName:        'NegativeTotalAmount',
    RuleDescription: 'Transaction total is negative. Negative totals outside of expected refund workflows may indicate fraudulent overrides or system manipulation.',
    FieldPath:       'TotalAmount',
    ValueToCheck:    '',
    AllowRangeCheck: true,
    MinValue:        NumberDecimal('-99999'),
    MaxValue:        NumberDecimal('-0.01'),
    Enabled:         true,
    SumValues:       false
  }
];

// Only insert rules that don't already exist (idempotent)
var inserted = 0;
newRules.forEach(function(rule) {
  var existing = db.Rules.findOne({ RuleName: rule.RuleName });
  if (!existing) {
    db.Rules.insertOne(rule);
    inserted++;
    print('Inserted: ' + rule.RuleName);
  } else {
    print('Skipped (already exists): ' + rule.RuleName);
  }
});

print('\nDone. ' + inserted + ' new rule(s) inserted.');
print('Run "Apply Rules" in the UI to evaluate all rules against the current dataset.');
