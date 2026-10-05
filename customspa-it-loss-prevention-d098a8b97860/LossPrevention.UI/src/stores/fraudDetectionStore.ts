import { defineStore } from "pinia";
import { markRaw } from 'vue';
import api from '@/api/api';

// ======================================================
//   FRAUD DETECTION THRESHOLDS & CONFIGURATION
// ======================================================
export interface FraudThresholdConfig {
  // High Value Transactions
  highValue: {
    percentile: number;
    stdDevMultiplier: number;
    minimumValue: number;
    description: string;
    detects: string;
  };
  extremeHighValue: {
    percentile: number;
    multiplier: number;
    minimumValue: number;
    description: string;
    detects: string;
  };
  
  // Low Value Transactions (Padding)
  lowValue: {
    percentile: number;
    meanMultiplier: number;
    minMultiplier: number;
    description: string;
    detects: string;
  };
  
  // Refunds
  refundAmount: {
    percentile: number;
    description: string;
    detects: string;
  };
  refundCount: {
    percentile: number;
    description: string;
    detects: string;
  };
  
  // Voids
  voidAmount: {
    percentile: number;
    description: string;
    detects: string;
  };
  voidCount: {
    percentile: number;
    description: string;
    detects: string;
  };
  
  // Discounts
  discountAmount: {
    percentile: number;
    meanMultiplier: number;
    description: string;
    detects: string;
  };
  discountCount: {
    percentile: number;
    minimum: number;
    description: string;
    detects: string;
  };
  
  // Overrides
  manualOverride: {
    percentile: number;
    description: string;
    detects: string;
  };
  priceOverride: {
    percentile: number;
    description: string;
    detects: string;
  };
  
  // Item Quantities
  itemQuantity: {
    percentile: number;
    multiplier: number;
    description: string;
    detects: string;
  };
  
  // Gift Cards
  giftCardAmount: {
    percentile: number;
    description: string;
    detects: string;
  };
  giftCardCount: {
    percentile: number;
    minimum: number;
    description: string;
    detects: string;
  };
  
  // Payments
  paymentMethods: {
    percentile: number;
    minimum: number;
    description: string;
    detects: string;
  };
  
  // Loyalty
  loyaltyPoints: {
    percentile: number;
    description: string;
    detects: string;
  };
  
  // Till/Drawer
  noSaleEvents: {
    percentile: number;
    minimum: number;
    description: string;
    detects: string;
  };
  
  // Velocity Detection
  velocity: {
    windowMinutes: number;
    minTransactions: number;
    percentile: number;
    description: string;
    detects: string;
  };
  
  // Temporal Patterns
  temporal: {
    offHoursStart: number;
    offHoursEnd: number;
    weekendPenalty: number;
    description: string;
    detects: string;
  };
  
  // Split Transaction Detection
  splitTransaction: {
    windowMinutes: number;
    thresholdProximity: number;
    minSplits: number;
    commonThresholds: number[];
    description: string;
    detects: string;
  };
  
  // Sweethearting
  sweethearting: {
    minOccurrences: number;
    discountPercentile: number;
    timeWindowDays: number;
    description: string;
    detects: string;
  };
  
  // Return Fraud
  returnFraud: {
    returnRatePercentile: number;
    returnAmountRatio: number;
    frequentReturner: number;
    windowDays: number;
    description: string;
    detects: string;
  };
}

// Default fraud detection thresholds with descriptions
const DEFAULT_FRAUD_THRESHOLDS: FraudThresholdConfig = {
  highValue: {
    percentile: 0.95,
    stdDevMultiplier: 2,
    minimumValue: 500,
    description: "Flag transactions in top 5% that are 2+ standard deviations above mean and exceed minimum value",
    detects: "Unusually large transactions that may indicate theft, unauthorized purchases, or data entry errors"
  },
  extremeHighValue: {
    percentile: 0.99,
    multiplier: 1.3,
    minimumValue: 1000,
    description: "Flag top 1% of transactions that are 1.3x the 95th percentile value",
    detects: "Extreme value transactions suggesting major theft, money laundering, or fraudulent activity"
  },
  lowValue: {
    percentile: 0.05,
    meanMultiplier: 0.1,
    minMultiplier: 2,
    description: "Flag bottom 5% transactions that are suspiciously low (less than 10% of mean)",
    detects: "Transaction padding schemes where employees ring up small items to appear busy while stealing"
  },
  refundAmount: {
    percentile: 0.95,
    description: "Flag top 5% of refund amounts",
    detects: "Return fraud, receipt fraud, or employees issuing fake refunds to steal cash"
  },
  refundCount: {
    percentile: 0.95,
    description: "Flag top 5% of refund counts",
    detects: "Excessive returns indicating organized return fraud or employee theft schemes"
  },
  voidAmount: {
    percentile: 0.95,
    description: "Flag top 5% of void amounts",
    detects: "Void fraud where employees void high-value items after customer pays to pocket difference"
  },
  voidCount: {
    percentile: 0.90,
    description: "Flag top 10% of void counts",
    detects: "Excessive voids suggesting scanning fraud or transaction manipulation"
  },
  discountAmount: {
    percentile: 0.90,
    meanMultiplier: 1.5,
    description: "Flag top 10% of discount amounts that are 1.5x the mean",
    detects: "Unauthorized discounts, employee theft through excessive markdowns, or discount abuse"
  },
  discountCount: {
    percentile: 0.90,
    minimum: 1,
    description: "Flag top 10% of discount counts (minimum 1 discount)",
    detects: "Employees giving unauthorized discounts to friends/family (sweethearting)"
  },
  manualOverride: {
    percentile: 0.90,
    description: "Flag top 10% of manual overrides",
    detects: "Price manipulation, unauthorized adjustments, or override abuse"
  },
  priceOverride: {
    percentile: 0.85,
    description: "Flag top 15% of price overrides",
    detects: "Employees manually changing prices to steal or favor customers"
  },
  itemQuantity: {
    percentile: 0.95,
    multiplier: 1.5,
    description: "Flag top 5% of item counts that are 1.5x the threshold",
    detects: "Unusual bulk purchases or scanning fraud (scanning 1 item while customer takes multiple)"
  },
  giftCardAmount: {
    percentile: 0.95,
    description: "Flag top 5% of gift card amounts",
    detects: "Gift card fraud, money laundering, or employees issuing fake gift cards"
  },
  giftCardCount: {
    percentile: 0.90,
    minimum: 1,
    description: "Flag top 10% of gift card counts",
    detects: "Multiple gift card purchases suggesting fraud rings or money laundering"
  },
  paymentMethods: {
    percentile: 0.95,
    minimum: 1,
    description: "Flag top 5% of payment method counts",
    detects: "Payment switching fraud or testing stolen credit cards with small amounts"
  },
  loyaltyPoints: {
    percentile: 0.95,
    description: "Flag top 5% of loyalty redemptions",
    detects: "Loyalty fraud where employees or customers fraudulently redeem excessive points"
  },
  noSaleEvents: {
    percentile: 0.90,
    minimum: 1,
    description: "Flag top 10% of no-sale/drawer open events",
    detects: "Cash theft through excessive till access without legitimate transactions"
  },
  velocity: {
    windowMinutes: 30,
    minTransactions: 5,
    percentile: 0.95,
    description: "Flag entities with 5+ transactions in 30 minutes in top 5%",
    detects: "Rapid-fire transactions suggesting scanning fraud, rushed theft, or employee collusion"
  },
  temporal: {
    offHoursStart: 22,
    offHoursEnd: 6,
    weekendPenalty: 0.5,
    description: "Flag transactions during off-hours (10 PM - 6 AM)",
    detects: "After-hours theft, unauthorized access, or suspicious timing patterns"
  },
  splitTransaction: {
    windowMinutes: 60,
    thresholdProximity: 0.1,
    minSplits: 2,
    commonThresholds: [50, 100, 200, 500, 1000, 2500, 5000, 10000],
    description: "Flag transactions just below approval thresholds within 10%",
    detects: "Structuring/smurfing to avoid approval limits or monitoring thresholds"
  },
  sweethearting: {
    minOccurrences: 3,
    discountPercentile: 0.80,
    timeWindowDays: 30,
    description: "Flag employee-customer pairs with 3+ transactions in 30 days",
    detects: "Employee-customer collusion where employees give discounts/free items to friends/family"
  },
  returnFraud: {
    returnRatePercentile: 0.90,
    returnAmountRatio: 0.5,
    frequentReturner: 3,
    windowDays: 30,
    description: "Flag returns exceeding 50% of purchases or 3+ returns in 30 days",
    detects: "Professional return fraud, wardrobing, receipt fraud, or return abuse"
  }
};

// ======================================================
//   FRAUD DETECTION STORE
// ======================================================
export const useFraudDetectionStore = defineStore("fraudDetectionStore", {
  state: () => ({
    thresholds: { ...DEFAULT_FRAUD_THRESHOLDS } as FraudThresholdConfig,
    fraudRulesCache: [] as any[],
    fraudMetadataMap: new Map() as Map<string, {
      fraudTypes: string[];
      severity: string;
      confidence: number;
      explanation: string;
      relevantFields: Record<string, any>;
    }>,
    // Map composite transaction keys to MongoDB _id values
    txIdToMongoIdMap: new Map() as Map<string, string>,
    loadingFraudAnalysis: false,
    cancelAnalysis: false,
    processedCount: 0,
    totalRows: 0,
    fraudCount: 0,
    currentBatch: 0,
    totalBatches: 0,
    elapsedTimeMs: 0,
    _timer: null as any,
    loadingSettings: false,
    savingSettings: false,
    lastSavedAt: null as Date | null,
    settingsId: null as string | null,
  }),

  getters: {
    /**
     * Get all threshold categories for the settings UI
     */
    thresholdCategories(): Array<{
      category: string;
      items: Array<{
        key: string;
        label: string;
        config: any;
      }>;
    }> {
      return [
        {
          category: "Transaction Values",
          items: [
            { key: "highValue", label: "High Value Transactions", config: this.thresholds.highValue },
            { key: "extremeHighValue", label: "Extreme High Value", config: this.thresholds.extremeHighValue },
            { key: "lowValue", label: "Low Value (Padding)", config: this.thresholds.lowValue }
          ]
        },
        {
          category: "Refunds & Returns",
          items: [
            { key: "refundAmount", label: "Refund Amount", config: this.thresholds.refundAmount },
            { key: "refundCount", label: "Refund Count", config: this.thresholds.refundCount },
            { key: "returnFraud", label: "Return Fraud Patterns", config: this.thresholds.returnFraud }
          ]
        },
        {
          category: "Voids & Cancellations",
          items: [
            { key: "voidAmount", label: "Void Amount", config: this.thresholds.voidAmount },
            { key: "voidCount", label: "Void Count", config: this.thresholds.voidCount }
          ]
        },
        {
          category: "Discounts & Overrides",
          items: [
            { key: "discountAmount", label: "Discount Amount", config: this.thresholds.discountAmount },
            { key: "discountCount", label: "Discount Count", config: this.thresholds.discountCount },
            { key: "manualOverride", label: "Manual Overrides", config: this.thresholds.manualOverride },
            { key: "priceOverride", label: "Price Overrides", config: this.thresholds.priceOverride }
          ]
        },
        {
          category: "Items & Quantities",
          items: [
            { key: "itemQuantity", label: "Item Quantities", config: this.thresholds.itemQuantity }
          ]
        },
        {
          category: "Gift Cards & Loyalty",
          items: [
            { key: "giftCardAmount", label: "Gift Card Amount", config: this.thresholds.giftCardAmount },
            { key: "giftCardCount", label: "Gift Card Count", config: this.thresholds.giftCardCount },
            { key: "loyaltyPoints", label: "Loyalty Points", config: this.thresholds.loyaltyPoints }
          ]
        },
        {
          category: "Payment Patterns",
          items: [
            { key: "paymentMethods", label: "Payment Methods", config: this.thresholds.paymentMethods }
          ]
        },
        {
          category: "Cash Management",
          items: [
            { key: "noSaleEvents", label: "No Sale Events", config: this.thresholds.noSaleEvents }
          ]
        },
        {
          category: "Behavioral Patterns",
          items: [
            { key: "velocity", label: "Transaction Velocity", config: this.thresholds.velocity },
            { key: "temporal", label: "Off-Hours Activity", config: this.thresholds.temporal },
            { key: "splitTransaction", label: "Split Transactions", config: this.thresholds.splitTransaction },
            { key: "sweethearting", label: "Sweethearting", config: this.thresholds.sweethearting }
          ]
        }
      ];
    }
  },

  actions: {
    /**
     * Load fraud detection settings from the API
     */
    async loadSettings() {
      this.loadingSettings = true;
      try {
        // Mock API call - replace with actual endpoint
        const response = await api.get('/api/fraud-detection/settings');
        
        if (response.data && response.data.thresholds) {
          this.thresholds = { ...DEFAULT_FRAUD_THRESHOLDS, ...response.data.thresholds };
          this.settingsId = response.data.id || null;
          this.lastSavedAt = response.data.updatedAt ? new Date(response.data.updatedAt) : null;
          this.clearFraudRulesCache();
          console.log('✅ Fraud detection settings loaded from API');
          return true;
        } else {
          // No saved settings, use defaults
          console.log('ℹ️ No saved settings found, using defaults');
          this.thresholds = { ...DEFAULT_FRAUD_THRESHOLDS };
          return false;
        }
      } catch (error: any) {
        console.error('❌ Failed to load fraud detection settings:', error);
        // On error, use defaults
        this.thresholds = { ...DEFAULT_FRAUD_THRESHOLDS };
        // Return false but don't throw - gracefully fallback to defaults
        return false;
      } finally {
        this.loadingSettings = false;
      }
    },

    /**
     * Save fraud detection settings to the API
     */
    async saveSettings() {
      this.savingSettings = true;
      try {
        // Mock API call - replace with actual endpoint
        const payload = {
          id: this.settingsId,
          thresholds: this.thresholds,
          updatedAt: new Date().toISOString()
        };

        let response;
        if (this.settingsId) {
          // Update existing settings
          response = await api.put(`/api/fraud-detection/settings/${this.settingsId}`, payload);
        } else {
          // Create new settings
          response = await api.post('/api/fraud-detection/settings', payload);
        }

        if (response.data) {
          this.settingsId = response.data.id || this.settingsId;
          this.lastSavedAt = new Date();
          console.log('✅ Fraud detection settings saved to API');
          return true;
        }
        return false;
      } catch (error: any) {
        console.error('❌ Failed to save fraud detection settings:', error);
        throw new Error(`Failed to save settings: ${error.message || 'Unknown error'}`);
      } finally {
        this.savingSettings = false;
      }
    },

    /**
     * Update a specific threshold configuration
     */
    updateThreshold(key: keyof FraudThresholdConfig, config: any) {
      this.thresholds[key] = { ...this.thresholds[key], ...config };
      // Clear cache to force regeneration with new thresholds
      this.clearFraudRulesCache();
    },

    /**
     * Reset all thresholds to defaults
     */
    resetToDefaults() {
      this.thresholds = { ...DEFAULT_FRAUD_THRESHOLDS };
      this.clearFraudRulesCache();
    },

    /**
     * Reset a specific threshold to default
     */
    resetThreshold(key: keyof FraudThresholdConfig) {
      this.thresholds[key] = { ...DEFAULT_FRAUD_THRESHOLDS[key] } as any;
      this.clearFraudRulesCache();
    },

    /**
     * Get fraud metadata for a specific document ID
     */
    getFraudMetadata(documentId: string) {
      return this.fraudMetadataMap.get(String(documentId));
    },

    /**
     * Check if a document is flagged as fraudulent
     */
    isFraudulent(documentId: string): boolean {
      return this.fraudMetadataMap.has(String(documentId));
    },

    /**
     * Get all fraudulent document IDs (MongoDB _id values)
     */
    getFraudulentIds(): string[] {
      return Array.from(this.fraudMetadataMap.keys());
    },

    /**
     * Store fraud metadata using MongoDB _id (converted from composite key if needed)
     */
    storeFraudMetadata(txId: string, metadata: {
      fraudTypes: string[];
      severity: string;
      confidence: number;
      explanation: string;
      relevantFields: Record<string, any>;
    }) {
      // Convert composite key to MongoDB _id if mapping exists
      const mongoId = this.txIdToMongoIdMap.get(txId) || txId;
      this.fraudMetadataMap.set(mongoId, metadata);
    },

    /**
     * Build mapping from composite transaction keys to MongoDB _id values
     */
    buildTxIdMapping(rows: any[], groupingFields: string[]) {
      this.txIdToMongoIdMap.clear();
      
      rows.forEach((row, idx) => {
        let txId: string;
        if (groupingFields.length > 0) {
          txId = groupingFields.map(field => String(row[field] || '')).filter(v => v).join('|');
          if (!txId) txId = String(idx);
        } else {
          txId = String(idx);
        }
        
        const mongoId = String(row._id || '');
        if (mongoId && !this.txIdToMongoIdMap.has(txId)) {
          this.txIdToMongoIdMap.set(txId, mongoId);
        }
      });
    },

    /**
     * Clear fraud rules cache (forces regeneration)
     */
    clearFraudRulesCache() {
      this.fraudRulesCache = [];
    },

    /**
     * Detect fields that uniquely identify transactions (generic approach).
     * Looks for fields like approval codes, confirmation numbers, transaction IDs, etc.
     * Returns array of field names that should be used for grouping.
     */
    detectUniqueTransactionFields(rows: any[]): string[] {
      if (!rows || rows.length === 0) return [];
      
      const uniqueIdFields: string[] = [];
      const sampleSize = Math.min(100, rows.length);
      const sample = rows.slice(0, sampleSize);
      
      // Common patterns for unique transaction identifiers
      const uniqueIdPatterns = [
        /approval/i,
        /confirmation/i,
        /authorization/i,
        /auth/i,
        /reference/i,
        /receipt/i,
        /invoice/i,
        /order.*id/i,
        /transaction.*id/i,
        /trans.*id/i,
        /ticket/i,
        /trace/i,
        /sequence/i
      ];
      
      // Test each field
      const fields = Object.keys(sample[0] || {});
      
      for (const field of fields) {
        // Skip _id and system fields
        if (field === '_id' || field.startsWith('_')) continue;
        
        // Check if field name matches unique ID patterns
        const matchesPattern = uniqueIdPatterns.some(pattern => pattern.test(field));
        
        if (matchesPattern) {
          // Verify the field has high uniqueness (at least 80% unique values in sample)
          const values = sample.map(r => String(r[field] || '')).filter(v => v);
          const uniqueValues = new Set(values);
          const uniquenessRatio = uniqueValues.size / Math.max(values.length, 1);
          
          // Also check that values are not empty/null for most rows
          const nonEmptyRatio = values.length / sampleSize;
          
          if (uniquenessRatio >= 0.8 && nonEmptyRatio >= 0.7) {
            console.log(`✅ Detected unique ID field: "${field}" (${(uniquenessRatio * 100).toFixed(1)}% unique, ${(nonEmptyRatio * 100).toFixed(1)}% populated)`);
            uniqueIdFields.push(field);
          }
        }
      }
      
      // If no pattern-based fields found, look for fields with high cardinality
      if (uniqueIdFields.length === 0) {
        for (const field of fields) {
          if (field === '_id' || field.startsWith('_')) continue;
          
          const values = sample.map(r => String(r[field] || '')).filter(v => v);
          const uniqueValues = new Set(values);
          const uniquenessRatio = uniqueValues.size / Math.max(values.length, 1);
          const nonEmptyRatio = values.length / sampleSize;
          
          // Very high uniqueness suggests it's a transaction identifier
          if (uniquenessRatio >= 0.95 && nonEmptyRatio >= 0.9) {
            console.log(`✅ Detected high-cardinality unique field: "${field}" (${(uniquenessRatio * 100).toFixed(1)}% unique)`);
            uniqueIdFields.push(field);
          }
        }
      }
      
      return uniqueIdFields;
    },

    /**
     * Stop fraud analysis
     */
    stopFraudAnalysis() {
      this.cancelAnalysis = true;
    },

    /**
     * Reset analysis state
     */
    resetAnalysisState() {
      this.fraudMetadataMap.clear();
      this.processedCount = 0;
      this.totalRows = 0;
      this.fraudCount = 0;
      this.cancelAnalysis = false;
      this.currentBatch = 0;
      this.totalBatches = 0;
      this.elapsedTimeMs = 0;
    },

    /**
     * Timer helpers
     */
    _startTimer(start: number) {
      this._stopTimer();
      this._timer = setInterval(() => {
        this.elapsedTimeMs = Math.round(performance.now() - start);
      }, 100);
    },

    _stopTimer() {
      if (this._timer) clearInterval(this._timer);
      this._timer = null;
    },

    /**
     * Export thresholds to JSON
     */
    exportThresholds(): string {
      return JSON.stringify(this.thresholds, null, 2);
    },

    /**
     * Import thresholds from JSON
     */
    importThresholds(json: string): boolean {
      try {
        const imported = JSON.parse(json);
        // Validate structure (basic check)
        if (typeof imported === 'object' && imported !== null) {
          this.thresholds = { ...DEFAULT_FRAUD_THRESHOLDS, ...imported };
          this.clearFraudRulesCache();
          return true;
        }
        return false;
      } catch (error) {
        console.error('Failed to import thresholds:', error);
        return false;
      }
    }
  }
});
