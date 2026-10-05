# Fraud Detection Settings API Contract

This document describes the API endpoints needed to support the fraud detection settings functionality.

## Endpoints

### 1. Get Fraud Detection Settings
Retrieves the saved fraud detection threshold configuration.

**Endpoint:** `GET /api/fraud-detection/settings`

**Response (200 OK):**
```json
{
  "id": "string (MongoDB ObjectId or UUID)",
  "thresholds": {
    "highValue": {
      "percentile": 0.95,
      "stdDevMultiplier": 2,
      "minimumValue": 500,
      "description": "Flag transactions in top 5% that are 2+ standard deviations above mean and exceed minimum value",
      "detects": "Unusually large transactions that may indicate theft, unauthorized purchases, or data entry errors"
    },
    "extremeHighValue": {
      "percentile": 0.99,
      "multiplier": 1.3,
      "minimumValue": 1000,
      "description": "Flag top 1% of transactions that are 1.3x the 95th percentile value",
      "detects": "Extreme value transactions suggesting major theft, money laundering, or fraudulent activity"
    },
    "lowValue": {
      "percentile": 0.05,
      "meanMultiplier": 0.1,
      "minMultiplier": 2,
      "description": "Flag bottom 5% transactions that are suspiciously low (less than 10% of mean)",
      "detects": "Transaction padding schemes where employees ring up small items to appear busy while stealing"
    },
    "refundAmount": {
      "percentile": 0.95,
      "description": "Flag top 5% of refund amounts",
      "detects": "Return fraud, receipt fraud, or employees issuing fake refunds to steal cash"
    },
    "refundCount": {
      "percentile": 0.95,
      "description": "Flag top 5% of refund counts",
      "detects": "Excessive returns indicating organized return fraud or employee theft schemes"
    },
    "voidAmount": {
      "percentile": 0.95,
      "description": "Flag top 5% of void amounts",
      "detects": "Void fraud where employees void high-value items after customer pays to pocket difference"
    },
    "voidCount": {
      "percentile": 0.90,
      "description": "Flag top 10% of void counts",
      "detects": "Excessive voids suggesting scanning fraud or transaction manipulation"
    },
    "discountAmount": {
      "percentile": 0.90,
      "meanMultiplier": 1.5,
      "description": "Flag top 10% of discount amounts that are 1.5x the mean",
      "detects": "Unauthorized discounts, employee theft through excessive markdowns, or discount abuse"
    },
    "discountCount": {
      "percentile": 0.90,
      "minimum": 1,
      "description": "Flag top 10% of discount counts (minimum 1 discount)",
      "detects": "Employees giving unauthorized discounts to friends/family (sweethearting)"
    },
    "manualOverride": {
      "percentile": 0.90,
      "description": "Flag top 10% of manual overrides",
      "detects": "Price manipulation, unauthorized adjustments, or override abuse"
    },
    "priceOverride": {
      "percentile": 0.85,
      "description": "Flag top 15% of price overrides",
      "detects": "Employees manually changing prices to steal or favor customers"
    },
    "itemQuantity": {
      "percentile": 0.95,
      "multiplier": 1.5,
      "description": "Flag top 5% of item counts that are 1.5x the threshold",
      "detects": "Unusual bulk purchases or scanning fraud (scanning 1 item while customer takes multiple)"
    },
    "giftCardAmount": {
      "percentile": 0.95,
      "description": "Flag top 5% of gift card amounts",
      "detects": "Gift card fraud, money laundering, or employees issuing fake gift cards"
    },
    "giftCardCount": {
      "percentile": 0.90,
      "minimum": 1,
      "description": "Flag top 10% of gift card counts",
      "detects": "Multiple gift card purchases suggesting fraud rings or money laundering"
    },
    "paymentMethods": {
      "percentile": 0.95,
      "minimum": 1,
      "description": "Flag top 5% of payment method counts",
      "detects": "Payment switching fraud or testing stolen credit cards with small amounts"
    },
    "loyaltyPoints": {
      "percentile": 0.95,
      "description": "Flag top 5% of loyalty redemptions",
      "detects": "Loyalty fraud where employees or customers fraudulently redeem excessive points"
    },
    "noSaleEvents": {
      "percentile": 0.90,
      "minimum": 1,
      "description": "Flag top 10% of no-sale/drawer open events",
      "detects": "Cash theft through excessive till access without legitimate transactions"
    },
    "velocity": {
      "windowMinutes": 30,
      "minTransactions": 5,
      "percentile": 0.95,
      "description": "Flag entities with 5+ transactions in 30 minutes in top 5%",
      "detects": "Rapid-fire transactions suggesting scanning fraud, rushed theft, or employee collusion"
    },
    "temporal": {
      "offHoursStart": 22,
      "offHoursEnd": 6,
      "weekendPenalty": 0.5,
      "description": "Flag transactions during off-hours (10 PM - 6 AM)",
      "detects": "After-hours theft, unauthorized access, or suspicious timing patterns"
    },
    "splitTransaction": {
      "windowMinutes": 60,
      "thresholdProximity": 0.1,
      "minSplits": 2,
      "commonThresholds": [50, 100, 200, 500, 1000, 2500, 5000, 10000],
      "description": "Flag transactions just below approval thresholds within 10%",
      "detects": "Structuring/smurfing to avoid approval limits or monitoring thresholds"
    },
    "sweethearting": {
      "minOccurrences": 3,
      "discountPercentile": 0.80,
      "timeWindowDays": 30,
      "description": "Flag employee-customer pairs with 3+ transactions in 30 days",
      "detects": "Employee-customer collusion where employees give discounts/free items to friends/family"
    },
    "returnFraud": {
      "returnRatePercentile": 0.90,
      "returnAmountRatio": 0.5,
      "frequentReturner": 3,
      "windowDays": 30,
      "description": "Flag returns exceeding 50% of purchases or 3+ returns in 30 days",
      "detects": "Professional return fraud, wardrobing, receipt fraud, or return abuse"
    }
  },
  "createdAt": "2025-12-07T10:00:00Z",
  "updatedAt": "2025-12-07T15:30:00Z",
  "createdBy": "user@example.com",
  "updatedBy": "user@example.com"
}
```

**Response (404 Not Found):** No settings saved yet
```json
{
  "message": "No fraud detection settings found",
  "code": "SETTINGS_NOT_FOUND"
}
```

---

### 2. Create Fraud Detection Settings
Creates a new fraud detection settings record.

**Endpoint:** `POST /api/fraud-detection/settings`

**Request Body:**
```json
{
  "thresholds": {
    // Same structure as GET response above
  }
}
```

**Response (201 Created):**
```json
{
  "id": "string (MongoDB ObjectId or UUID)",
  "thresholds": { /* ... */ },
  "createdAt": "2025-12-07T10:00:00Z",
  "updatedAt": "2025-12-07T10:00:00Z",
  "createdBy": "current-user@example.com",
  "updatedBy": "current-user@example.com"
}
```

---

### 3. Update Fraud Detection Settings
Updates existing fraud detection settings.

**Endpoint:** `PUT /api/fraud-detection/settings/:id`

**Path Parameters:**
- `id` - The settings document ID

**Request Body:**
```json
{
  "id": "string (MongoDB ObjectId or UUID)",
  "thresholds": {
    // Same structure as GET response above
  },
  "updatedAt": "2025-12-07T15:30:00Z"
}
```

**Response (200 OK):**
```json
{
  "id": "string (MongoDB ObjectId or UUID)",
  "thresholds": { /* ... */ },
  "createdAt": "2025-12-07T10:00:00Z",
  "updatedAt": "2025-12-07T15:30:00Z",
  "createdBy": "user@example.com",
  "updatedBy": "current-user@example.com"
}
```

**Response (404 Not Found):**
```json
{
  "message": "Settings not found",
  "code": "SETTINGS_NOT_FOUND"
}
```

---

## Database Schema (MongoDB)

### Collection: `fraudDetectionSettings`

```javascript
{
  _id: ObjectId,
  thresholds: {
    highValue: {
      percentile: Number,
      stdDevMultiplier: Number,
      minimumValue: Number,
      description: String,
      detects: String
    },
    // ... all other threshold configurations
  },
  createdAt: ISODate,
  updatedAt: ISODate,
  createdBy: String,  // User email or ID
  updatedBy: String   // User email or ID
}
```

**Indexes:**
- `{ updatedAt: -1 }` - For retrieving most recent settings

**Notes:**
- Typically only one document should exist (singleton pattern)
- Could be scoped by organization/tenant if multi-tenant
- Consider adding versioning if audit trail is needed

---

## Error Handling

All endpoints should return appropriate HTTP status codes:

- `200 OK` - Successful GET/PUT
- `201 Created` - Successful POST
- `400 Bad Request` - Invalid request body or validation errors
- `401 Unauthorized` - User not authenticated
- `403 Forbidden` - User lacks permission to modify settings
- `404 Not Found` - Settings not found
- `500 Internal Server Error` - Server error

**Error Response Format:**
```json
{
  "message": "Human-readable error message",
  "code": "ERROR_CODE",
  "details": { /* Optional additional context */ }
}
```

---

## Security Considerations

1. **Authentication:** All endpoints require valid JWT token
2. **Authorization:** Only admin users should be able to modify settings
3. **Validation:** Server must validate all threshold values are within reasonable ranges
4. **Audit:** Log all changes to fraud detection settings
5. **Versioning:** Consider storing historical versions for rollback capability

---

## Implementation Notes

1. The frontend will automatically load settings on component mount
2. Settings are cached in Pinia store until page refresh
3. Changes are applied immediately in the UI but require clicking "Save to Server" to persist
4. If server returns 404, the frontend will gracefully use built-in defaults
5. The `id` field is tracked client-side to determine whether to POST (create) or PUT (update)
