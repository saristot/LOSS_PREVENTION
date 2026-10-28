<!-- IMPACT-META
schema: 1
mode: how
step: 18_antipattern_deep_dive
commit: fc7d820908b8b230fb47a2669920ba7efdb413a8
commit_short: fc7d820
branch: main
worktree: dirty
baseline_date: 2026-10-07T18:19:51+02:00
generated_at: 2026-10-08T10:17:43+02:00
-->
# 🔍 ANTIPATTERN ASSESSMENT REPORT

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (local changes only in `Docs/` and `.github/`; application code identical to the commit)

**Date**: 2026-10-08
**Codebase**: `customspa-it-loss-prevention-d098a8b97860` (LOSS_PREVENTION): .NET 8 / FastEndpoints / MongoDB backend (5 projects) + Vue 3 / Vite / Pinia SPA (`LossPrevention.UI`)
**Health Score**: **45 / 100**: 100 − Σ penalties of the 20 findings (see "Health Score Derivation")

## 📊 EXECUTIVE SUMMARY

The backend is tidy at the macro level (recognizable layers, REPR, short functions, no circular dependencies), but it has a "leaky" MongoDB abstraction and an anemic domain. The most serious debt is in the frontend: two god files embed business and security logic, generated and executed dynamically (`eval`, `new Function`). The technical debt is **critical**: 4 critical findings affect data security and the reliability of anti-fraud decisions, although the overall volume remains manageable (≈ 20 targeted interventions).

### Health Score Derivation

Penalty per finding: Critical = 6, High = 4, Medium = 2, Low = 1. No penalty applies to absent patterns (Big Ball of Mud, Circular Dependency).

| Severity | No. of findings | Unit penalty | Total |
|---|---|---|---|
| Critical | 4 | 6 | 24 |
| High | 2 | 4 | 8 |
| Medium | 9 | 2 | 18 |
| Low | 5 | 1 | 5 |
| **Total** | **20** | | **55** |

**Health Score = 100 − 55 = 45 / 100.**

> Version note: v1.0 reported 52/100 without a formula. The value was recalculated with the explicit formula above, on the 20 findings verified in this version.

## 🚨 PRIORITY TABLE (Top Findings)

| File/Module | Anti-Pattern | Severity | Impact |
| :--- | :--- | :--- | :--- |
| **AP-01** `LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs:66-71` | Golden Hammer ("the Mongo pipeline from the client solves everything") / trust boundary violation | Critical | Query injection: reading of other collections via `$lookup`/`$unionWith` (users, tokens, SFTP passwords), DoS; impossible to apply security filters |
| **AP-02** `LossPrevention.UI/src/stores/aiStore.ts` (1,994 lines, 99 functions) | God Object / The Blob + code generation (`new Function`, `:1769`) | Critical | LLM, classification, statistics, rule generation and execution, and UI state in a single store; not testable, not reusable by the backend |
| **AP-03** `LossPrevention.UI/src/components/reports/resultsGrid.vue` (2,271 lines) | God Object + `eval` (code injection, `:1377`) | Critical | Expressions saved in shared workspaces and executed in other users' browsers (persistent XSS with access to the JWT in `localStorage`) |
| **AP-04** `LossPrevention.UI/src/helpers/fieldLock.ts:1-48` | Security by obscurity (client-side enforcement) | Critical | The row-level lock is applied only by the client; modifying the request is enough to read the data of all shops |
| **AP-05** `LossPrevention.Application/Services/Data/Rules/RulesService.cs:34-58, 141-155` | Leaky abstraction + N+1 writes | High | Full scan and `ReplaceOne` per document; flags mass-deleted before recalculation; `UpdateRuleAsync` writes to the wrong path |
| **AP-06** BE `RuleHelper` ↔ FE `aiStore.ts`/`fraudDetectionStore.ts:177` | Stovepipe / split-brain of the anti-fraud logic | High | Two anti-fraud engines with different models; default thresholds defined only in the frontend; results neither reproducible nor auditable on the server side |
| **AP-07** `LossPrevention.Infrastructure/Repositories/MongoRepository.cs:27` | Leaky abstraction (public `Collection`) | Medium | Services and endpoints coupled to the driver; access to other collections bypassing the repository |
| **AP-08** `LossPrevention.Application.csproj` → Infrastructure; `Domain/Entities/*` with 22 `[Bson*]` attributes | Vendor lock-in (MongoDB in the domain) / dependency inversion | Medium | Non-portable domain; tests and persistence changes are costly |
| **AP-09** 20 endpoints (Dashboard, Groups, Notifications, FraudDetection…) | Fat endpoint / layer skipping | Medium | Logic and mapping in the endpoints, duplicated and not testable in isolation |
| **AP-10** `Create/UpdateFraudDetectionSettingsEndpoint.cs` (47-388 ≈ 66-407) | Copy-paste programming | Medium | 341 cloned lines; 5 identical mapping methods of 162 NLOC |
| **AP-11** `Domain/Entities/*`; `AddMemberToGroupEndpoint.cs:56-63` | Anemic Domain Model + Feature Envy | Medium | Invariants scattered across endpoints and helpers; lost update on groups |
| **AP-12** anti-fraud threshold in 8 files (BE + FE) | Shotgun Surgery | Medium | Adding a threshold requires coordinated changes in 2 languages |
| **AP-13** `aiStore.ts:8, 88, 111` | Vendor/runtime lock-in (Ollama) + Golden Hammer regex | Medium | LLM provider and URL hardcoded in the client; fragile field classification |
| **AP-14** `aiStore.ts:1203-1530` (CCN 87); `ReportDataservice.cs:126-150`; `MappingService.cs:269-288` | Spaghetti / Deeply nested code | Medium | 20 functions with CCN > 15; very costly maintenance and testing |
| **AP-15** 31 `catch (Exception)` + 4 empty catches (BE); 115 `catch` / 55 `console.error` (FE) | Error swallowing / generic exceptions | Medium | Hidden errors, "successful" jobs with errors, internal messages exposed to the client |
| **AP-16** `DataIngestionConfiguration.cs:31,34`; `FileProcessingCoordinator.cs:94` | Primitive Obsession | Low | States and types as strings (`"one-time"`, `"daily"`, `"filesystem"`/`"file system"`), 0 enums in the backend |
| **AP-17** `DataIngestionBackgroundService.cs:13,28,100-101`; `MongoRepository.cs:34`; `IndexSuggestionHelper.cs:23` | Magic Numbers | Low | Intervals, limits and samples neither configurable nor documented |
| **AP-18** `DapperRepository.cs`; `IXmlEnrichmentRule`; `XmlProcessingService.cs:43` | Dead code / Speculative generality | Low | Noise, false perception of available features |
| **AP-19** AG Grid + Tabulator; `vue-grid-layout-v3` + `grid-layout-plus`; `nuxt`; Newtonsoft + System.Text.Json | Duplicate libraries / unused dependencies | Low | Heavier bundle, wider vulnerability surface |
| **AP-20** `LossPrevention.API/Program.cs:42-47` | Service Locator | Low | `BuildServiceProvider()` during configuration; hidden dependencies |

## 🔍 DETAILED ANALYSIS (Top 3 Critical Problems)

For the detailed analysis, the three critical findings with the greatest combined impact on security and maintainability were chosen (AP-01, AP-02, AP-03). AP-04 is described in the catalog (Appendix B) and is resolved together with AP-01.

### 1. Trust boundary violation / Golden Hammer in `GetReportDataEndpoint.cs`

**Where:** `LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs`, lines 66-71 (`HandleAsync`); execution in `LossPrevention.Application/Services/Data/ReportDataservice.cs:25-86`.

**Analysis:**
The server delegates the construction of the MongoDB query to the client and executes it without validation, converting each stage with `BsonDocument.Parse`. It violates *least privilege* and *never trust the client* (OWASP A03 Injection, CWE-943):
- a user with `CAN_VIEW_REPORT` can use `$lookup`/`$unionWith` to read `Users` (hashes/salts), `PasswordResetTokens` and `DataIngestionConfigurations` (SFTP passwords);
- heavy stages without limits cause DoS;
- the server cannot prepend the user's lock filter, and the cache (key = hash of the request, `:92-97`) does not distinguish between users.

The `$out`/`$merge` write stages do not succeed: the server always appends `$count` or `$skip`/`$limit` (`ReportDataservice.cs:40-52`, with `Take > 0` mandatory at `:45-50` of the endpoint), and `$out`/`$merge` must be the last stage. It is also a *Golden Hammer*: the aggregation pipeline becomes the only query language, even for the interface of a form.

**Current Code (Snippet):**
```csharp
// GetReportDataEndpoint.cs:66-71
var pipeline = req.QueryPipeline
                  .Where(x => x != null)
                  .Select(x => BsonDocument.Parse(x.ToString()))
                  .ToArray();

var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
```

**Proposed Solution:**
The client sends the query model it already has (`Tab.SelectedFields`, `Query.Conditions`, `GroupByField`). A server-side `ReportQueryBuilder`:
1. validates the fields against `Mappings`;
2. allows only known operators;
3. prepends the lock filter read from the claims;
4. enforces `$limit` and `maxTimeMS`.

Stages such as `$lookup`, `$unionWith`, `$out`, `$merge`, `$function`, `$where` and `$accumulator` cannot be generated. The cache key includes user and lock. Migration with a feature flag and comparison of the results on the saved workspaces.

**Refactoring Example:**
```csharp
public sealed record ReportQuery(IReadOnlyList<FieldDTO> Fields, QueryDTO Filter, string? GroupBy, int Skip, int Take);

public BsonDocument[] Build(ReportQuery q, ClaimsPrincipal user, IReadOnlySet<string> knownFields)
{
    var stages = new List<BsonDocument>();
    var lockField = user.FindFirst("LockField")?.Value;
    var lockValue = user.FindFirst("LockValue")?.Value;
    if (lockField is not null && knownFields.Contains(lockField))
        stages.Add(new("$match", new BsonDocument(lockField, lockValue)));
    stages.Add(new("$match", ConditionTranslator.Translate(q.Filter, knownFields))); // whitelist: $eq,$ne,$gt,$gte,$lt,$lte,$in, escaped $regex
    if (q.GroupBy is not null) stages.Add(GroupStage(q, knownFields));
    stages.Add(new("$project", Projection(q.Fields, knownFields)));
    return stages.ToArray(); // $skip/$limit and maxTimeMS added by the service
}
```

### 2. God Object / The Blob in `aiStore.ts`

**Where:** `LossPrevention.UI/src/stores/aiStore.ts`: entire file (1,994 lines, 99 functions). Hotspots:
- `buildGenericFraudRuleTemplates`, lines 1203-1530, CCN 87;
- `enrichWithCrossTransactionMetrics`, lines 986-1142, CCN 43;
- `analyzeFraudInData`, lines 1667-1922, CCN 34;
- compilation of the rules with `new Function`, lines 1768-1774.

**Analysis:**
The store violates SRP: it contains the LLM client (`:8`, `:88`), NLQ, semantic classification of fields via regex (`SEMANTIC_PATTERNS`, `:111`), descriptive statistics, anti-fraud rule generation, compilation and execution, UI state and timers.

The rules are JavaScript strings built by interpolating the **field names** of the data (`row["${f}_total"]`) and compiled with `new Function`:
- they are neither typed nor testable;
- they are not reusable by the backend;
- a crafted field name in the ingested files (e.g. containing `"]`) becomes code executed in the browser (CWE-95).

It also violates KISS and DRY with respect to the backend rule engine (AP-06).

**Current Code (Snippet):**
```ts
// aiStore.ts:1224-1229 (inside buildGenericFraudRuleTemplates)
rules.push({
  fraudType: "High Value Transaction",
  fields: [f],
  condition: `row["${f}_total"] > ${p95}`,
  severity: "medium",
});

// aiStore.ts:1768-1774 (inside analyzeFraudInData)
const compiled = rules.map((rule, index) => {
  const fn = new Function(
    "row",
    `try { return (${rule.condition}) } catch { return false }`
  );
  return { ...rule, fn, index: index + 1 };
});
```

**Proposed Solution:**
1. Introduce a **declarative rule model** (`{ field, op, threshold }`) evaluated by an interpreter with whitelisted operators.
2. Break down the store into pure, testable modules:
   - `llmClient`, behind an interface (see AP-13);
   - `fieldClassifier`;
   - `statistics`;
   - `ruleGenerator`;
   - `ruleEvaluator`.
3. Move `computeStats` and the `build*Rules` functions into a C# service (`FraudAnalysisService`) executed as a job, with persisted and auditable results.
4. Leave in the store only the UI state and the API calls.

Before the refactoring, characterization tests are needed (snapshots of the rules generated on a fixed dataset).

**Refactoring Example:**
```ts
type Op = '>' | '<' | '>=' | '<=' | '==';
type Rule = { fraudType: string; field: string; op: Op; threshold: number; severity: 'low' | 'medium' | 'high' };

const ops: Record<Op, (a: number, b: number) => boolean> = {
  '>': (a, b) => a > b, '<': (a, b) => a < b, '>=': (a, b) => a >= b, '<=': (a, b) => a <= b, '==': (a, b) => a === b,
};

export const evaluate = (row: Record<string, unknown>, r: Rule): boolean => {
  const v = row[`${r.field}_total`];
  return typeof v === 'number' && ops[r.op](v, r.threshold);
};
```

### 3. Code injection via `eval` + God Object in `resultsGrid.vue`

**Where:** `LossPrevention.UI/src/components/reports/resultsGrid.vue`, lines 1360-1382 (`valueGetter` of the calculated fields; `eval` at line 1377), in a component of 2,271 lines. The component also manages the grid, export, drill-down, lock (`:1687`) and anti-fraud analysis.

**Analysis:**
The expression of the calculated field is written by a user and saved in the workspace, which can be shared. When another user opens the report, `eval` executes it in their browser with access to the JWT in `localStorage` (`loginStore.ts:92`): it is a persistent XSS (CWE-95).

The replace at line 1371 substitutes only the identifiers that match keys of the row and lets everything else through, for example `fetch(...)` or `localStorage`. The size of the file violates SRP: every change to the grid risks regressions on unrelated functions.

**Current Code (Snippet):**
```ts
// resultsGrid.vue:1371-1377
const expr = expression.replace(/\b(\w+)\b/g, (m: any) => {
  if (Object.prototype.hasOwnProperty.call(row, m)) {
    return row[m] === undefined || row[m] === null ? '""' : `row["${m}"]`;
  }
  return m;
});
const result = eval(expr);
```

**Proposed Solution:**
- Replace `eval` with a whitelisted arithmetic expression parser (e.g. `expr-eval` with logical operators, assignment and function definition disabled, or `jsep` + interpreter), compiling once per field.
- Validate the expressions also on the server side when the workspace is saved.
- Break down `resultsGrid.vue` into:
  - `useGridColumns` (composable);
  - `useCalculatedFields`;
  - `useExport`;
  - `FraudOverlay`, a component that consumes the backend results (AP-02).
- Add the ESLint rule `no-eval`.

**Refactoring Example:**
```ts
import { Parser } from 'expr-eval';

const parser = new Parser({ operators: { logical: false, assignment: false, fndef: false } });
const compiled = parser.parse(expression);                  // once per calculated field
valueGetter: (params) => {
  const vars = numericFieldsOf(params.data);                // known numeric variables only
  const result = compiled.evaluate(vars);
  return `${field.prefix ?? ''}${result.toFixed(2)}${field.suffix ?? ''}`;
}
```

## 🛠️ GENERAL RECOMMENDATIONS & NEXT STEPS

- [ ] **Week 1**: eliminate `eval` (AP-03) and `new Function` (AP-02, declarative interpreter); block the arbitrary pipeline with a provisional whitelist of stages (AP-01).
- [ ] **Months 1-2**: server-side Query DSL with lock applied by the server (AP-01, AP-04); single anti-fraud engine in the backend with persisted thresholds and audit (AP-06, AP-12); rule engine with bulk updates (AP-05).
- [ ] Write characterization tests, then break down `aiStore.ts` and `resultsGrid.vue` (AP-02, AP-03, AP-14).
- [ ] Close the repository's `Collection` behind explicit methods (`UpdateManyAsync`, `BulkWriteAsync`, `AggregateAsync`) and move the logic of the 20 endpoints into the services (AP-07, AP-09); single threshold mapping (AP-10).
- [ ] Enums/value objects for `ScheduleType`, `Recurrence`, `SourceType` and `FileType`; constants/configuration for intervals and limits (AP-16, AP-17).
- [ ] Error handling: `IExceptionHandler` + `ProblemDetails`, no `ex.Message` to the client, specific catches (AP-15).
- [ ] Remove dead code and duplicate or unused libraries: Dapper, `IXmlEnrichmentRule`, `grid-layout-plus`, `nuxt`, one of the two grids (AP-18, AP-19); service locator in `Program.cs` (AP-20).
- [ ] Introduce a backend-side `LlmProvider` adapter in place of the direct call to Ollama from the browser (AP-13).
- [ ] **Tools to integrate into CI**:
  - SonarQube/SonarCloud (C# + TS, quality gate on duplication < 5 % and new critical issues = 0);
  - ESLint `plugin:vue/vue3-recommended` with `no-eval`, `no-new-func` and `no-implied-eval`;
  - Roslyn analyzers with `TreatWarningsAsErrors` on new projects;
  - ArchUnitNET (or NetArchTest) to forbid `IMongoRepository<>` and `MongoDB.Driver` in the endpoints and `MongoDB.Bson` in the Domain;
  - jscpd (duplication threshold);
  - gitleaks (secret scanning).

---

## Appendix A — Analysis checklist by phase

Outcome of each check required by the prompt (Phases 1-4): ✅ = anti-pattern absent, ⚠️ = partially present, ❌ = present. Each item reports the evidence and the reference finding.

### Phase 1 — Initial scan and metrics

| Check | Result |
|---|---|
| Technology stack | Backend: .NET 8, ASP.NET Core + FastEndpoints 6, MongoDB.Driver 3.4, FluentValidation, SSH.NET. Frontend: Vue 3 + Vite + Pinia + Vuetify, AG Grid, Tabulator, Chart.js, axios; local Ollama LLM (`qwen2.5:14b`) called from the browser |
| Directory structure | `LossPrevention.API` (Endpoints by feature), `LossPrevention.Application` (Services, DTO, Handlers, Helpers, Validators), `LossPrevention.Domain` (Entities, Exceptions), `LossPrevention.Infrastructure` (Repositories, DI), `LossPrevention.DataIngestionService` (console), `LossPrevention.UI/src` (components, stores, helpers, api, router) |
| Source files | 232 C# + 41 Vue + 28 TS (in `LossPrevention.UI/src`) |
| Non-blank lines | Backend 10,760; frontend 15,836 |
| Classes / functions | Backend 264 classes (0 enums), 475 functions; frontend 569 functions (lizard) |
| God files (> 500 lines) | `resultsGrid.vue` 2,271 · `aiStore.ts` 1,994 · `selectFields.vue` 823 · `fraudDetectionStore.ts` 728 · `queryBuilderTabs.vue` 694 · `FraudSettingsDialog.vue` 595 · `dataingestion.vue` 567 · `notifications.vue` 562; no C# file exceeds 500 lines (max `MappingService.cs` 428, `FileProcessingCoordinator.cs` 424, `UpdateFraudDetectionSettingsEndpoint.cs` 407) |
| Functions with CCN > 15 | 20 (6 BE + 14 FE); max 87 (`aiStore.ts:1203-1530`) |
| Duplication (jscpd) | BE 8.58 % (56 clones, 1,052 lines); FE see [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |

### Phase 2 — Architectural analysis (macro-level)

**2.1 Big Ball of Mud & Spaghetti Architecture**
- ✅ Chaotic structure without layers: **absent**. Recognizable API/Application/Domain/Infrastructure layers; frontend organized into components/stores/helpers.
- ✅ Circular dependencies between main modules: **absent**.
  - The graph of the .NET projects is acyclic (API → Application/Infrastructure, Application → Domain/Infrastructure, Infrastructure → Domain).
  - Between the Pinia stores there are only unidirectional dependencies (`aiStore` → `fraudDetectionStore`, `notificationStore` → `groupStore`).
  - A complete analysis of import cycles between Vue components (e.g. `madge`) was not performed.
- ⚠️ Non-linear flows: the ingestion → mapping → rules chain is partly synchronous (`FileProcessingCoordinator.cs:105-108`) and partly manual (`GET /rules/apply`). The statistical analysis takes place in the browser. See AP-05 and AP-06.

**2.2 Stovepipe System (Silos)**
- ❌ Duplicated instead of shared logic:
  - two anti-fraud engines (min/max rules in the backend, statistics and generated rules in the frontend);
  - two ingestion pipelines (API `FileProcessingCoordinator` and console `DataIngestionService/Program.cs:85-102`) with different logic (batch vs single insert, rules ignored). See AP-06.
- ⚠️ Point-to-point integration: the frontend calls Ollama directly on `http://localhost:11434` (`aiStore.ts:88`) without going through the backend (AP-13); for SFTP and SMTP the direct calls are adequate for the scale of the system.

**2.3 Golden Hammer**
- ❌ The MongoDB aggregation pipeline sent by the client is used as a universal query language (AP-01).
- ❌ Semantic classification of fields based on regexes on the names (`SEMANTIC_PATTERNS`, `aiStore.ts:111`) and generation of JavaScript code as a rule engine (AP-02, AP-13).

**2.4 Vendor Lock-in**
- ❌ MongoDB in the domain: entities with 22 `[Bson*]` attributes and the `ObjectId` type (e.g. `User.cs:1,7`); endpoints that use the driver's `Builders<T>` (e.g. `ListNotificationsEndpoint.cs:30`). See AP-08.
- ❌ Hardcoded Ollama LLM (model `aiStore.ts:8`, URL `:88`), without an adapter (AP-13).
- ✅ Cloud: no dependency on proprietary cloud services in the code.

### Phase 3 — Design pattern analysis (class/module level)

**3.1 God Object / The Blob**
- ❌ `aiStore.ts` (99 functions, 1,994 lines) and `resultsGrid.vue` (2,271 lines) exceed all thresholds (AP-02, AP-03).
- ⚠️ `fraudDetectionStore.ts` (29 functions, 728 lines) and `selectFields.vue` (823 lines).
- ⚠️ Backend: `MappingService` (428 lines, schema discovery + CRUD + batch) and `FileProcessingCoordinator` (424 lines: orchestration, SFTP, parsing, persistence, idempotency) are borderline; no class has more than 20 public methods.
- ⚠️ Generic names: `BsonHelper`, `RuleHelper` and `DistanceHelper` are static helpers. `IndexSuggestionHelper.cs` contains the `IndexService` class.

**3.2 Circular Dependency**
- ✅ Absent, see 2.1.

**3.3 Shotgun Surgery**
- ❌ Adding or modifying an anti-fraud threshold requires coordinated changes in **8 files**, in C# and TypeScript (AP-12):
  - `FraudDetectionSettings.cs:29`;
  - `FraudDetectionSettingsDTO.cs:15`;
  - `CreateFraudDetectionSettingsEndpoint.cs` (24 occurrences);
  - `UpdateFraudDetectionSettingsEndpoint.cs` (24);
  - `GetFraudDetectionSettingsEndpoint.cs` (12);
  - `fraudDetectionStore.ts:10,177`;
  - `FraudSettingsDialog.vue:417`;
  - `aiStore.ts:1219`.

**3.4 Feature Envy**
- ❌ `AddMemberToGroupEndpoint.cs:56-63` directly manipulates `GroupDocument.Members` and `UpdatedAtUtc` (logic that belongs to `Group.AddMember`).
- ❌ `RuleHelper.EvaluateRule(doc, rule)` reads the fields of `RuleConfiguration` (behavior that belongs to the rule).
- ❌ `BsonHelper` works entirely on the data of `BsonValue`/`MappingItem`.
- See AP-11.

**3.5 Anemic Domain Model**
- ❌ 19 entities with properties only and 0 behavior methods; 2 domain exceptions never used (`DeleteUserRoleException`, `InvalidCustomerException`); states as strings (AP-11, AP-16).

### Phase 4 — Code analysis (implementation level)

**4.1 Complexity and readability**
- ❌ Spaghetti code / long methods:
  - 18 backend functions > 50 NLOC, of which 5 mapping methods of 162 NLOC;
  - `buildGenericFraudRuleTemplates` with 254 NLOC and CCN 87;
  - `DistanceDataService.GetDistanceAsync` with 122 NLOC and CCN 20 (AP-14).
- ❌ Magic numbers/strings:
  - intervals (`FromMinutes(1)`, `FromSeconds(10)`, 50 s);
  - limits `.Take(20)`, sample 100, `ProcessMappings(1000)`/`int.MaxValue`, 1 h cache;
  - strings `"sftp"`/`"filesystem"`/`"file system"` (AP-16, AP-17).
- ❌ Deeply nested code: `ReportDataservice.cs:117-150` (8 levels: if → foreach → if → if → foreach → else if → if → for); `MappingService.cs:269-288` (async lambda in `Parallel.ForEachAsync` → foreach → if → if) (AP-14).

**4.2 DRY violations**
- ❌ Copy-paste programming:
  - FraudDetection Create/Update (341 lines);
  - `MapThresholdsToDTO` repeated in 3 endpoints;
  - 56 backend clones in total (AP-10).

**4.3 Error handling**
- ❌ Error swallowing:
  - 4 empty untyped catches (`BsonHelper.cs:221,280`, `XmlToBsonConverterHelper.cs:149`, `DashboardMapping.cs:89`);
  - SFTP errors recorded in the result but with the job completed (`SftpFileProcessingService.cs:107-122`);
  - frontend: 115 `catch`, 55 `console.error`, no centralized notification.
- ❌ Generic exceptions: 31 `catch (Exception)` in the backend; `ex.Message` returned to the client in 13 places (AP-15).

**4.4 Primitive Obsession**
- ❌ `ScheduleType`, `Recurrence`, sources and file types as strings; thresholds and lock as primitives (`LockField`/`LockValue` strings in `User.cs:18-19`); widespread `any` in the frontend (`buildGenericFraudRuleTemplates(classification: any, stats: any)`, `aiStore.ts:1203`) (AP-16).

---

## Appendix B — Complete catalog of findings

Each finding reports the verified location (file:line at commit `fc7d820`), evidence, impact, severity and recommended refactoring. AP-01…AP-03 are detailed in the "Detailed analysis" section.

### AP-04 — Security by obscurity: lock applied only by the client · **Critical**
- **Location**:
  - `LossPrevention.UI/src/helpers/fieldLock.ts:1-48`;
  - consumers in `resultsGrid.vue:1687`, `toolbar.vue:180`, `tabularBlock.vue:259` (the latter with empty mappings `[]`);
  - claims issued in `LoginEndpoint.cs:65-69` and never read by the backend.
- **Evidence**:
  ```ts
  // fieldLock.ts:2-8
  const token = localStorage.getItem('token');
  if (!token) return undefined;
  const payload = JSON.parse(atob(token.split('.')[1]));
  if (!payload.LockField || payload.LockValue == null) return undefined;
  ```
- **Impact**: the filter by shop/area exists only in the query built by the browser. A user can remove it (DevTools or direct call to the API) and read all the data (CWE-602, OWASP API1 BOLA).
- **Refactoring**: apply the lock server-side as the first `$match` in the Query DSL (AP-01), in distance and export; keep `fieldLock.ts` only for UX (showing the locked filter).

### AP-05 — Leaky abstraction + N+1 writes in the rule engine · **High**
- **Location**: `LossPrevention.Application/Services/Data/Rules/RulesService.cs:34-58` (`ApplyRulesAsync`), `:141-148` (`UpdateRuleAsync`).
- **Evidence**:
  ```csharp
  var docs = (await _reportDataRepository.GetAllAsync()).ToList();                    // :34 full scan
  await _reportDataRepository.Collection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, unsetUpdate); // :41 removes ALL flags
  await _reportDataRepository.Collection.ReplaceOneAsync(filter, doc);                // :58 one write per document
  var docUpdate = Builders<BsonDocument>.Update.Set($"{rule.RuleName}", isMatch);     // :147 wrong path (root, not FraudFlags.*)
  ```
- **Impact**:
  - O(N) memory and time with N round-trips;
  - during the recalculation all documents remain without flags (the unset is not atomic with respect to the rewrite);
  - after `UpdateRuleAsync` the flags are inconsistent.
- **Refactoring**: one `UpdateMany` with an update pipeline per rule (`$set: { "FraudFlags.<rule>": <$and/$gte/$lte expression> }`), execution as an asynchronous post-ingestion job, and a repository with explicit methods instead of `Collection`.

### AP-06 — Stovepipe / split-brain of the anti-fraud logic · **High**
- **Location**:
  - BE: `RuleHelper.EvaluateRule` (used in `RulesService.cs:50`), console `DataIngestionService/Program.cs:38-47` (rules loaded and ignored), `:85-102`;
  - FE: `aiStore.ts:1203-1530` and `fraudDetectionStore.ts:177`.
- **Evidence**:
  ```ts
  // fraudDetectionStore.ts:177-181 — single source of the threshold defaults
  const DEFAULT_FRAUD_THRESHOLDS: FraudThresholdConfig = {
    highValue: {
      percentile: 0.95,
      stdDevMultiplier: 2,
      minimumValue: 500,
  ```
- **Impact**:
  - anti-fraud decisions depend on the browser and on the loaded page of data, so they are neither reproducible, auditable nor executable in batch;
  - backend and frontend use different rule models;
  - two divergent ingestion pipelines.
- **Refactoring**:
  - a single `FraudAnalysisService` in the backend, with persisted thresholds (server-side defaults) and results saved with the rule version;
  - the frontend only displays;
  - the console reuses `FileProcessingCoordinator`.

### AP-07 — Leaky abstraction: public `Collection` in the repository · **Medium**
- **Location**: `LossPrevention.Infrastructure/Repositories/MongoRepository.cs:27`; usages in `RulesService.cs:41,58,148` and `FileProcessingCoordinator.cs:390-392,408-410`.
- **Evidence**:
  ```csharp
  public IMongoCollection<TDocument> Collection => _collection;          // MongoRepository.cs:27
  await _reportDataRepository.Collection.Database
      .GetCollection<BsonDocument>("ProcessedFiles")                      // FileProcessingCoordinator.cs:390-391
  ```
- **Impact**: the repository encapsulates nothing. The services use the driver directly and reach other collections (`ProcessedFiles`, not configured), making mocks and persistence changes impossible.
- **Refactoring**: make `Collection` `internal`/private; add typed `UpdateManyAsync`, `BulkWriteAsync` and `AggregateAsync`; create a dedicated `IProcessedFileRepository`.

### AP-08 — MongoDB vendor lock-in in the domain / dependency inversion · **Medium**
- **Location**: `LossPrevention.Application.csproj` (ProjectReference → Infrastructure); `LossPrevention.Domain/Entities/**` (22 `[Bson*]` attributes, `ObjectId` in `User.cs:1,7,16`); `Builders<T>` in the endpoints (e.g. `ListNotificationsEndpoint.cs:30`).
- **Evidence**:
  ```csharp
  using MongoDB.Bson;                    // User.cs:1
  public ObjectId _id { get; set; }      // User.cs:7
  ```
- **Impact**: the domain depends on the driver. Application depends on Infrastructure instead of the other way around, so testing and replacing the persistence cost more.
- **Refactoring**:
  - move the repository interfaces into Application/Domain and invert the reference;
  - use `string` ids in the domain and mapping with `BsonClassMap` in Infrastructure;
  - ArchUnitNET rule that forbids `MongoDB.*` in the Domain and in the endpoints.

### AP-09 — Fat endpoint / layer skipping · **Medium**
- **Location**: 20 endpoints that inject `IMongoRepository<T>`, e.g. `ListNotificationsEndpoint.cs:30`, `CreateGroupEndpoint.cs:34`, `CreateDashboardEndpoint.cs:35`, `CreateFraudDetectionSettingsEndpoint.cs:44`.
- **Evidence**:
  ```csharp
  var filter = Builders<NotificationDocument>.Filter.Empty;   // ListNotificationsEndpoint.cs:30 — query, no recipient filter
  ```
- **Impact**: business and authorization logic in the endpoints, not reusable and not testable in isolation. In the example, the missing filter by recipient is a security bug.
- **Refactoring**: application services for Groups, Notifications, Dashboard and FraudSettings; the endpoints are limited to binding, validation and HTTP mapping.

### AP-10 — Copy-paste programming · **Medium**
- **Location**:
  - `CreateFraudDetectionSettingsEndpoint.cs:47-388` ≈ `UpdateFraudDetectionSettingsEndpoint.cs:66-407` (341 lines, jscpd);
  - `MapThresholdsToDTO` in Create `:226-387`, Get `:60-221`, Update `:245-406`;
  - `MapToEntity` in Create `:50-211` and Update `:69-230`.
- **Evidence**: 5 methods of 162 NLOC each, with CCN 1 (field-by-field mapping).
- **Impact**: every new threshold must be added 5 times (feeds AP-12); silent divergences between create and update.
- **Refactoring**: a single mapper (e.g. Mapperly, source generator, or a static `FraudSettingsMapper`) used by the 3 endpoints; thresholds as a typed dictionary or value object.

### AP-11 — Anemic Domain Model + Feature Envy · **Medium**
- **Location**: `LossPrevention.Domain/Entities/**` (19 entities without methods); `AddMemberToGroupEndpoint.cs:56-63`.
- **Evidence**:
  ```csharp
  if (!existing.Members.Contains(userOid))
  {
      existing.Members.Add(userOid);
      existing.UpdatedAtUtc = DateTime.UtcNow;
      var updateDefinition = Builders<GroupDocument>.Update
          .Set(g => g.Members, existing.Members)       // rewrites the whole list
  ```
- **Impact**: invariants scattered across endpoints and helpers. The read-modify-write without optimistic concurrency causes lost updates with concurrent requests.
- **Refactoring**: `Group.AddMember(userId)` in the domain; persistence with atomic `$addToSet`; `Rule.Evaluate(doc)` instead of `RuleHelper.EvaluateRule`.

### AP-12 — Shotgun Surgery on the anti-fraud thresholds · **Medium**
- **Location**: 8 files (see Phase 3.3): `FraudDetectionSettings.cs:29`, `FraudDetectionSettingsDTO.cs:15`, 3 FraudDetection endpoints, `fraudDetectionStore.ts:10,177`, `FraudSettingsDialog.vue:417`, `aiStore.ts:1219`.
- **Evidence**: `highValue`/`HighValue` appears in 8 files and 75 lines across C# and TypeScript.
- **Impact**: slow and risky changes; easy to forget one of the places (e.g. the default exists only in the frontend).
- **Refactoring**: data-driven threshold schema (list of `ThresholdDefinition` with key, type and default) served by the backend; UI generated from the schema; single mapper (AP-10).

### AP-13 — Ollama vendor/runtime lock-in + Golden Hammer regex · **Medium**
- **Location**: `LossPrevention.UI/src/stores/aiStore.ts:8` (model), `:88` (URL), `:111` (`SEMANTIC_PATTERNS`).
- **Evidence**:
  ```ts
  const MODEL = "qwen2.5:14b";                                      // :8
  const res = await fetch("http://localhost:11434/api/generate", {  // :88
  export const SEMANTIC_PATTERNS: Record<string, RegExp[]> = {      // :111
  ```
- **Impact**:
  - every workstation requires a local Ollama;
  - changing provider or model requires a frontend release;
  - no access control, quota or logging on LLM requests;
  - the classification of fields depends on the names used by the POS.
- **Refactoring**: backend endpoint `/ai/*` behind a configurable `ILlmProvider` interface (Ollama, Azure OpenAI…); classification based on the `Mappings` (declared types) with regexes only as a fallback.

### AP-14 — Spaghetti / deeply nested code · **Medium**
- **Location**:
  - `aiStore.ts:1203-1530` (CCN 87), `:986-1142` (CCN 43), `:1667-1922` (CCN 34);
  - `dataIngestionStore.ts:164-228` (CCN 36);
  - `UpdateDataIngestionConfigurationEndpoint.cs:23-101` (CCN 24);
  - `ReportDataservice.cs:126-150`;
  - `MappingService.cs:269-288`.
- **Evidence**:
  ```csharp
  // ReportDataservice.cs:126-146 — levels 3-8 of 8 (starting from if :117 and foreach :121)
  if (dateFields.Contains(name))
  {
      if (val.IsBsonDocument)
      {
          foreach (var opName in opDoc.Names.ToList())
          {
              if (opName is "$gt" or "$gte" or "$lt" or "$lte" or "$eq" or "$ne") { ... }
              else if (opName is "$in" or "$nin")
              {
                  if (opVal.IsBsonArray)
                  {
                      for (int i = 0; i < arr.Count; i++)
                          if (TryCoerceDateValue(arr[i], out var coerced)) arr[i] = coerced;
  ```
- **Impact**: 20 functions with CCN > 15 are almost impossible to cover with tests and at high risk of regression.
- **Refactoring**: guard clauses and early returns; method extraction (`CoerceOperatorDocument`, `CoerceArray`); strategy tables instead of `if` chains in the rule generator; CCN ≤ 15 limit in the quality gate.

### AP-15 — Error swallowing / generic exceptions · **Medium**
- **Location**:
  - BE: 31 `catch (Exception)`; empty catches in `BsonHelper.cs:221,280`, `XmlToBsonConverterHelper.cs:149`, `DashboardMapping.cs:89`; `SftpFileProcessingService.cs:107-122`; `ex.Message` to the client (e.g. `GetReportDataEndpoint.cs:87`);
  - FE: 115 `catch`, 55 `console.error`.
- **Evidence**:
  ```csharp
  try { result = Convert.ToDecimal(v.AsDouble, CultureInfo.InvariantCulture); return true; }
  catch { /* avoid throw */ }                                   // BsonHelper.cs:220-221
  AddError("query", "Invalid query pipeline: " + ex.Message);  // GetReportDataEndpoint.cs:87
  ```
- **Impact**: masked bugs (conversions failing silently), ingestion jobs "completed" with errors, internal information exposed to the client (CWE-209).
- **Refactoring**: catching specific exceptions (`OverflowException`, `MongoException`, `SshException`); global `IExceptionHandler` + `ProblemDetails`; `PartialSuccess` job outcome with metrics; in the frontend, axios interceptor and centralized notifications.

### AP-16 — Primitive Obsession · **Low**
- **Location**: `DataIngestionConfiguration.cs:31,34`; `FileProcessingCoordinator.cs:94`; `UpdateDataIngestionConfigurationEndpoint.cs:37,45`; `User.cs:18-19`; `aiStore.ts:1203`.
- **Evidence**:
  ```csharp
  public string ScheduleType { get; set; } = "one-time"; // "one-time" | "recurring"   (:31)
  public string Recurrence { get; set; } = "daily";      // "daily" | "weekly"          (:34) — but "monthly" is handled in DataIngestionBackgroundService.cs:140
  if (normalizedSource == "filesystem" || normalizedSource == "file system")          // FileProcessingCoordinator.cs:94
  ```
- **Impact**: unvalidated values (the model comment does not include `monthly`, even though it is handled); fragile comparisons.
- **Refactoring**: `ScheduleType`, `Recurrence` and `SourceType` enums with string serialization (`[BsonRepresentation(BsonType.String)]`/`JsonStringEnumConverter`); `RowLock(Field, Value)` value object; TypeScript types instead of `any`.

### AP-17 — Magic Numbers · **Low**
- **Location**:
  - `DataIngestionBackgroundService.cs:13` (`FromMinutes(1)`), `:28` (`FromSeconds(10)`), `:100-101` (50 s);
  - `MongoRepository.cs:34` (`.Take(20)`);
  - `IndexSuggestionHelper.cs:23` (sample 100);
  - `RulesService.cs:78` (`ProcessMappings(1000)`);
  - `GetReportDataEndpoint.cs:81` (1 h cache).
- **Evidence**:
  ```csharp
  var now = DateTime.Now;                                                    // :97 local time
  if (config.LastRunAt.HasValue && (now - config.LastRunAt.Value).TotalSeconds < 50)  // :100-101, LastRunAt set in UTC (:86) and never persisted (comment :87)
  ```
- **Impact**: non-configurable behavior. In the example, the magic number combines with a comparison between local time and UTC that makes the anti-double-execution guard unreliable on servers with a time zone other than UTC.
- **Refactoring**: `IngestionSchedulerOptions`, `IndexingOptions` and `ReportCacheOptions` via `IOptions<T>`; `TimeProvider`/`DateTime.UtcNow` everywhere.

### AP-18 — Dead code / Speculative generality · **Low**
- **Location**:
  - `LossPrevention.Infrastructure/Repositories/DapperRepository.cs` (205 commented-out lines);
  - `IXmlEnrichmentRule` (0 implementations);
  - `XmlProcessingService.cs:40-44`;
  - `GetReportDataEndpoint.cs:64` (unused `GetAllMappings()`);
  - `DataIngestionService/Program.cs:24-25` (unused constants);
  - domain exceptions never thrown;
  - empty `Endpoints\Login\` folder in the csproj.
- **Evidence**:
  ```csharp
  public Task InsertManyAsync(IEnumerable<BsonDocument> docs)
  {
      // This method is deprecated - FileProcessingCoordinator handles insertion now
      throw new NotImplementedException("Use FileProcessingCoordinator.RunIngestionAsync instead");   // XmlProcessingService.cs:43
  }
  ```
- **Impact**: noise and false perception of available features. The interface promises a method that throws an exception (violates LSP).
- **Refactoring**: removal (the history remains in Git); separate the parsing interface from the persistence interface.

### AP-19 — Duplicate libraries / unused dependencies · **Low**
- **Location**: `LossPrevention.UI/package.json`:
  - `ag-grid-community`/`ag-grid-vue3` (`:13-14`, used in `resultsGrid.vue:504` and `tabularBlock.vue:44`) **and** `tabulator-tables` (`:26`, used only in `distanceTable.vue:25`);
  - `vue-grid-layout-v3` (`:28`, used in `dashboardGrid.vue:84`) **and** `grid-layout-plus` (`:18`, never imported);
  - `nuxt` (`:22`) and `@nuxt/devtools` (`:35`), never used in a Vite project.

  Backend: `Newtonsoft.Json` (3 files) and `System.Text.Json` (4 files).
- **Evidence**: grep of the imports in `LossPrevention.UI/src`: no import of `grid-layout-plus` or of `nuxt`.
- **Impact**: heavier bundle; wider CVE surface (`nuxt` brings with it many vulnerable transitive dependencies, see doc 16); two APIs to know for the same task.
- **Refactoring**: remove `grid-layout-plus`, `nuxt` and `@nuxt/devtools`; migrate `distanceTable.vue` to AG Grid; consolidate the backend on System.Text.Json.

### AP-20 — Service Locator in `Program.cs` · **Low**
- **Location**: `LossPrevention.API/Program.cs:42-47`.
- **Evidence**:
  ```csharp
  var tempServices = new ServiceCollection();
  tempServices.AddInfrastructureServices(builder.Configuration);
  tempServices.AddRepositoryServiceCollection(builder.Configuration);
  using var tempProvider = tempServices.BuildServiceProvider();
  var settings = tempProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
  ```
- **Impact**: a second DI container (with possible duplicate singletons) built only to read a configuration section; the comment at line 41 ("Load rules from MongoDB") does not match the code.
- **Refactoring**: `builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()`, or `IConfigureNamedOptions<JwtBearerOptions>`.

---

## Appendix C — Method

- **Tools**:
  - `lizard` (CCN, NLOC, functions) on C#, TS and Vue;
  - `jscpd` (duplication);
  - line and file counting with PowerShell;
  - grep/ripgrep for patterns (`eval`, `new Function`, `catch`, `Collection`, `Builders<`, imports);
  - manual reading of the cited files.

  All line numbers are verified at commit `fc7d820`.
- **Limitations**:
  - complete analysis of import cycles between Vue components not performed;
  - Maintainability Index not calculated: N/A — not derivable from the code without Roslyn/SonarQube analyzers.
- **Historical vendor documentation**: present only in commit `593f6de` and removed in `d768cd9` (`git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`, from `C:\repository\LOSS_PREVENTION`). Not used as evidence: all findings derive from the code.

---

## Reference Documents

- [00_deep_dive.md](00_deep_dive.md)
- [01_context.md](01_context.md)
- [02_functional_overview.md](02_functional_overview.md)
- [03_non_functional_overview.md](03_non_functional_overview.md)
- [04_constraints.md](04_constraints.md)
- [05_principles.md](05_principles.md)
- [06_software_architecture.md](06_software_architecture.md)
- [07_code.md](07_code.md)
- [08_data.md](08_data.md)
- [09_infrastructure_architecture.md](09_infrastructure_architecture.md)
- [10_deployment.md](10_deployment.md)
- [11_development_environment.md](11_development_environment.md)
- [12_operation_and_support.md](12_operation_and_support.md)
- [13_decision_log.md](13_decision_log.md)
- [14_metrics.md](14_metrics.md)
- [15_fp_cocomo.md](15_fp_cocomo.md)
- [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md)
- [17_backend_deep_assessment.md](17_backend_deep_assessment.md)
- [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md)

## Change Log

| Version | Date | Author | Changes |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Document creation (replaces run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Health Score recalculated with explicit formula (52 → 45/100, 20 findings: 4 Critical, 2 High, 9 Medium, 5 Low); priority table with verified IDs and file:line; detailed analyses with exact lines (`eval` `resultsGrid.vue:1377`, `new Function` `aiStore.ts:1769`, pipeline `GetReportDataEndpoint.cs:66-71`) and risk of injection via field names; Phases 1-4 checklist with outcome and evidence; catalog AP-04…AP-20 with snippets, impact and refactoring; corrections: TS files 29 → 28, `queryBuilderTabs.vue` 693 → 694 and `dataingestion.vue` 566 → 567 lines; new findings: non-atomic mass unset of flags, local time/UTC comparison in the scheduler, lost update on groups, unused `grid-layout-plus`/`nuxt`; complete references |
