# Euclidean Distance Analysis - Quick Start Guide

## What Is It?

A tool that finds records most similar to a selected record based on field values. Think of it as "find more like this" for your data.

## When To Use It

- **Find fraud patterns**: Compare a fraudulent transaction to find similar suspicious ones
- **Detect anomalies**: Identify unusual records by seeing what's "normal"
- **Find duplicates**: Locate potential duplicate records
- **Pattern analysis**: Group similar transactions or events

## Quick Steps

### 1. Run Your Query
Execute a query that returns the type of records you want to analyze.

### 2. Select a Record
In the results grid, click on field values from ONE row:
- ✅ Select at least 3 fields (5-10 is better)
- ✅ Include numeric fields (amounts, counts, scores)
- ✅ Include the `_id` field
- ✅ Include date fields

### 3. Open Analysis
Click the **"Euclidean Distance"** button.

### 4. Configure
In the dialog:
- Choose **Start Date Field** and **End Date Field** (these filter which records to compare)
- Set the **date range** (wider = more results but slower)
- Pick a **Legend Label Field** (helps identify records, like Customer Name or Transaction ID)

### 5. Run & Review
- Click **"Run Distance Analysis"**
- View the **Radar Chart** for visual comparison
- Review the **Results Table** sorted by similarity

## Understanding The Scores

### Overall Score (Most Important)
- **90-100%**: Nearly identical - potential duplicate
- **70-89%**: Very similar - strong pattern match
- **50-69%**: Moderately similar - loose pattern
- **Below 50%**: Not very similar

### Field Match
Shows what % of selected fields could be compared.
- Low % = many fields were missing in the comparison record

### Distance Match  
Shows how close the actual values are.
- 100% = identical values
- Lower % = more different values

## Tips for Best Results

### DO:
✅ Select 5-10 relevant fields  
✅ Include a mix of numeric and categorical fields  
✅ Use fields that exist in most records  
✅ Choose meaningful date ranges  
✅ Select a good key field for identification  

### DON'T:
❌ Select fewer than 3 fields  
❌ Include only unique fields (like secondary IDs)  
❌ Use extremely narrow date ranges  
❌ Select fields with mostly null values  

## Common Issues

### "Must select at least 3 fields"
**Problem**: Not enough fields selected from the query results.  
**Solution**: Click more field values in the results grid before opening the dialog.

### "No similar records found"
**Problem**: No matches in the date range or selected fields don't overlap with other records.  
**Solution**: Expand your date range or select more common fields.

### All scores are low
**Problem**: Your selected record is genuinely unique/unusual.  
**Solution**: This might be expected! It indicates the record is an outlier.

### Missing required fields
**Problem**: Didn't fill in all the date fields.  
**Solution**: Select date fields and enter/confirm their values.

## Example Use Case

### Fraud Detection Scenario

**Goal**: Find transactions similar to a known fraudulent transaction.

**Steps**:
1. Query transactions from last 6 months
2. Click on a fraudulent transaction's values:
   - Amount: $1,234.56
   - MerchantType: Online Retail
   - Location: New York
   - Time: 2:30 AM
   - CustomerAge: 65
   - RiskScore: 8

3. Open Distance Analysis
4. Set date range: Last 6 months
5. Choose key field: TransactionID
6. Run analysis

**Results**:
- Top 10 transactions most similar to the fraudulent one
- High scores (>80%) indicate potential fraud
- Review these transactions for patterns

## Need More Help?

- Click the **?** (help) icon in the dialog for detailed explanation
- See `EUCLIDEAN_DISTANCE_ANALYSIS.md` for comprehensive documentation
- Contact your system administrator for backend configuration questions

## Visual Guide

```
┌─────────────────────────────────────────┐
│ Query Results Grid                      │
│ ┌─────────┬────────┬────────┬─────────┐│
│ │_id      │Amount  │Location│Date     ││  ← Click these values
│ │abc123   │$1,500  │NYC     │2024-1-1 ││     to select fields
│ │xyz789   │$2,200  │LA      │2024-1-2 ││
│ └─────────┴────────┴────────┴─────────┘│
└─────────────────────────────────────────┘
                 ↓
      [Euclidean Distance] ← Click button
                 ↓
┌─────────────────────────────────────────┐
│ Distance Analysis Dialog                │
│                                         │
│ Selected Fields: 4                      │
│ [_id] [Amount] [Location] [Date]        │
│                                         │
│ Start Date Field: [Date ▼]             │
│ Start Date: [2024-01-01 00:00]         │
│ End Date Field: [Date ▼]               │
│ End Date: [2024-12-31 23:59]           │
│                                         │
│ Legend Field: [TransactionID ▼]        │
│                                         │
│     [Run Distance Analysis]             │
│                                         │
│ Results:                                │
│ ┌─────────────────────────────────────┐│
│ │Key     Score  FieldMatch  Distance  ││
│ │TXN-789 92.5%  100%        85.0%     ││ ← Most similar
│ │TXN-456 87.3%  100%        74.6%     ││
│ │TXN-234 81.2%  83%         79.4%     ││
│ └─────────────────────────────────────┘│
└─────────────────────────────────────────┘
```

## Performance Tips

- **Smaller date ranges** = faster results
- **More records** in date range = slower processing
- **More fields** = more accurate but slightly slower
- Typical analysis takes 1-5 seconds

## Data Privacy Note

This analysis only compares fields you've selected and only shows records you have permission to view. The comparison happens server-side and no data is stored beyond the active session.
