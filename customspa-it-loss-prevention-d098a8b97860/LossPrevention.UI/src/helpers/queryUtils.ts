// queryutils.ts
import type { ConditionGroup, Condition } from '@/interfaces/tab';

/* ------------------------------- utilities -------------------------------- */
export function normalize(str: string) {
  return str?.toLowerCase();
}

/**
 * Pretty-print any Mongo object/pipeline in Mongo shell syntax.
 * - {$numberDecimal:"..."} -> NumberDecimal("...")
 * - {$date:"..."}          -> ISODate("...")
 */
// helpers/queryUtils.ts
export function toShellSyntax(pipeline: unknown): string {
  // Accept array or object and pretty-print once
  if (pipeline && (Array.isArray(pipeline) || typeof pipeline === 'object')) {
    return JSON.stringify(pipeline, null, 2);
  }
  return String(pipeline ?? '');
}


/* ---------------------------- type helpers (grid) --------------------------- */
const dtOf = (m: any) => (m?.DataType ?? m?.dataType ?? '').toString().trim().toLowerCase();
const isDecimalType = (dt?: string) => !!dt && (dt === 'decimal' || dt === 'decimal128');
const isIntType     = (dt?: string) => !!dt && (dt === 'int32' || dt === 'int64' || dt === 'long' || dt === 'integer');
const isDoubleType  = (dt?: string) => !!dt && (dt === 'double' || dt === 'number' || dt === 'float' || dt === 'numeric');
const isDateType    = (dt?: string) => !!dt && (dt === 'date' || dt === 'datetime' || dt === 'timestamp');

const toDecimalExtended = (v: any) => ({ $numberDecimal: String(v) });
const toDateExtended    = (v: any) => {
  const d = new Date(v);
  return isNaN(d.getTime()) ? v : { $date: d.toISOString() };
};

const COMPARATIVE_OPS = new Set(['$gt', '$gte', '$lt', '$lte']);
const EQUALITY_OPS    = new Set(['$eq', '$ne']);
const ARRAY_OPS       = new Set(['$in', '$nin']);
const VALUE_OPS       = new Set([...COMPARATIVE_OPS, ...EQUALITY_OPS, ...ARRAY_OPS]);

function findMappingByAliasOrName(mappingsStore: { mappings?: any[] }, key?: string) {
  if (!key) return undefined;
  const maps = mappingsStore.mappings || [];
  // prefer alias/name match (case-insensitive)
  return maps.find((m: any) =>
    normalize(m.alias ?? m.Alias) === normalize(key) ||
    normalize(m.name  ?? m.Name ) === normalize(key)
  ) ?? maps.find((m: any) =>
    // legacy fallback: match Name only
    normalize(m.Name) === normalize(key)
  );
}

/* --------------------------- condition builder ----------------------------- */
/**
 * Build ONE leaf condition with type-aware coercion, matching the results grid:
 * - Decimal128: eq/ne => {$numberDecimal:"..."}, ranges use plain numbers
 * - Numbers: coerce to Number where possible
 * - Dates: convert to {$date:"..."}; toShellSyntax prints ISODate("...")
 * - $in/$nin: coerce each element by type (decimals to {$numberDecimal}, dates to {$date})
 */
function buildTypeAwareCondition(
  mappingsStore: { mappings?: any[] },
  c: { field: string; operator: string; value: any }
) {
  const m = findMappingByAliasOrName(mappingsStore, c.field);
  const fieldPath = (m?.name ?? m?.Name ?? c.field) as string;
  const dt = dtOf(m);

  let op = c.operator as string;
  let val: any = c.value;

  // Allow comma-separated strings for $in/$nin
  if (ARRAY_OPS.has(op) && typeof val === 'string') {
    val = val.split(',').map((s: string) => s.trim()).filter((s: string) => s.length > 0);
  }

  // Array operators
  if (ARRAY_OPS.has(op) && Array.isArray(val)) {
    let arr: any[] = val;

    if (isDecimalType(dt)) {
      arr = val.map((x: any) => toDecimalExtended(x));
    } else if (isIntType(dt) || isDoubleType(dt)) {
      arr = val.map((x: any) => {
        const n = Number(x);
        return Number.isNaN(n) ? x : n;
      });
    } else if (isDateType(dt)) {
      arr = val.map((x: any) => toDateExtended(x));
    }

    return { [fieldPath]: { [op]: arr } };
  }

  // Scalar / value operators
  if (VALUE_OPS.has(op)) {
    if (isDateType(dt)) {
      // dates compare against Extended JSON { $date: ISOString }
      val = toDateExtended(val);
    } else if (COMPARATIVE_OPS.has(op)) {
      // for gt/gte/lt/lte we NEVER use NumberDecimal; use plain numbers
      if (isIntType(dt) || isDoubleType(dt) || isDecimalType(dt)) {
        const n = Number(val);
        if (!Number.isNaN(n)) val = n;
      }
    } else if (EQUALITY_OPS.has(op)) {
      if (isDecimalType(dt)) {
        val = toDecimalExtended(val);
      } else if (isIntType(dt) || isDoubleType(dt)) {
        const n = Number(val);
        if (!Number.isNaN(n)) val = n;
      }
    }

    return { [fieldPath]: { [op]: val } };
  }

  // Operators like $exists, $regex, etc. (pass-through)
  return { [fieldPath]: { [op]: val } };
}

/* ------------------------------- main export ------------------------------- */
/**
 * Generates a $match object consistent with the grid implementation + ISODate handling:
 * - Type-aware numeric/decimal handling
 * - Decimal128 equality uses {$numberDecimal:"..."}; ranges use plain numbers
 * - Dates encoded as {$date:"..."} so shell shows ISODate("...") via toShellSyntax
 * - $in/$nin coerce arrays by type
 * - Supports nested groups (AND/OR/NOR) via recursion
 * - Applies optional userLock as an extra equality condition
 */
export function generateSimpleMatch(
  mappingsStore: { mappings?: any[] },
  group: ConditionGroup,
  userLock?: { field: string; value: any }
): any {
  const typeMap: Record<string, '$and' | '$or' | '$nor'> = { AND: '$and', OR: '$or', NOR: '$nor' };
  const logic = typeMap[group?.type || 'AND'] || '$and';

  const parts: any[] = [];

  for (const cond of group?.conditions || []) {
    // Nested group
    if ('type' in cond && (cond as any).type) {
      const nested = generateSimpleMatch(mappingsStore, cond as ConditionGroup, undefined);
      if (nested) parts.push(nested);
      continue;
    }

    // Leaf condition
    const c = cond as Condition;
    if (!('field' in c) || !('operator' in c) || !('value' in c)) continue;

    const built = buildTypeAwareCondition(mappingsStore, {
      field: c.field,
      operator: c.operator,
      value: c.value
    });

    parts.push(built);
  }

  // Apply user lock (simple equality; leave as-is to avoid surprises)
  if (userLock?.field && userLock.value !== undefined) {
    const m = findMappingByAliasOrName(mappingsStore, userLock.field);
    const fieldPath = (m?.name ?? m?.Name ?? userLock.field) as string;
    parts.push({ [fieldPath]: { $eq: userLock.value } });
  }

  if (parts.length === 0) return null;
  if (parts.length === 1) return parts[0];
  return { [logic]: parts };
}
