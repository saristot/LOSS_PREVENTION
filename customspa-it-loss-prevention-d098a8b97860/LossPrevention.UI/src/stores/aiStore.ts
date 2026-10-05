import { defineStore } from "pinia";
import JSON5 from "json5";
import { useFraudDetectionStore } from "./fraudDetectionStore";

// ======================================================
//   LLM CONFIG (FAST, STRICT, DETERMINISTIC)
// ======================================================
const MODEL = "qwen2.5:14b"; // Use larger model with JSON mode for better accuracy
const LLM_OPTIONS = {
  temperature: 0.05,
  top_p: 0.8,
  num_predict: 1500,
  num_ctx: 4096
};

const LLM_OPTIONS_FAST = {
  temperature: 0.05,
  top_p: 0.85,
  num_predict: 1200,
  num_ctx: 2048,
  num_thread: 8
};

// ======================================================
//   HELPERS
// ======================================================
function extractJsonArray(text: string): string | null {
  text = text.replace(/```json|```/g, "").trim();
  const s = text.indexOf("[");
  const e = text.lastIndexOf("]");
  if (s < 0 || e <= s) return null;

  let extracted = text.slice(s, e + 1);

  extracted = extracted.replace(/,(\s*])/g, "$1");

  return extracted;
}

function clean(text: string): string {
  // Extract JSON from the response - find first { and matching }
  const firstBrace = text.indexOf('{');
  if (firstBrace === -1) {
    return text.trim();
  }
  
  // Find matching closing brace using bracket matching
  let depth = 0;
  let inString = false;
  let escapeNext = false;
  
  for (let i = firstBrace; i < text.length; i++) {
    const char = text[i];
    
    if (escapeNext) {
      escapeNext = false;
      continue;
    }
    
    if (char === '\\' && inString) {
      escapeNext = true;
      continue;
    }
    
    if (char === '"') {
      inString = !inString;
      continue;
    }
    
    if (!inString) {
      if (char === '{') depth++;
      if (char === '}') depth--;
      
      if (depth === 0) {
        text = text.substring(firstBrace, i + 1);
        break;
      }
    }
  }
  
  return text
    .replace(/ObjectId\("([^"]+)"\)/g, '"$1"')
    .replace(/ISODate\("([^"]+)"\)/g, '"$1"')
    .trim();
}

async function llm(prompt: string, signal?: AbortSignal, useJsonMode = true) {
  const res = await fetch("http://localhost:11434/api/generate", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      model: MODEL,
      prompt,
      stream: false,
      format: useJsonMode ? "json" : undefined, // Force JSON-only output
      options: LLM_OPTIONS
    }),
    signal
  });

  if (!res.ok) throw new Error("LLM error " + res.status);
  const json = await res.json();
  return (json.response || "").trim();
}

// ======================================================
//   SEMANTIC FIELD CATEGORIES (Generic Field Classification)
// ======================================================
// These patterns help identify field semantics without relying on exact names
// Users can extend/override via mappings with semanticType property
export const SEMANTIC_PATTERNS: Record<string, RegExp[]> = {
  // Transaction identifiers
  transaction_id: [/trans(action)?[_\-]?(id|num|no|number|ref)/i, /order[_\-]?(id|num|no)/i, /receipt/i, /invoice/i],
  
  // Monetary amounts
  amount: [/amount|total|price|cost|value|sum|subtotal|grand|net|gross|sale/i],
  refund_amount: [/refund.*amount|return.*amount|credit.*amount|rf[_\-]?amt/i, /amount.*refund|amount.*return/i],
  void_amount: [/void.*amount|cancel.*amount|vd[_\-]?amt/i, /amount.*void/i],
  discount_amount: [/discount|promo|coupon|markdown|reduction|savings/i],
  tax_amount: [/tax|vat|gst|hst|pst/i],
  
  // Counts
  item_count: [/item.*count|qty|quantity|units|line.*items?|sku.*count/i],
  refund_count: [/refund.*count|return.*count|rf[_\-]?cnt/i],
  void_count: [/void.*count|cancel.*count|vd[_\-]?cnt/i],
  discount_count: [/discount.*count|promo.*count|coupon.*count/i],
  
  // Override/adjustment fields
  manual_override: [/override|manual|adjust|modify|correction/i],
  price_override: [/price.*override|override.*price|price.*change|price.*adj/i],
  
  // Gift cards & vouchers
  gift_card: [/gift.*card|voucher|certificate|gc[_\-]?amt|stored.*value/i],
  
  // Payment methods
  payment_type: [/payment.*type|tender|pay.*method|payment.*method/i],
  cash_amount: [/cash|currency|notes|coins/i],
  card_amount: [/card|credit|debit|visa|master|amex/i],
  
  // Loyalty/rewards
  loyalty: [/loyal|point|reward|member|club|program/i],
  
  // Till/drawer operations
  no_sale: [/no.*sale|drawer.*open|till.*open|cash.*drop/i],
  
  // Entity identifiers
  employee_id: [/employee|emp[_\-]?id|staff|cashier|clerk|operator|user[_\-]?id|associate/i],
  customer_id: [/customer|cust[_\-]?id|member[_\-]?id|client|patron|shopper/i],
  store_id: [/store|location|branch|site|outlet|shop/i],
  register_id: [/register|till|terminal|pos|lane|checkout/i],
  
  // Time fields
  timestamp: [/date|time|ts|created|modified|posted|processed/i],
  
  // Product identifiers
  product_id: [/product|item[_\-]?id|sku|upc|ean|barcode|plu/i],
};

// ======================================================
//   STORE (OPTIMIZED WITH SHALLOW REACTIVITY)
// ======================================================
import { markRaw } from 'vue';

export const useAiStore = defineStore("aiStore", {
  state: () => ({
    question: "",
    nlqResult: null as {
      title: string;
      selectedFields: any[];
      query: any;
    } | null,
    fraudReports: [] as any[],
    // Large arrays - will be replaced wholesale, not mutated
    fraudAnalysisResults: [] as any[],
    // NEW: Lightweight fraud tracking (only _id + metadata)
    loading: false,
    loadingFraud: false,
    abortController: null as AbortController | null,
    _timer: null as any,
  }),

  actions: {
    // SEMANTIC FIELD CLASSIFICATION
    // -------------------------------------------
    /**
     * Classify a field by its semantic meaning using pattern matching and mapping hints.
     * Returns semantic category (e.g., 'refund_amount', 'employee_id') or null.
     */
    classifyFieldSemantic(fieldName: string, mapping?: any): string | null {
      // First check if mapping has explicit semanticType
      if (mapping?.semanticType) {
        return mapping.semanticType;
      }
      
      // Try pattern matching against all semantic categories
      for (const [category, patterns] of Object.entries(SEMANTIC_PATTERNS)) {
        for (const pattern of patterns) {
          if (pattern.test(fieldName)) {
            return category;
          }
        }
      }
      
      return null;
    },

    /**
     * Build a semantic field map for the dataset.
     * Returns: { category: [fieldName1, fieldName2, ...], ... }
     */
    buildSemanticFieldMap(fields: string[], mappings?: any[]): Record<string, string[]> {
      const semanticMap: Record<string, string[]> = {};
      const mappingByName = new Map(
        (mappings || []).map(m => [m.name?.toLowerCase(), m])
      );
      
      for (const field of fields) {
        const mapping = mappingByName.get(field.toLowerCase());
        const semantic = this.classifyFieldSemantic(field, mapping);
        
        if (semantic) {
          if (!semanticMap[semantic]) {
            semanticMap[semantic] = [];
          }
          semanticMap[semantic].push(field);
        }
      }
      
      return semanticMap;
    },

    /**
     * Get the first field matching a semantic category.
     */
    getSemanticField(semanticMap: Record<string, string[]>, category: string): string | null {
      return semanticMap[category]?.[0] || null;
    },

    /**
     * Get all fields matching a semantic category.
     */
    getSemanticFields(semanticMap: Record<string, string[]>, category: string): string[] {
      return semanticMap[category] || [];
    },

    // -------------------------------------------
    // NATURAL LANGUAGE → QUERY  (UNCHANGED)
    // -------------------------------------------
    async generateQuery(mappings: any[]) {
      const schemaLines = mappings
        .map(m => (m.name ? `- ${m.name}: ${m.dataType}` : ""))
        .join("\n");

      const mappingLines = mappings
        .map(m => (m.alias ? `- ${m.alias} → ${m.name}` : ""))
        .join("\n");

      const prompt = `RESPOND WITH ONLY A JSON OBJECT. NO EXPLANATIONS. NO MARKDOWN.

You generate MongoDB queries. Given a user request, output a single JSON object.

Available fields:
${schemaLines}

Field aliases:
${mappingLines}

User request: "${this.question}"

Output format (respond with this structure only, nothing else):
{"title":"descriptive title","selectedFields":[{"name":"FIELD.Name","groupBy":false,"aggregation":null}],"query":{"type":"AND","conditions":[{"field":"FIELD.Name","operator":"$gt","value":100}]}}

Operators: $eq, $ne, $gt, $gte, $lt, $lte, $in, $nin, $regex
Aggregations: null, sum, avg, count, min, max

JSON:`;

      this.loading = true;
      const start = performance.now();
      this._startTimer(start);

      try {
        const raw = clean(await llm(prompt));
        const parsed = JSON5.parse(raw);

        this.nlqResult = {
          title: parsed.title || this.question,
          selectedFields: (parsed.selectedFields || []).map((x: any) => ({
            name: x.name,
            groupBy: x.groupBy ?? false,
            aggregation: x.aggregation ?? null,
            visible: true
          })),
          query: parsed.query
        };
      } catch (err) {
        console.error("NLQ error:", err);
        this.nlqResult = null;
      } finally {
        this.loading = false;
        this._stopTimer();
      }
    },

    // -------------------------------------------
    // FRAUD REPORT GENERATION (UNCHANGED)
    // -------------------------------------------
    async generateFraudReports() {
      const list = [
        "- Statistical Outliers",
        "- Behavioral Anomalies",
        "- Payment Irregularities",
        "- Quantity / Ratio Anomalies",
        "- Session / Timing Anomalies",
        "- Identity / User Irregularities"
      ].join("\n");

      const prompt = `
Generate JSON ARRAY of fraud report templates.

Fraud types:
${list}

Output array of:
{
  "title": "string",
  "description": "string",
  "selectedFields": [
    { "name": "Field", "groupBy": false, "aggregation": null }
  ],
  "query": {
    "type": "AND",
    "conditions": [
      { "field": "Amount", "operator": "$gt", "value": 100 }
    ]
  }
}

Rules:
- Strict JSON.
- No placeholders.
`;

      this.loadingFraud = true;
      this.fraudReports = [];
      const start = performance.now();
      this._startTimer(start);

      try {
        const raw = clean(await llm(prompt));
        const json = JSON5.parse(extractJsonArray(raw) || "[]");

        this.fraudReports = json.map((x: any) => ({
          title: x.title,
          description: x.description,
          selectedFields: (x.selectedFields || []).map((f: any) => ({
            name: f.name,
            groupBy: f.groupBy ?? false,
            aggregation: f.aggregation ?? null,
            visible: true
          })),
          query: x.query,
          pipeline: x.pipeline || []
        }));
      } catch (err) {
        console.error("Fraud report error:", err);
      } finally {
        this.loadingFraud = false;
        this._stopTimer();
      }
    },
    // ======================================================
    //   GENERIC FRAUD ENGINE — FOUNDATIONS
    // ======================================================

    //--------------------------------------------------------
    // 1. CLASSIFY FIELDS (Universal Dataset Support - UPGRADED)
    //--------------------------------------------------------
    classifyFields(rows: any[]) {
      if (!rows || rows.length === 0) return {};
      const sample = rows.slice(0, Math.min(rows.length, 2000));
      const fields = Object.keys(sample[0]);
      const classification: Record<string, string> = {};

      const toNum = (v: any) => {
        if (typeof v === "number") return v;
        if (typeof v === "string" && v.trim() !== "" && !isNaN(Number(v)))
          return Number(v);
        return null;
      };

      for (const field of fields) {
        const vals = sample
          .map((r) => r[field])
          .filter((v) => v !== null && v !== undefined);

        if (vals.length === 0) {
          classification[field] = "unknown";
          continue;
        }

        // Check if this is an ID field (should NOT be treated as numeric)
        const isIdField = /id|barcode|sku|upc|ean|gtin|code|key|reference|serial|transaction.*num|order.*num|receipt/i.test(field);
        
        // Boolean inference
        const boolLike = vals.every((v) =>
          ["true", "false", "0", "1", 0, 1, true, false].includes(
            typeof v === "string" ? v.toLowerCase() : v
          )
        );
        if (boolLike) {
          classification[field] = "boolean";
          continue;
        }

        // Timestamp inference
        const ts = vals.filter((v) => !isNaN(Date.parse(v))).length / vals.length;
        if (ts > 0.7) {
          classification[field] = "timestamp";
          continue;
        }

        // Numeric-string conversion (but skip ID fields)
        const numVals = vals.map((v) => toNum(v)).filter((v) => v !== null);
        const ratio = numVals.length / vals.length;
        
        // If it's an ID field, treat as string even if numeric
        if (isIdField) {
          classification[field] = "string";
          continue;
        }
        
        if (ratio > 0.7) {
          const nums = numVals.sort((a, b) => a - b);
          const max = nums[nums.length - 1];
          const min = nums[0];
          const allIntegers = numVals.every(n => Number.isInteger(n));
          
          // Check cardinality - high cardinality numeric fields are likely IDs
          const uniq = new Set(vals);
          const cardinality = uniq.size / vals.length;
          
          // If almost all values are unique (>80% unique), likely an ID
          if (cardinality > 0.8 && allIntegers) {
            classification[field] = "string";
            continue;
          }
          
          // Better classification logic:
          // - If field name suggests count/quantity -> count
          // - If all values are integers and max <= 500 -> count
          // - If field name suggests amount/price/total -> amount
          // - Otherwise use max value heuristic
          
          const isCountName = /count|qty|quantity|items|num|number|units/i.test(field);
          const isAmountName = /amount|price|total|cost|value|dollar|sum|subtotal|grand/i.test(field);
          
          if (isCountName || (allIntegers && max <= 500 && min >= 0)) {
            classification[field] = "count";
          } else if (isAmountName || max > 500 || !allIntegers) {
            classification[field] = "amount";
          } else {
            // Fallback: small integers are counts, larger/decimals are amounts
            classification[field] = (allIntegers && max <= 200) ? "count" : "amount";
          }
          continue;
        }

        classification[field] = "string";
      }
      return classification;
    },

    //--------------------------------------------------------
    // 2. AUTO-DETECT GROUPING KEY (NEXT-GEN GENERIC VERSION)
    //--------------------------------------------------------
    detectGroupingKeys(rows: any[], classification: any) {
      if (!rows || rows.length === 0) return null;

      const sample = rows.slice(0, Math.min(2000, rows.length));
      const fields = Object.keys(sample[0]);

      // ---------------------------
      // Helper functions
      // ---------------------------

      const looksLikeUUID = (v: any) => {
        if (typeof v !== "string") return false;
        return (
          /^[0-9a-fA-F]{24}$/.test(v) ||              // Mongo ObjectId
          /^[0-9a-fA-F-]{32,36}$/.test(v) ||          // UUID-like
          /^[A-Za-z0-9]{10,40}$/.test(v)              // generic token
        );
      };

      const looksLikeNumericId = (v: any) =>
        typeof v === "number" && v > 0 && Number.isInteger(v);

      const isMostlyNumericId = (vals: any[]) =>
        vals.filter(looksLikeNumericId).length / vals.length > 0.8;

      const isMostlyUUID = (vals: any[]) =>
        vals.filter(looksLikeUUID).length / vals.length > 0.8;

      // ---------------------------
      // Compute cardinality, repetition, structure score
      // ---------------------------
      const scores: { field: string; score: number }[] = [];

      for (const f of fields) {
        const values = sample.map(r => r[f]).filter(v => v !== null && v !== undefined);
        if (values.length < 3) continue;

        const uniq = new Set(values);
        const cardinality = uniq.size / values.length; // 0 = repeated a lot, 1 = unique per row

        // Skip trivially unique fields (e.g., MongoDB _id)
        if (cardinality > 0.95) continue;

        let score = 0;

        // 1. Cardinality (best between 0.02 and 0.60)
        const target = 0.30; // ideal grouping ratio
        score += Math.max(0, 1 - Math.abs(cardinality - target) * 2);

        // 2. Repetition clusters (values repeating in consecutive rows)
        let repeats = 0;
        for (let i = 1; i < values.length; i++) {
          if (values[i] === values[i - 1]) repeats++;
        }
        const repeatRatio = repeats / values.length;
        score += repeatRatio * 0.5;

        // 3. ID-like structure bonus
        if (isMostlyUUID(values) || isMostlyNumericId(values)) {
          score += 0.3;
        }

        // 4. Field name heuristics
        const nameBonus = /id|trans|order|session|customer|user|account/i.test(f);
        if (nameBonus) score += 0.2;

        scores.push({ field: f, score });
      }

      // Return highest scoring field
      if (scores.length === 0) return null;
      scores.sort((a, b) => b.score - a.score);
      return scores[0].field;
    },

    //--------------------------------------------------------
    // 3. STATISTICS ENGINE (UPGRADED - handles string numbers)
    //--------------------------------------------------------
    computeStats(rows: any[], classification: any) {
      const stats: any = {};
      for (const [field, type] of Object.entries(classification)) {
        if (type !== "amount" && type !== "count") continue;

        const vals = rows
          .map((r) => {
            const v = r[field];
            if (typeof v === "number") return v;
            if (typeof v === "string" && !isNaN(Number(v))) return Number(v);
            return null;
          })
          .filter((v) => v !== null)
          .sort((a, b) => a - b);

        if (vals.length < 10) continue;

        const p = (pct: number) => vals[Math.floor(vals.length * pct)];
        const mean = vals.reduce((a, b) => a + b, 0) / vals.length;
        const sd = Math.sqrt(
          vals.reduce((a, b) => a + Math.pow(b - mean, 2), 0) / vals.length
        );

        stats[field] = {
          p05: p(0.05),
          p10: p(0.10),
          p85: p(0.85),
          p90: p(0.90),
          p95: p(0.95),
          p99: p(0.99),
          p999: p(0.999),
          mean,
          sd,
          min: vals[0],
          max: vals[vals.length - 1],
        };
      }
      return stats;
    },

    //--------------------------------------------------------
    // 4. SAFE GETTERS
    //--------------------------------------------------------
    _getNumber(row: any, field: string) {
      const v = row[field];
      return typeof v === "number" && !isNaN(v) ? v : 0;
    },

    _getTimestamp(row: any, field: string) {
      const v = row[field];
      const t = Date.parse(v);
      return isNaN(t) ? null : new Date(t);
    },

    // ======================================================
    //   ADVANCED FRAUD DETECTION METHODS
    // ======================================================

    //--------------------------------------------------------
    // VELOCITY DETECTION - Rapid-fire transactions
    //--------------------------------------------------------
    /**
     * Detect entities with unusually high transaction velocity.
     * Returns rules for flagging rapid transaction patterns.
     */
    buildVelocityRules(
      rows: any[], 
      semanticMap: Record<string, string[]>, 
      classification: any
    ): any[] {
      const rules: any[] = [];
      
      // Find timestamp field
      const timestampFields = Object.keys(classification).filter(f => classification[f] === 'timestamp');
      if (timestampFields.length === 0) return rules;
      
      const timestampField = timestampFields[0];
      
      // Find entity fields (employee, customer, register)
      const entityCategories = ['employee_id', 'customer_id', 'register_id', 'store_id'];
      const entityFields: string[] = [];
      
      for (const cat of entityCategories) {
        const fields = this.getSemanticFields(semanticMap, cat);
        entityFields.push(...fields);
      }
      
      if (entityFields.length === 0) return rules;
      
      // Calculate velocity statistics per entity
      const fraudStore = useFraudDetectionStore();
      const { windowMinutes, minTransactions, percentile } = fraudStore.thresholds.velocity;
      const windowMs = windowMinutes * 60 * 1000;
      
      for (const entityField of entityFields) {
        // Group transactions by entity and calculate velocity
        const entityGroups = new Map<string, Date[]>();
        
        for (const row of rows) {
          const entityId = String(row[entityField] || '');
          const ts = this._getTimestamp(row, timestampField);
          if (!entityId || !ts) continue;
          
          if (!entityGroups.has(entityId)) {
            entityGroups.set(entityId, []);
          }
          entityGroups.get(entityId)!.push(ts);
        }
        
        // Calculate max velocity for each entity
        const velocities: number[] = [];
        
        for (const [entityId, timestamps] of entityGroups.entries()) {
          if (timestamps.length < minTransactions) continue;
          
          timestamps.sort((a, b) => a.getTime() - b.getTime());
          
          // Sliding window to find max transaction count
          let maxCount = 0;
          for (let i = 0; i < timestamps.length; i++) {
            const windowEnd = timestamps[i].getTime() + windowMs;
            let count = 0;
            for (let j = i; j < timestamps.length && timestamps[j].getTime() <= windowEnd; j++) {
              count++;
            }
            maxCount = Math.max(maxCount, count);
          }
          
          if (maxCount >= minTransactions) {
            velocities.push(maxCount);
          }
        }
        
        if (velocities.length < 10) continue;
        
        // Calculate percentile threshold
        velocities.sort((a, b) => a - b);
        const threshold = velocities[Math.floor(velocities.length * percentile)];
        
        if (threshold >= minTransactions) {
          rules.push({
            fraudType: "High Transaction Velocity",
            fields: [entityField, timestampField],
            condition: `row["_velocity_${entityField}"] > ${threshold}`,
            severity: "high",
            description: `More than ${threshold} transactions within ${windowMinutes} minutes`,
            velocityField: entityField,
            velocityThreshold: threshold,
            velocityWindow: windowMinutes
          });
        }
      }
      
      return rules;
    },

    //--------------------------------------------------------
    // TEMPORAL PATTERN DETECTION - Off-hours transactions
    //--------------------------------------------------------
    /**
     * Detect transactions during unusual hours.
     */
    buildTemporalRules(
      rows: any[], 
      classification: any
    ): any[] {
      const rules: any[] = [];
      
      // Find timestamp field
      const timestampFields = Object.keys(classification).filter(f => classification[f] === 'timestamp');
      if (timestampFields.length === 0) return rules;
      
      const timestampField = timestampFields[0];
      const fraudStore = useFraudDetectionStore();
      const { offHoursStart, offHoursEnd } = fraudStore.thresholds.temporal;
      
      // Check if dataset has off-hours transactions
      let offHoursCount = 0;
      let totalWithTime = 0;
      
      for (const row of rows.slice(0, 2000)) {
        const ts = this._getTimestamp(row, timestampField);
        if (!ts) continue;
        
        totalWithTime++;
        const hour = ts.getHours();
        
        if (hour >= offHoursStart || hour < offHoursEnd) {
          offHoursCount++;
        }
      }
      
      // Only add rule if off-hours transactions exist but are unusual (< 20%)
      const offHoursRatio = totalWithTime > 0 ? offHoursCount / totalWithTime : 0;
      
      if (offHoursRatio > 0 && offHoursRatio < 0.2) {
        rules.push({
          fraudType: "Off-Hours Transaction",
          fields: [timestampField],
          condition: `(function(ts) { 
            const d = new Date(ts); 
            const h = d.getHours(); 
            return h >= ${offHoursStart} || h < ${offHoursEnd}; 
          })(row["${timestampField}"])`,
          severity: "medium",
          description: `Transaction during off-hours (${offHoursStart}:00 - ${offHoursEnd}:00)`,
          temporalField: timestampField
        });
      }
      
      // Weekend high-value transactions
      const amountFields = Object.keys(classification).filter(f => classification[f] === 'amount');
      if (amountFields.length > 0) {
        rules.push({
          fraudType: "Weekend High-Value Transaction",
          fields: [timestampField, amountFields[0]],
          condition: `(function(ts) { 
            const d = new Date(ts); 
            return d.getDay() === 0 || d.getDay() === 6; 
          })(row["${timestampField}"]) && row["${amountFields[0]}_total"] > row["_p95_${amountFields[0]}"]`,
          severity: "low",
          description: "High-value transaction on weekend",
          temporalField: timestampField
        });
      }
      
      return rules;
    },

    //--------------------------------------------------------
    // SPLIT TRANSACTION DETECTION
    //--------------------------------------------------------
    /**
     * Detect transactions that appear to avoid approval thresholds.
     */
    buildSplitTransactionRules(
      rows: any[], 
      semanticMap: Record<string, string[]>, 
      classification: any,
      stats: any
    ): any[] {
      const rules: any[] = [];
      
      const amountFields = Object.keys(classification).filter(f => classification[f] === 'amount');
      const timestampFields = Object.keys(classification).filter(f => classification[f] === 'timestamp');
      
      if (amountFields.length === 0 || timestampFields.length === 0) return rules;
      
      const amountField = amountFields[0];
      const timestampField = timestampFields[0];
      const fraudStore = useFraudDetectionStore();
      const { thresholdProximity, commonThresholds } = fraudStore.thresholds.splitTransaction;
      
      // Find entity fields for grouping
      const customerFields = this.getSemanticFields(semanticMap, 'customer_id');
      const entityField = customerFields[0] || this.getSemanticField(semanticMap, 'employee_id');
      
      if (!entityField) return rules;
      
      // Detect transactions just below common thresholds
      for (const threshold of commonThresholds) {
        const lowerBound = threshold * (1 - thresholdProximity);
        const upperBound = threshold * 0.99; // Just below threshold
        
        rules.push({
          fraudType: "Potential Split Transaction",
          fields: [amountField, entityField],
          condition: `row["${amountField}_total"] >= ${lowerBound} && row["${amountField}_total"] < ${upperBound}`,
          severity: "medium",
          description: `Transaction amount just below ${threshold} threshold (${lowerBound.toFixed(2)} - ${upperBound.toFixed(2)})`,
          splitThreshold: threshold
        });
      }
      
      return rules;
    },

    //--------------------------------------------------------
    // SWEETHEARTING DETECTION - Employee-Customer Patterns
    //--------------------------------------------------------
    /**
     * Detect unusual employee-customer relationships (potential collusion).
     */
    buildSweetheartingRules(
      rows: any[], 
      semanticMap: Record<string, string[]>, 
      classification: any,
      stats: any
    ): any[] {
      const rules: any[] = [];
      
      const employeeFields = this.getSemanticFields(semanticMap, 'employee_id');
      const customerFields = this.getSemanticFields(semanticMap, 'customer_id');
      const discountFields = this.getSemanticFields(semanticMap, 'discount_amount');
      
      if (employeeFields.length === 0 || customerFields.length === 0) return rules;
      
      const employeeField = employeeFields[0];
      const customerField = customerFields[0];
      const fraudStore = useFraudDetectionStore();
      const { minOccurrences, discountPercentile } = fraudStore.thresholds.sweethearting;
      
      // Analyze employee-customer pair frequency
      const pairCounts = new Map<string, number>();
      const pairDiscounts = new Map<string, number[]>();
      
      for (const row of rows) {
        const empId = String(row[employeeField] || '');
        const custId = String(row[customerField] || '');
        if (!empId || !custId) continue;
        
        const pairKey = `${empId}::${custId}`;
        pairCounts.set(pairKey, (pairCounts.get(pairKey) || 0) + 1);
        
        // Track discounts for this pair
        if (discountFields.length > 0) {
          const discount = this._getNumber(row, discountFields[0]);
          if (discount > 0) {
            if (!pairDiscounts.has(pairKey)) {
              pairDiscounts.set(pairKey, []);
            }
            pairDiscounts.get(pairKey)!.push(discount);
          }
        }
      }
      
      // Calculate frequency threshold
      const frequencies = Array.from(pairCounts.values()).sort((a, b) => a - b);
      if (frequencies.length < 10) return rules;
      
      const freqThreshold = Math.max(
        minOccurrences,
        frequencies[Math.floor(frequencies.length * 0.95)]
      );
      
      // Add sweethearting rule
      rules.push({
        fraudType: "Potential Sweethearting",
        fields: [employeeField, customerField],
        condition: `row["_pair_frequency_${employeeField}_${customerField}"] >= ${freqThreshold}`,
        severity: "high",
        description: `Employee-customer pair with ${freqThreshold}+ transactions`,
        pairFields: [employeeField, customerField],
        frequencyThreshold: freqThreshold
      });
      
      // If discounts are available, add discount-based rule
      if (discountFields.length > 0) {
        const discountField = discountFields[0];
        const discountStats = stats[discountField];
        
        if (discountStats) {
          const discountThreshold = discountStats[`p${Math.floor(discountPercentile * 100)}`] || discountStats.p90;
          
          rules.push({
            fraudType: "High Discount to Repeat Customer",
            fields: [employeeField, customerField, discountField],
            condition: `row["_pair_frequency_${employeeField}_${customerField}"] >= 2 && row["${discountField}_total"] > ${discountThreshold}`,
            severity: "high",
            description: `Repeat customer receiving above-average discounts`,
            pairFields: [employeeField, customerField]
          });
        }
      }
      
      return rules;
    },

    //--------------------------------------------------------
    // RETURN FRAUD PATTERN DETECTION
    //--------------------------------------------------------
    /**
     * Detect unusual return/refund patterns.
     */
    buildReturnFraudRules(
      rows: any[], 
      semanticMap: Record<string, string[]>, 
      classification: any,
      stats: any
    ): any[] {
      const rules: any[] = [];
      
      const refundAmountFields = this.getSemanticFields(semanticMap, 'refund_amount');
      const refundCountFields = this.getSemanticFields(semanticMap, 'refund_count');
      const amountFields = Object.keys(classification).filter(f => classification[f] === 'amount');
      const customerFields = this.getSemanticFields(semanticMap, 'customer_id');
      
      // Return amount to purchase ratio
      if (refundAmountFields.length > 0 && amountFields.length > 0) {
        const refundField = refundAmountFields[0];
        const amountField = amountFields.find(f => !f.toLowerCase().includes('refund')) || amountFields[0];
        
        const fraudStore = useFraudDetectionStore();
        const { returnAmountRatio } = fraudStore.thresholds.returnFraud;
        
        rules.push({
          fraudType: "High Return Ratio",
          fields: [refundField, amountField],
          condition: `row["${refundField}_total"] > 0 && (row["${refundField}_total"] / Math.max(row["${amountField}_total"], 1)) > ${returnAmountRatio}`,
          severity: "high",
          description: `Returns exceed ${returnAmountRatio * 100}% of purchase amount`
        });
      }
      
      // Frequent returner
      if (refundCountFields.length > 0 && customerFields.length > 0) {
        const fraudStore = useFraudDetectionStore();
        const { frequentReturner } = fraudStore.thresholds.returnFraud;
        
        rules.push({
          fraudType: "Frequent Returner",
          fields: [refundCountFields[0], customerFields[0]],
          condition: `row["${refundCountFields[0]}_total"] >= ${frequentReturner}`,
          severity: "medium",
          description: `${frequentReturner}+ returns detected`
        });
      }
      
      // Return without original purchase (if we can detect)
      // This would require transaction linking which may not be available
      
      return rules;
    },

    //--------------------------------------------------------
    // CROSS-TRANSACTION ANALYSIS
    //--------------------------------------------------------
    /**
     * Analyze patterns across related transactions for sophisticated fraud detection.
     * This method enriches row data with cross-transaction metrics.
     */
    enrichWithCrossTransactionMetrics(
      rows: any[],
      semanticMap: Record<string, string[]>,
      classification: any
    ): Map<string, Record<string, any>> {
      const enrichments = new Map<string, Record<string, any>>();
      
      const timestampFields = Object.keys(classification).filter(f => classification[f] === 'timestamp');
      const employeeFields = this.getSemanticFields(semanticMap, 'employee_id');
      const customerFields = this.getSemanticFields(semanticMap, 'customer_id');
      const amountFields = Object.keys(classification).filter(f => classification[f] === 'amount');
      const discountFields = this.getSemanticFields(semanticMap, 'discount_amount');
      
      const timestampField = timestampFields[0];
      const employeeField = employeeFields[0];
      const customerField = customerFields[0];
      const amountField = amountFields[0];
      const discountField = discountFields[0];
      
      // Build entity aggregations
      const employeeStats = new Map<string, { 
        transactionCount: number; 
        totalAmount: number; 
        totalDiscount: number;
        customers: Set<string>;
        timestamps: Date[];
      }>();
      
      const customerStats = new Map<string, {
        transactionCount: number;
        totalAmount: number;
        employees: Set<string>;
      }>();
      
      const pairStats = new Map<string, number>();
      
      // First pass: collect statistics
      for (const row of rows) {
        const rowId = String(row._id || '');
        const empId = employeeField ? String(row[employeeField] || '') : '';
        const custId = customerField ? String(row[customerField] || '') : '';
        const amount = amountField ? this._getNumber(row, amountField) : 0;
        const discount = discountField ? this._getNumber(row, discountField) : 0;
        const ts = timestampField ? this._getTimestamp(row, timestampField) : null;
        
        // Employee stats
        if (empId) {
          if (!employeeStats.has(empId)) {
            employeeStats.set(empId, {
              transactionCount: 0,
              totalAmount: 0,
              totalDiscount: 0,
              customers: new Set(),
              timestamps: []
            });
          }
          const empStat = employeeStats.get(empId)!;
          empStat.transactionCount++;
          empStat.totalAmount += amount;
          empStat.totalDiscount += discount;
          if (custId) empStat.customers.add(custId);
          if (ts) empStat.timestamps.push(ts);
        }
        
        // Customer stats
        if (custId) {
          if (!customerStats.has(custId)) {
            customerStats.set(custId, {
              transactionCount: 0,
              totalAmount: 0,
              employees: new Set()
            });
          }
          const custStat = customerStats.get(custId)!;
          custStat.transactionCount++;
          custStat.totalAmount += amount;
          if (empId) custStat.employees.add(empId);
        }
        
        // Employee-Customer pair frequency
        if (empId && custId) {
          const pairKey = `${empId}::${custId}`;
          pairStats.set(pairKey, (pairStats.get(pairKey) || 0) + 1);
        }
      }
      
      // Calculate velocity for each employee
      const employeeVelocity = new Map<string, number>();
      const fraudStore = useFraudDetectionStore();
      const { windowMinutes } = fraudStore.thresholds.velocity;
      const windowMs = windowMinutes * 60 * 1000;
      
      for (const [empId, stats] of employeeStats.entries()) {
        if (stats.timestamps.length < 2) continue;
        
        const sorted = stats.timestamps.sort((a, b) => a.getTime() - b.getTime());
        let maxVelocity = 0;
        
        for (let i = 0; i < sorted.length; i++) {
          const windowEnd = sorted[i].getTime() + windowMs;
          let count = 0;
          for (let j = i; j < sorted.length && sorted[j].getTime() <= windowEnd; j++) {
            count++;
          }
          maxVelocity = Math.max(maxVelocity, count);
        }
        
        employeeVelocity.set(empId, maxVelocity);
      }
      
      // Second pass: enrich each row
      for (const row of rows) {
        const rowId = String(row._id || '');
        const empId = employeeField ? String(row[employeeField] || '') : '';
        const custId = customerField ? String(row[customerField] || '') : '';
        
        const metrics: Record<string, any> = {};
        
        // Employee metrics
        if (empId && employeeStats.has(empId)) {
          const empStat = employeeStats.get(empId)!;
          metrics[`_emp_transaction_count`] = empStat.transactionCount;
          metrics[`_emp_total_amount`] = empStat.totalAmount;
          metrics[`_emp_avg_amount`] = empStat.totalAmount / empStat.transactionCount;
          metrics[`_emp_total_discount`] = empStat.totalDiscount;
          metrics[`_emp_discount_rate`] = empStat.totalDiscount / Math.max(empStat.totalAmount, 1);
          metrics[`_emp_unique_customers`] = empStat.customers.size;
          metrics[`_emp_velocity`] = employeeVelocity.get(empId) || 0;
        }
        
        // Customer metrics
        if (custId && customerStats.has(custId)) {
          const custStat = customerStats.get(custId)!;
          metrics[`_cust_transaction_count`] = custStat.transactionCount;
          metrics[`_cust_total_amount`] = custStat.totalAmount;
          metrics[`_cust_avg_amount`] = custStat.totalAmount / custStat.transactionCount;
          metrics[`_cust_unique_employees`] = custStat.employees.size;
        }
        
        // Pair metrics
        if (empId && custId) {
          const pairKey = `${empId}::${custId}`;
          metrics[`_pair_frequency`] = pairStats.get(pairKey) || 0;
          
          // Add named pair frequency for rule conditions
          if (employeeField && customerField) {
            metrics[`_pair_frequency_${employeeField}_${customerField}`] = pairStats.get(pairKey) || 0;
          }
        }
        
        if (Object.keys(metrics).length > 0) {
          enrichments.set(rowId, metrics);
        }
      }
      
      return enrichments;
    },

    /**
     * Build rules that require cross-transaction context.
     */
    buildCrossTransactionRules(
      rows: any[],
      semanticMap: Record<string, string[]>,
      classification: any,
      stats: any
    ): any[] {
      const rules: any[] = [];
      
      const employeeFields = this.getSemanticFields(semanticMap, 'employee_id');
      const customerFields = this.getSemanticFields(semanticMap, 'customer_id');
      
      // Employee with unusually high discount rate
      if (employeeFields.length > 0) {
        rules.push({
          fraudType: "High Discount Rate Employee",
          fields: [employeeFields[0]],
          condition: `row["_emp_discount_rate"] > 0.15`,
          severity: "high",
          description: "Employee gives discounts on >15% of transaction value",
          requiresCrossTransaction: true
        });
        
        // Employee with high velocity
        const fraudStore = useFraudDetectionStore();
        rules.push({
          fraudType: "High Velocity Employee",
          fields: [employeeFields[0]],
          condition: `row["_emp_velocity"] > ${fraudStore.thresholds.velocity.minTransactions}`,
          severity: "medium",
          description: `Employee processes ${fraudStore.thresholds.velocity.minTransactions}+ transactions in ${fraudStore.thresholds.velocity.windowMinutes} minutes`,
          requiresCrossTransaction: true
        });
      }
      
      // Customer concentrating purchases with one employee
      if (customerFields.length > 0 && employeeFields.length > 0) {
        rules.push({
          fraudType: "Customer-Employee Concentration",
          fields: [customerFields[0], employeeFields[0]],
          condition: `row["_cust_unique_employees"] === 1 && row["_cust_transaction_count"] >= 3`,
          severity: "medium",
          description: "Customer only transacts with one employee",
          requiresCrossTransaction: true
        });
      }
      
      return rules;
    },

    // ======================================================
    //   PART 3 — GENERIC + RETAIL/ONLINE FRAUD RULE TEMPLATES
    // ======================================================

    //--------------------------------------------------------
    // COMPREHENSIVE FRAUD RULE TEMPLATES (Generic, Field-Agnostic)
    //--------------------------------------------------------
    buildGenericFraudRuleTemplates(classification: any, stats: any) {
      const fraudStore = useFraudDetectionStore();
      const FRAUD_THRESHOLDS = fraudStore.thresholds;
      
      const fields = Object.keys(classification);
      const amounts = fields.filter(f => classification[f] === "amount");
      const counts = fields.filter(f => classification[f] === "count");

      const rules: any[] = [];

      // ===================================================================
      // 1. HIGH VALUE TRANSACTION FRAUD
      // ===================================================================
      for (const f of amounts) {
        if (!stats[f]) continue;
        const { mean, sd } = stats[f];
        const p95 = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.highValue.percentile * 100)}`] || stats[f].p95;
        const p99 = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.extremeHighValue.percentile * 100)}`] || stats[f].p99;
        
        // Flag top 5% as potentially suspicious high-value (only if above minimum threshold)
        if (p95 > mean + (FRAUD_THRESHOLDS.highValue.stdDevMultiplier * sd) && p95 >= FRAUD_THRESHOLDS.highValue.minimumValue) {
          rules.push({
            fraudType: "High Value Transaction",
            fields: [f],
            condition: `row["${f}_total"] > ${p95}`,
            severity: "medium",
          });
        }
        
        // Flag top 1% as high-risk high-value (only if above minimum threshold)
        if (p99 > p95 * FRAUD_THRESHOLDS.extremeHighValue.multiplier && p99 >= FRAUD_THRESHOLDS.extremeHighValue.minimumValue) {
          rules.push({
            fraudType: "Extreme High Value Transaction",
            fields: [f],
            condition: `row["${f}_total"] > ${p99}`,
            severity: "high",
          });
        }
      }

      // ===================================================================
      // 2. LOW VALUE "PADDING" FRAUD
      // ===================================================================
      for (const f of amounts) {
        if (!stats[f]) continue;
        const { mean, min, p05 } = stats[f];
        
        // Flag suspiciously low transactions - must be in bottom 5th percentile
        // AND below a reasonable threshold (not just any small transaction)
        // Use the 5th percentile from the data, but cap it to avoid false positives
        const p5Value = p05 || (mean * FRAUD_THRESHOLDS.lowValue.meanMultiplier);
        
        // Only flag if the transaction is:
        // 1. Below the 5th percentile of the dataset
        // 2. Very small relative to the mean (less than 5% of mean)
        // 3. But greater than 0 (not free items)
        const lowThreshold = Math.min(
          p5Value,
          mean * 0.05  // Must be less than 5% of mean to be "suspiciously low"
        );
        
        // Only create the rule if threshold is meaningful (> $1 and < reasonable amount)
        if (lowThreshold > 1 && lowThreshold < mean * 0.1) {
          rules.push({
            fraudType: "Suspicious Low Value Transaction",
            fields: [f],
            condition: `row["${f}_total"] > 0 && row["${f}_total"] < ${lowThreshold.toFixed(2)}`,
            severity: "low",
          });
        }
      }

      // ===================================================================
      // 3. HIGH REFUND FRAUD
      // ===================================================================
      const refundFields = fields.filter(f => /refund|return/i.test(f));
      for (const f of refundFields) {
        if (classification[f] === "amount" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.refundAmount.percentile * 100)}`] || stats[f].p95;
          if (threshold > 0) {
            rules.push({
              fraudType: "High Refund Amount",
              fields: [f],
              condition: `row["${f}_total"] > ${threshold}`,
              severity: "high",
            });
          }
        } else if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.refundCount.percentile * 100)}`] || stats[f].p95;
          if (threshold > 0) {
            rules.push({
              fraudType: "Excessive Refund Count",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "high",
            });
          }
        }
      }

      // ===================================================================
      // 4. HIGH VOID FRAUD
      // ===================================================================
      const voidFields = fields.filter(f => /void|cancel|deleted/i.test(f));
      for (const f of voidFields) {
        if (classification[f] === "amount" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.voidAmount.percentile * 100)}`] || stats[f].p95;
          if (threshold > 0) {
            rules.push({
              fraudType: "High Void Amount",
              fields: [f],
              condition: `row["${f}_total"] > ${threshold}`,
              severity: "high",
            });
          }
        } else if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.voidCount.percentile * 100)}`] || stats[f].p90;
          if (threshold > 0) {
            rules.push({
              fraudType: "Excessive Void Count",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "high",
            });
          }
        }
      }

      // ===================================================================
      // 5. BASKET/ITEM MANIPULATION
      // ===================================================================
      const overrideFields = fields.filter(f => /override|manual|modify|adjust/i.test(f));
      for (const f of overrideFields) {
        if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.manualOverride.percentile * 100)}`] || stats[f].p90;
          if (threshold > 0) {
            rules.push({
              fraudType: "Excessive Manual Overrides",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "medium",
            });
          }
        }
      }

      // Item count anomalies
      for (const f of counts) {
        if (!stats[f]) continue;
        const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.itemQuantity.percentile * 100)}`] || stats[f].p95;
        const { max } = stats[f];
        
        if (threshold > 0 && max > threshold * FRAUD_THRESHOLDS.itemQuantity.multiplier) {
          rules.push({
            fraudType: "Unusual Item Quantity",
            fields: [f],
            condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
            severity: "medium",
          });
        }
      }

      // ===================================================================
      // 6. GIFT CARD FRAUD
      // ===================================================================
      const giftCardFields = fields.filter(f => /gift|card|voucher|certificate/i.test(f));
      for (const f of giftCardFields) {
        if (classification[f] === "amount" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.giftCardAmount.percentile * 100)}`] || stats[f].p95;
          if (threshold > 0) {
            rules.push({
              fraudType: "High Gift Card Value",
              fields: [f],
              condition: `row["${f}_total"] > ${threshold}`,
              severity: "medium",
            });
          }
        } else if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.giftCardCount.percentile * 100)}`] || stats[f].p90;
          if (threshold > FRAUD_THRESHOLDS.giftCardCount.minimum) {
            rules.push({
              fraudType: "Multiple Gift Cards Used",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "medium",
            });
          }
        }
      }

      // ===================================================================
      // 7. DISCOUNT ABUSE
      // ===================================================================
      const discountFields = fields.filter(f => /discount|promo|coupon|markdown|staff.*disc/i.test(f));
      for (const f of discountFields) {
        if (classification[f] === "amount" && stats[f]) {
          const { mean } = stats[f];
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.discountAmount.percentile * 100)}`] || stats[f].p90;
          if (threshold > mean * FRAUD_THRESHOLDS.discountAmount.meanMultiplier) {
            rules.push({
              fraudType: "Excessive Discount Amount",
              fields: [f],
              condition: `row["${f}_total"] > ${threshold}`,
              severity: "high",
            });
          }
        } else if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.discountCount.percentile * 100)}`] || stats[f].p90;
          if (threshold > FRAUD_THRESHOLDS.discountCount.minimum) {
            rules.push({
              fraudType: "Multiple Discounts Applied",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "medium",
            });
          }
        }
      }

      // ===================================================================
      // 8. PRICE OVERRIDE FRAUD
      // ===================================================================
      const priceOverrideFields = fields.filter(f => /price.*override|override.*price/i.test(f));
      for (const f of priceOverrideFields) {
        if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.priceOverride.percentile * 100)}`] || stats[f].p85;
          if (threshold > 0) {
            rules.push({
              fraudType: "Excessive Price Overrides",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "high",
            });
          }
        }
      }

      // ===================================================================
      // 9. NEGATIVE VALUES (Data Manipulation)
      // ===================================================================
      for (const f of amounts) {
        if (!stats[f]) continue;
        const { min } = stats[f];
        
        if (min < 0) {
          rules.push({
            fraudType: "Negative Amount Detected",
            fields: [f],
            condition: `row["${f}_total"] < 0`,
            severity: "high",
          });
        }
      }

      // ===================================================================
      // 10. ZERO VALUE TRANSACTIONS
      // ===================================================================
      const totalFields = amounts.filter(f => 
        /total|amount|subtotal|grand/i.test(f) && 
        !(/discount|refund|void|tax|tip/i.test(f))
      );
      for (const f of totalFields) {
        if (stats[f] && stats[f].min >= 0) {
          rules.push({
            fraudType: "Zero Value Transaction",
            fields: [f],
            condition: `row["${f}_total"] === 0`,
            severity: "medium",
          });
        }
      }

      // ===================================================================
      // 11. TENDER SWITCHING / PAYMENT ANOMALIES
      // ===================================================================
      const paymentFields = fields.filter(f => /payment|tender|cash|card/i.test(f));
      for (const f of paymentFields) {
        if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.paymentMethods.percentile * 100)}`] || stats[f].p95;
          if (threshold > FRAUD_THRESHOLDS.paymentMethods.minimum) {
            rules.push({
              fraudType: "Multiple Payment Methods",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "low",
            });
          }
        }
      }

      // ===================================================================
      // 12. LOYALTY / POINTS FRAUD
      // ===================================================================
      const loyaltyFields = fields.filter(f => /loyalty|point|reward/i.test(f));
      for (const f of loyaltyFields) {
        if (classification[f] === "amount" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.loyaltyPoints.percentile * 100)}`] || stats[f].p95;
          if (threshold > 0) {
            rules.push({
              fraudType: "High Loyalty Points Redemption",
              fields: [f],
              condition: `row["${f}_total"] > ${threshold}`,
              severity: "medium",
            });
          }
        }
      }

      // ===================================================================
      // 13. CASH DRAWER / TILL FRAUD
      // ===================================================================
      const noSaleFields = fields.filter(f => /no.*sale|drawer.*open|till.*open/i.test(f));
      for (const f of noSaleFields) {
        if (classification[f] === "count" && stats[f]) {
          const threshold = stats[f][`p${Math.floor(FRAUD_THRESHOLDS.noSaleEvents.percentile * 100)}`] || stats[f].p90;
          if (threshold > FRAUD_THRESHOLDS.noSaleEvents.minimum) {
            rules.push({
              fraudType: "Excessive No-Sale Events",
              fields: [f],
              condition: `row["${f}_total"] > ${Math.ceil(threshold)}`,
              severity: "high",
            });
          }
        }
      }

      return rules;
    },
    // ======================================================
    //   PART 4 — GENERATE FRAUD RULES (NEW GENERIC ENGINE)
    // ======================================================

    async generateFraudRules(rows: any[], mappings?: any[]) {
      // Use cache if available
      const fraudStore = useFraudDetectionStore();
      if (fraudStore.fraudRulesCache.length > 0) {
        return fraudStore.fraudRulesCache;
      }

      if (!rows || rows.length === 0) {
        throw new Error("No sample data provided for fraud rule generation.");
      }

      const sample = rows.slice(0, Math.min(3000, rows.length));
      const fields = Object.keys(sample[0]);

      //------------------------------------------------------
      // 1. CLASSIFY FIELDS (numeric, boolean, timestamp, etc.)
      //------------------------------------------------------
      const classification = this.classifyFields(sample);

      //------------------------------------------------------
      // 2. BUILD SEMANTIC FIELD MAP
      //------------------------------------------------------
      const semanticMap = this.buildSemanticFieldMap(fields, mappings);
      console.log('🏷️ Semantic field map:', semanticMap);

      //------------------------------------------------------
      // 3. DETECT GROUPING KEY (transaction/order/session)
      //------------------------------------------------------
      const groupingKey = this.detectGroupingKeys(sample, classification);

      //------------------------------------------------------
      // 4. STATISTICS ENGINE
      //------------------------------------------------------
      const stats = this.computeStats(sample, classification);

      //------------------------------------------------------
      // 5. BUILD ALL FRAUD RULE TEMPLATES
      //------------------------------------------------------
      let rules: any[] = [];
      
      // 5a. Basic statistical rules (existing)
      const basicRules = this.buildGenericFraudRuleTemplates(classification, stats);
      rules.push(...basicRules);
      console.log(`📊 Generated ${basicRules.length} basic statistical rules`);
      
      // 5b. Velocity detection rules (NEW)
      try {
        const velocityRules = this.buildVelocityRules(sample, semanticMap, classification);
        rules.push(...velocityRules);
        console.log(`⚡ Generated ${velocityRules.length} velocity detection rules`);
      } catch (e) {
        console.warn('Velocity rule generation failed:', e);
      }
      
      // 5c. Temporal pattern rules (NEW)
      try {
        const temporalRules = this.buildTemporalRules(sample, classification);
        rules.push(...temporalRules);
        console.log(`🕐 Generated ${temporalRules.length} temporal pattern rules`);
      } catch (e) {
        console.warn('Temporal rule generation failed:', e);
      }
      
      // 5d. Split transaction rules (NEW)
      try {
        const splitRules = this.buildSplitTransactionRules(sample, semanticMap, classification, stats);
        rules.push(...splitRules);
        console.log(`✂️ Generated ${splitRules.length} split transaction rules`);
      } catch (e) {
        console.warn('Split transaction rule generation failed:', e);
      }
      
      // 5e. Sweethearting/employee fraud rules (NEW)
      try {
        const sweetheartingRules = this.buildSweetheartingRules(sample, semanticMap, classification, stats);
        rules.push(...sweetheartingRules);
        console.log(`💕 Generated ${sweetheartingRules.length} sweethearting detection rules`);
      } catch (e) {
        console.warn('Sweethearting rule generation failed:', e);
      }
      
      // 5f. Return fraud rules (NEW)
      try {
        const returnFraudRules = this.buildReturnFraudRules(sample, semanticMap, classification, stats);
        rules.push(...returnFraudRules);
        console.log(`↩️ Generated ${returnFraudRules.length} return fraud rules`);
      } catch (e) {
        console.warn('Return fraud rule generation failed:', e);
      }
      
      // 5g. Cross-transaction rules (NEW)
      try {
        const crossTxRules = this.buildCrossTransactionRules(sample, semanticMap, classification, stats);
        rules.push(...crossTxRules);
        console.log(`🔗 Generated ${crossTxRules.length} cross-transaction rules`);
      } catch (e) {
        console.warn('Cross-transaction rule generation failed:', e);
      }

      console.log(`🔍 Total fraud rules generated: ${rules.length}`);

      if (!rules.length) {
        throw new Error("No fraud rules could be generated for this dataset.");
      }

      //------------------------------------------------------
      // 6. Attach metadata and return (mark as raw to avoid reactivity)
      //------------------------------------------------------
      const enhancedRules = rules.map(rule => ({
        ...rule,
        groupingKey,
        classification,
        semanticMap,
        stats
      }));

      // markRaw prevents Vue from making this deeply reactive
      fraudStore.fraudRulesCache = markRaw(enhancedRules);
      return enhancedRules;
    },
    // ======================================================
    //   PART 5 — GENERIC FRAUD ANALYSIS ENGINE (ON-DEMAND)
    // ======================================================

    /**
     * Analyze fraud in a specific page/chunk of data.
     * Does NOT cache the data - only stores fraud metadata.
     * @param rows - Current page/chunk of data to analyze
     * @param mappings - Field mappings
     * @param isFirstChunk - Whether this is the first chunk (resets state)
     * @param totalDocumentCount - Total number of documents across all pages
     */
    async analyzeFraudInData(rows: any[], mappings?: any[], isFirstChunk = true, totalDocumentCount?: number) {
      if (!rows || rows.length === 0) return;

      const fraudStore = useFraudDetectionStore();
      
      // Only reset on first chunk
      if (isFirstChunk) {
        fraudStore.loadingFraudAnalysis = true;
        fraudStore.cancelAnalysis = false;
        this.fraudAnalysisResults = [];
        fraudStore.fraudMetadataMap.clear();
        fraudStore.processedCount = 0;
        fraudStore.totalRows = totalDocumentCount || rows.length;
        fraudStore.fraudCount = 0;
        fraudStore._startTimer(performance.now());
      }

      const start = performance.now();

      // ---------------------------------------------------------
      // 1. Load or generate rules
      // ---------------------------------------------------------
      const sample = rows.slice(0, Math.min(3000, rows.length));
      const inferredMappings =
        mappings ||
        Object.keys(rows[0]).map(f => ({
          name: f,
          dataType:
            typeof rows[0][f] === "number"
              ? "number"
              : typeof rows[0][f] === "boolean"
                ? "boolean"
                : typeof rows[0][f] === "string" && !isNaN(Date.parse(rows[0][f]))
                  ? "timestamp"
                  : "string"
        }));

      const rules =
        fraudStore.fraudRulesCache.length > 0
          ? fraudStore.fraudRulesCache
          : await this.generateFraudRules(sample, inferredMappings);

      const classification = rules[0].classification;
      const suggestedGroupingKey = rules[0].groupingKey;
      const semanticMap = rules[0].semanticMap || {};
      
      // ---------------------------------------------------------
      // 2. Smart unique transaction detection (GENERIC)
      // ---------------------------------------------------------
      // Detect fields that identify unique transactions (approval codes, confirmation numbers, etc.)
      const uniqueIdFields = fraudStore.detectUniqueTransactionFields(rows);
      
      // Use detected unique ID fields to build composite key, or fall back to suggested key
      const groupingFields = uniqueIdFields.length > 0 
        ? uniqueIdFields 
        : (suggestedGroupingKey ? [suggestedGroupingKey] : []);
      
      console.log(`🔍 Grouping strategy: ${groupingFields.length > 0 ? 'Composite key from ' + groupingFields.length + ' field(s)' : 'Row index'} - analyzing ${rows.length} rows`);
      if (uniqueIdFields.length > 0) {
        console.log(`🆔 Unique identifier fields: ${uniqueIdFields.join(', ')}`);
      }

      // ---------------------------------------------------------
      // 3. Build cross-transaction enrichments
      // ---------------------------------------------------------
      const crossTxEnrichments = this.enrichWithCrossTransactionMetrics(rows, semanticMap, classification);
      console.log(`🔗 Enriched ${crossTxEnrichments.size} rows with cross-transaction metrics`);

      // ---------------------------------------------------------
      // 4. Build transaction groups with composite keys (UPGRADED)
      // ---------------------------------------------------------
      const groups = new Map();
      
      rows.forEach((r, idx) => {
        // Build composite key from all unique ID fields
        let key: string;
        if (groupingFields.length > 0) {
          // Combine all unique identifier fields into a single key
          key = groupingFields.map(field => String(r[field] || '')).filter(v => v).join('|');
          // If key is empty, fall back to index
          if (!key) key = String(idx);
        } else {
          key = String(idx);
        }
        
        if (!groups.has(key)) groups.set(key, { rows: [], idx: [] });
        groups.get(key).rows.push(r);
        groups.get(key).idx.push(idx);
      });
      
      // Build mapping from composite keys to MongoDB _id values in fraud store
      fraudStore.buildTxIdMapping(rows, groupingFields);

      const transactions = Array.from(groups.entries());
      console.log(`📊 Grouped ${rows.length} rows into ${transactions.length} unique transactions (${((transactions.length / rows.length) * 100).toFixed(1)}% deduplication)`);
      fraudStore.totalRows = rows.length; // Keep as total rows, not transactions
      fraudStore.totalBatches = rules.length;

      // ---------------------------------------------------------
      // 4. Compile rules safely (UPGRADED)
      // ---------------------------------------------------------
      const compiled = rules.map((rule, index) => {
        const fn = new Function(
          "row",
          `try { return (${rule.condition}) } catch { return false }`
        );
        return { ...rule, fn, index: index + 1 };
      });

      const matched = new Set();
      let processedTransactions = 0;
      const totalTransactions = transactions.length;
      
      // Map to accumulate fraud types per transaction: Map<txId, { types: Set, severity: string, fields: object }>
      const transactionFraudMap = new Map();

      // ---------------------------------------------------------
      // 5. Apply rules to each transaction (UPGRADED - Multi-fraud detection)
      // ---------------------------------------------------------
      for (let ruleIdx = 0; ruleIdx < compiled.length; ruleIdx++) {
        if (fraudStore.cancelAnalysis) break;
        
        const rule = compiled[ruleIdx];
        fraudStore.currentBatch = rule.index;
        
        processedTransactions = 0;
        
        for (const [txId, group] of groups.entries()) {
          if (fraudStore.cancelAnalysis) break;

          // Calculate aggregated values for this transaction
          const aggregated: any = {};
          
          // Add cross-transaction enrichments
          const firstRowId = String(group.rows[0]?._id || '');
          const enrichment = crossTxEnrichments.get(firstRowId) || {};
          Object.assign(aggregated, enrichment);
          
          for (const [field, type] of Object.entries(classification)) {
            if (type === "amount" || type === "count") {
              // Only include rows that have actual values for this field
              const validValues = group.rows
                .map((r: any) => {
                  const v = r[field];
                  if (v === null || v === undefined || v === '') return null;
                  if (typeof v === "number") return v;
                  if (typeof v === "string" && !isNaN(Number(v))) return Number(v);
                  return null;
                })
                .filter((v: number | null) => v !== null) as number[];
              
              // Only add to aggregated if there are actual values
              if (validValues.length > 0) {
                aggregated[field + "_total"] = validValues.reduce((a, b) => a + b, 0);
              }
              // If no valid values, don't add the field at all (undefined means "not applicable")
            }
          }

          const rep = group.rows[0];

          // Test the aggregated transaction-level data against the rule
          // Only test if the fields referenced by the rule exist in aggregated data
          let triggered = false;
          
          // Check if this transaction has data for the fields this rule tests
          const hasRelevantData = rule.fields.every((f: string) => 
            aggregated[f + "_total"] !== undefined || rep[f] !== undefined
          );
          
          if (hasRelevantData) {
            for (const r of group.rows) {
              const testRow = { ...r, ...aggregated };
              if (rule.fn(testRow)) {
                triggered = true;
                break;
              }
            }
          }

          if (triggered) {
            // Get or create fraud info for this transaction
            if (!transactionFraudMap.has(txId)) {
              transactionFraudMap.set(txId, {
                types: new Set(),
                severity: rule.severity,
                allFields: {},
                rowIndices: group.idx
              });
              matched.add(txId);
            }
            
            const fraudInfo = transactionFraudMap.get(txId);
            
            // Add this fraud type
            fraudInfo.types.add(rule.fraudType);
            
            // Upgrade severity if this rule is higher (high > medium > low)
            if (rule.severity === "high" || (rule.severity === "medium" && fraudInfo.severity === "low")) {
              fraudInfo.severity = rule.severity;
            }
            
            // Merge relevant fields
            rule.fields.forEach((f: string) => {
              fraudInfo.allFields[f] = rep[f] ?? aggregated[f + "_total"];
            });
          }
          
          processedTransactions++;
        }
        
        // Update fraud count and progress only once per rule (much faster)
        fraudStore.fraudCount = matched.size;
        const overallProgress = (ruleIdx + 1) / compiled.length;
        fraudStore.processedCount = Math.floor(overallProgress * fraudStore.totalRows);
        
        // Allow UI to update only between rules
        await new Promise(res => setTimeout(res, 0));
      }
      
      // ---------------------------------------------------------
      // 5. Store lightweight fraud metadata (MEMORY EFFICIENT)
      // ---------------------------------------------------------
      for (const [txId, fraudInfo] of transactionFraudMap.entries()) {
        const fraudTypes = Array.from(fraudInfo.types) as string[];
        const fraudTypeStr = fraudTypes.join(", ");
        const confidence = Math.min(85 + (fraudTypes.length * 5), 99);
        const explanation: string = fraudTypes.length > 1 
          ? `Multiple fraud indicators: ${fraudTypeStr}` 
          : (fraudTypes[0] || 'Unknown');
        
        // Store in fraud store (it will handle txId → _id conversion)
        fraudStore.storeFraudMetadata(txId, {
          fraudTypes: fraudTypes,
          severity: fraudInfo.severity,
          confidence: confidence,
          explanation: explanation,
          relevantFields: fraudInfo.allFields
        });
      }
      
      // Update progress
      fraudStore.fraudCount = fraudStore.fraudMetadataMap.size;
      fraudStore.processedCount += rows.length;

      // ---------------------------------------------------------
      // 6. Return chunk result
      // ---------------------------------------------------------
      return {
        cancelled: fraudStore.cancelAnalysis,
        chunkFraudCount: matched.size,
        totalFraudCount: fraudStore.fraudMetadataMap.size,
        processedCount: fraudStore.processedCount,
        timeMs: performance.now() - start
      };
    },

    /**
     * Get fraud metadata for a specific document ID
     */
    getFraudMetadata(documentId: string) {
      const fraudStore = useFraudDetectionStore();
      return fraudStore.getFraudMetadata(documentId);
    },

    /**
     * Check if a document is flagged as fraudulent
     */
    isFraudulent(documentId: string): boolean {
      const fraudStore = useFraudDetectionStore();
      return fraudStore.isFraudulent(documentId);
    },

    /**
     * Get all fraudulent document IDs
     */
    getFraudulentIds(): string[] {
      const fraudStore = useFraudDetectionStore();
      return fraudStore.getFraudulentIds();
    }
    ,
    // ======================================================
    //   PART 6 — REMAINING HELPERS & CACHE MANAGEMENT
    // ======================================================

    stopFraudAnalysis() {
      const fraudStore = useFraudDetectionStore();
      fraudStore.stopFraudAnalysis();
      if (this.abortController) {
        this.abortController.abort();
        this.abortController = null;
      }
    },

    clearFraudRulesCache() {
      const fraudStore = useFraudDetectionStore();
      fraudStore.clearFraudRulesCache();
    },

    //------------------------------------------------------
    // RESET ANALYSIS STATE (OPTIONAL UTILITY)
    //------------------------------------------------------
    resetAnalysisState() {
      const fraudStore = useFraudDetectionStore();
      this.fraudAnalysisResults = [];
      fraudStore.resetAnalysisState();
    },

    //------------------------------------------------------
    // TIMER UTILITIES
    //------------------------------------------------------
    _startTimer(start: number) {
      if (this._timer) clearInterval(this._timer);
      this._timer = setInterval(() => {
        const elapsed = performance.now() - start;
        console.log(`⏱️ Elapsed: ${(elapsed / 1000).toFixed(1)}s`);
      }, 1000);
    },

    _stopTimer() {
      if (this._timer) {
        clearInterval(this._timer);
        this._timer = null;
      }
    },

  } // end actions
}); // end defineStore
