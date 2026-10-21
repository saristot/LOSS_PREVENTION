<!-- IMPACT-META
schema: 1
mode: how
step: 03_non_functional_overview
commit: fc7d820908b8b230fb47a2669920ba7efdb413a8
commit_short: fc7d820
branch: main
worktree: dirty
baseline_date: 2026-10-07T18:19:51+02:00
generated_at: 2026-10-08T09:38:00+02:00
-->
# Non-Functional Overview - LOSS_PREVENTION Project

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (local changes only in `Docs/` and `.github/`; application code identical to the commit)

**Date**: 2026-10-08  
**Version**: 1.1  
**Authors**: IMPACT REVERSE how (static code analysis + IMPACT verification)  
**Audience**: Software architects, tech leads, development team, QA

---

### Method and conventions

- The repository **contains no formalized non-functional requirements**: no SLA, benchmark, load test, telemetry or monitoring configuration.
- Each requirement reports:
  - **AS-IS**: behavior observed in the code at commit `fc7d820`, with `file:riga` evidence;
  - **TARGET SMART**: proposed requirement, measurable and with a time horizon. The targets are **proposals to be validated** with the client; the horizon "R1" denotes the first modernization release (see [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md));
  - **Priority**: Critical / High / Medium / Low.
- **[NOT ASSESSABLE]** marks the requirements for which the code and the repository history provide no explicit elements (rule of the IMPACT 03 prompt).
- The vendor documentation (`Docs/roadmap.txt`, `Docs/09_SECURITY_DOCUMENTATION.md`, etc.) **does not exist at the baseline**: it was present only in commit `593f6de` and was removed in `d768cd9`. It can be consulted with `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`.

---

## 1. MANIFEST Service Level Requirements

Requirements directly perceived by the user: performance, usability, reliability and availability. The code shows several structural bottlenecks (full collection scans, computation in the browser), but no actual measurements.

### 1.1 Performance

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-PERF-00 | **[NOT ASSESSABLE]** Actual response times (P50/P95): no benchmark, load test or telemetry in the repository | Define a baseline with load tests before R1 | High |
| NFR-PERF-01 | Report query: the aggregation runs **2 times** per request (`$count` + page, `ReportDataservice.cs:40-52`); 1 h in-memory cache (`GetReportDataEndpoint.cs:81`); no upper limit on `Take` (only `Take > 0`, line 45; the heatmap requests 100,000 rows, `components/reports/toolbar.vue:193`) | P95 `POST /data/report/query` < 3 s with 1 M documents and `Take ≤ 1.000`, indexes on the filtered fields; measured with load tests in R1 | Critical |
| NFR-PERF-02 | `ApplyRulesAsync`: `GetAllAsync()` on the whole `ReportData` (`RulesService.cs:34`) + one `ReplaceOneAsync` per document (line 58) | Rule application on 1 M documents < 10 min and process memory < 1 GB (bulk write / pipeline update), by R1 | High |
| NFR-PERF-03 | `GetDistanceAsync`: loads into memory all documents in the interval (`DistanceDataservice.cs:39`) and all the mappings; cost O(N·F) | P95 `POST /distance` < 5 s with 100 k documents in the interval, by R1 | High |
| NFR-PERF-04 | Statistical analysis in the browser: one `POST /data/report/query` per page + JS evaluation (`resultsGrid.vue:1175-1203`) | Server-side analysis of 1 M transactions < 15 min, asynchronous with progress reporting, by R1 | High |
| NFR-PERF-05 | Ingestion: sequential SFTP file by file, in two passes (download + re-read from `processed/`, `FileProcessingCoordinator.cs:236-331`); console with `Parallel.ForEachAsync` and `InsertOneAsync` per document (`DataIngestionService/Program.cs:85, 102`) | ≥ 50,000 transactions/min per instance with batch inserts, measured on a real dataset | Medium |
| NFR-PERF-06 | Indexes on `ReportData`: only `_id` and `ttl_BeginDateTime` (`Data/LossPrevention/ReportData.metadata.json`). `IndexService.ProcessIndexesAsync` (`Services/Data/IndexSuggestionHelper.cs:21-39`) creates an index for **every** field of the first 100 documents, but it is invoked only by the console (`DataIngestionService/Program.cs:117`) | Indexes managed by code/migration on the recurring filter and sort fields; ≤ 15 indexes per collection; `explain()` check of the main queries | High |

### 1.2 Usability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-USA-01 | English only, strings hardcoded in the 41 SFCs; no i18n library in `package.json` | 100% of labels externalized and IT/EN translation by R1 | Medium |
| NFR-USA-02 | Accessibility not addressed: only 1 `aria-` attribute (`App.vue:19`) and only 1 `alt=` in `src/`; no a11y tests | WCAG 2.1 AA on the 6 main screens (Login, Home, Query, Dashboard, Rules, Users); 0 "serious" violations with axe-core in CI | Medium |
| NFR-USA-03 | Feedback via toast/snackbar; errors of the scheduled ingestion are visible only in the logs; in the manual run the API responds 500 and the UI shows a generic message (`dataingestion.vue:458-472`) | Ingestion run history in the UI (outcome, files, records, errors) for ≥ 30 days | High |
| NFR-USA-04 | Automatic logout at token expiry (1 h, `loginStore.ts:54-62`) with no advance warning and no refresh token | Warning 5 min before expiry and silent session renewal | Low |
| NFR-USA-05 | **[NOT ASSESSABLE]** Mobile/tablet use: Vuetify is responsive, but there are no requirements or tests on devices | — | Low |
| NFR-USA-06 | Debug left in the code: 57 `console.log` in `src/` (e.g. every `computed` of the menu in `App.vue:314-369`) | 0 `console.log` in the production build (lint rule) | Low |

### 1.3 Reliability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-REL-00 | **[NOT ASSESSABLE]** MTBF / error rate: no operational data, persistent log or monitoring | Measure the 5xx error rate after the introduction of structured logs | Medium |
| NFR-REL-01 | `ApplyRulesAsync` runs `$unset FraudFlags` on all documents (`RulesService.cs:40-41`) and then rewrites document by document: an error halfway through leaves the flags inconsistent | Idempotent batch operation with persisted execution state; resumption after an error without loss of flags | High |
| NFR-REL-02 | Ingestion: `InsertMany` and `MarkFileAsProcessed` not transactional (`FileProcessingCoordinator.cs:374-377`); dedup by file name only; no Mongo session/transaction in the code (0 occurrences of `StartSession`) | Exactly-once per file (content hash + state) verified by integration tests | High |
| NFR-REL-03 | Scheduler: `LastRunAt` set but not saved (`DataIngestionBackgroundService.cs:86-87`); `one-time` schedule never executed (line 55); `ManualLoad` flag never read | Persistent scheduler with state and history; 100% of the configured schedules executed or reported | High |
| NFR-REL-04 | 31 `catch (Exception …)` blocks in the backend, many return `ex.Message` to the client (e.g. `GetReportDataEndpoint.cs:85-88`); no retry policy (0 references to Polly/Retry) | Centralized error handling (ProblemDetails) with correlation-id; retry with backoff toward Mongo/SFTP/SMTP | Medium |
| NFR-REL-05 | `POST /data/create-transactions` responds "Inserted" but does not persist (`CreateTransactionEndpoint.cs:49-53`: only `ProcessAsync`, no insert); `XmlProcessingService.InsertManyAsync` throws `NotImplementedException` (line 43) | 0 endpoints that report success without effect, verified by tests | High |
| NFR-REL-06 | Data consistency: the 1 h cache is not invalidated after ingestion or `Apply Rules` (key = SHA-256 of pipeline+skip+take, `GetReportDataEndpoint.cs:92-98`) | Data visible within 1 min of ingestion/rules (explicit invalidation) | Medium |

### 1.4 Availability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-AV-00 | **[NOT ASSESSABLE]** SLA %: no agreement or objective in the repository | Proposal: 99.5% monthly during business hours, to be validated | High |
| NFR-AV-01 | Single instance: `IMemoryCache` (`Program.cs:49`) and in-process scheduler (`Program.cs:111`) prevent a safe scale-out (duplicate jobs). The historical vendor roadmap (`roadmap.txt`, commit `593f6de`) stated "Load balancing - SOA - At least 2 applications for uptime" | ≥ 2 API instances behind a load balancer with single-leader scheduler and distributed cache, by R1 | High |
| NFR-AV-02 | **[NOT ASSESSABLE]** RPO/RTO: no backup/restore strategy in the code; there is only a development mongodump in `Data/LossPrevention` | Proposal: RPO ≤ 24 h, RTO ≤ 4 h with restore tested every quarter | High |
| NFR-AV-03 | Failover: none. No health check (`AddHealthChecks` absent in `Program.cs`); the DB is initialized synchronously at startup (`Program.cs:115-119`): if Mongo is not reachable the API does not start | `/health/live` and `/health/ready` endpoints; automatic restart managed by the platform | High |

---

## 2. OPERATIONAL Service Level Requirements

Operational requirements: throughput, serviceability, testability, manageability and security. Security is the area with the most gaps and is covered row by row.

### 2.1 Throughput

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-THR-01 | **[NOT ASSESSABLE]** Expected volume (transactions/day, concurrent users): no data. The development dump contains ≈ 3,000 `ReportData` documents | Collect the real volumes from the client (the historical roadmap reported "Real world data … Waiting for Custom") | High |
| NFR-THR-02 | No rate limiting (`AddRateLimiter` absent), no explicit limit on the body (the Kestrel default of ~30 MB applies) | Login ≤ 5 attempts/min per IP; API ≤ 100 requests/min per user; `Take` ≤ 1,000 | High |

### 2.2 Serviceability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-SRV-01 | Logs only with the default console provider (`appsettings.json` → `Logging:LogLevel:Default = Information`); `ILogger<T>` used in 4 classes (`DatabaseInitializationService`, `DataIngestionBackgroundService`, `FileProcessingCoordinator`, `SftpFileProcessingService`); 6 `Console.WriteLine` | Structured JSON logs with correlation-id on 100% of requests; retention ≥ 30 days | High |
| NFR-SRV-02 | No health check, metric or tracing | Health check + metrics (P95 latency, errors, ingestion duration) exposed by R1 | High |

### 2.3 Testability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-TST-01 | 0 .NET test projects; no `test` script in `package.json` (only `dev/build/preview`); no CI | ≥ 60% line coverage on the Application services and ≥ 80% on `RuleHelper` / `DistanceHelper` by R1; build + test on every PR | Critical |
| NFR-TST-02 | Business logic in the browser (`aiStore.ts` 1,994 lines, `analyzeFraudInData` at line 1667) and rules compiled with `new Function` (line 1769): not testable server-side | Server-side statistical engine with deterministic unit tests | High |
| NFR-TST-03 | 20 endpoints use `IMongoRepository<…>` directly, so integration tests with a real Mongo are needed | Endpoints without direct repository access (verifiable with ArchUnitNET) | Medium |

### 2.4 Manageability

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-MNG-01 | A single `appsettings.json` (no `appsettings.{Environment}.json`); CORS hardcoded to `localhost:5173/5174` (`Program.cs:24-34`); LLM URL hardcoded (`aiStore.ts:88`); API base URL from `VITE_API_BASE_URL`, with no versioned `.env` file | Per-environment configuration (Dev/UAT/Prod); 0 hardcoded URLs or hosts in the code | High |
| NFR-MNG-02 | Seed and migrations with manual `mongosh` scripts ordered `00..04` (`Data/MongoDBScripts`) | Versioned migrations executed automatically at deploy | Medium |

### 2.5 Security

| ID | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|------------------|-----------------------|----------|
| NFR-SEC-01 | Arbitrary aggregation pipeline from the client (`GetReportDataRequest.QueryPipeline: List<object>`, `BsonDocument.Parse` in `GetReportDataEndpoint.cs:66-69`) | Whitelist of stages/operators; tests that reject `$lookup`, `$out`, `$merge`, `$unionWith`, `$function`, `$where` | Critical |
| NFR-SEC-02 | Row-level lock (`LockField/LockValue`) applied only in the browser (`fieldLock.ts`); the cache is not per user (`GetReportDataEndpoint.cs:92-98`) | Filter enforced server-side on 100% of the data endpoints; cache key including identity/lock | Critical |
| NFR-SEC-03 | `PasswordHash`/`PasswordSalt` returned by `GetUsersEndpoint.cs:36`, `GetUserByIdEndpoint.cs:56-57`, `GetUserByUsernameEndpoint.cs:55-56` | 0 credential fields in the API responses (contract test) | Critical |
| NFR-SEC-04 | `JwtSettings.SecretKey` in clear text in `appsettings.json`; symmetric HMAC-SHA256 signature (`Program.cs:63-76`) | 0 secrets in the repository (secret scanning in CI); key from a secret store with rotation | Critical |
| NFR-SEC-05 | No `UseHttpsRedirection`/HSTS; SMTP with `EnableSsl = false` (`EmailService.cs:64`); SFTP with password only, without host key verification | TLS 1.2+ end-to-end; SMTP STARTTLS; SFTP with key and host key pinning | High |
| NFR-SEC-06 | ✅ PBKDF2-SHA256 passwords with 600,000 iterations and upgrade of the legacy hashes from 10,000 (`PasswordHasher.cs:9-10, 21`; `UserService.cs:205-213`); reset tokens hashed with SHA-256 and valid for 1 h | Keep; add lockout after 5 failed attempts | Medium |
| NFR-SEC-07 | `GET /notifications` returns all notifications (`ListNotificationsEndpoint.cs:30-31`) | Each user sees only the notifications addressed to them, to their group or role | High |
| NFR-SEC-08 | **[NOT ASSESSABLE]** Audit log: absent in the code. The historical vendor documentation (`09_SECURITY_DOCUMENTATION.md:596`, commit `593f6de`) declared it "✅" | Audit of logins, changes to rules/thresholds/users and analyses performed, retained ≥ 1 year | High |

---

## 3. DEVELOPMENT Service Level Requirements

Requirements on the realizability and planability of development. The stack is mainstream, but all quality and reproducible-build tools are missing.

| ID | Category | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|-----------|------------------|-----------------------|----------|
| NFR-DEV-01 | Realizability | Mainstream stack (.NET 8, FastEndpoints 6, MongoDB.Driver 3.4, Vue 3.5, Vuetify 3.7, Vite 6): skills easily available. .NET 8 is LTS with end of support 10/11/2026 | Migration to .NET 10 LTS before 10/11/2026 | High |
| NFR-DEV-02 | Realizability | Obsolete package `Microsoft.AspNetCore.Http.Features 5.0.17`; `nuxt ^3.0.0` dependency present but unused; `xlsx ^0.18.5` (latest version published on npm) | 0 deprecated or unused packages; dependency audit in CI | Medium |
| NFR-DEV-03 | Planability | No backlog, estimate or milestone in the repository. The historical vendor roadmap (`roadmap.txt`, only commit `593f6de`) stated "Within 3 Months Time frame" and "Initial Roadmap - 3-6 Months", without per-activity estimates | Estimated backlog (see [15_fp_cocomo.md](15_fp_cocomo.md) and [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md)) | Medium |
| NFR-DEV-04 | Build reproducibility | No CI/CD pipeline or Dockerfile; `package.json` with `^` ranges and `package-lock.json` **not aligned** (`npm ci` fails); no `test`/`lint`/`type-check` script | Deterministic CI build (`npm ci`, `dotnet build`) green on every PR | High |
| NFR-DEV-05 | Build reproducibility | 12 imports whose upper/lower case does not match the file name (e.g. `@/stores/workspaceStore` vs `workspacestore.ts` in 6 files) and one import unresolvable on every OS (`ScheduleForm.vue:27` → `@/store/useDataIngestionStore`) | Green build on Linux; 0 imports with wrong case | High |

---

## 4. EVOLUTIONARY Service Level Requirements

Requirements on evolution over time. The schema-on-read model gives ingestion flexibility; the concentration of logic in a few very large files limits its maintainability.

| ID | Category | AS-IS (evidence) | Proposed TARGET SMART | Priority |
|----|-----------|------------------|-----------------------|----------|
| NFR-EVO-01 | Scalability | Schema-less data + Mongo aggregations: they scale on the DB side. Anti-fraud logic in the browser and in-process scheduler/cache: they do not scale | Server-side anti-fraud computation; stateless API horizontally scalable (≥ 2 instances) | High |
| NFR-EVO-02 | Extensibility | Strategy for file formats (`IFileProcessingService`: XML, CSV, JSON registered in `Program.cs:102-104`); `IXmlEnrichmentRule` planned but **with no registered implementations** | New format addable with 1 class + 1 registration, without changes to the coordinator | Low |
| NFR-EVO-03 | Maintainability | Hotspots: `buildGenericFraudRuleTemplates` CCN 87 / 254 NLOC (`aiStore.ts:1203-1530`, measured with lizard); `resultsGrid.vue` 2,271 lines; `aiStore.ts` 1,994 lines; FraudDetection endpoints 388/407 lines with duplicated mapping | No function with CCN > 15; no file > 500 lines in the modules touched by R1 | High |
| NFR-EVO-04 | Flexibility | Mappings generated from the data: new fields without deploy; but rules, saved reports, TTL and heuristics depend on the source field names | Versioned data contract per source format; rules referring to stable aliases | Medium |
| NFR-EVO-05 | Portability | Windows path hardcoded in the console (`C:\xmlstore5\xml`, `DataIngestionService/Program.cs:76`); 12 case-sensitive imports that break the build on Linux/macOS; otherwise cross-platform runtime (.NET 8, Node) | Build and execution on Linux containers, 0 absolute paths in the code | Medium |
| NFR-EVO-06 | Reusability | Generic `MongoRepository<T>` reused by all services, but it exposes `Collection` (`IMongoRepository.cs:170`); statistical logic not reusable from the backend | Repository without exposed `Collection`; rule/statistics engine in a shared library | Medium |

---

## 5. NFR Priority Matrix

The matrix summarizes the priorities and links them to the architectural decisions documented in [06_software_architecture.md](06_software_architecture.md) and [13_decision_log.md](13_decision_log.md).

| Priority | NFR | Architectural driver |
|----------|-----|-----------------------|
| **Critical** | NFR-SEC-01..04, NFR-TST-01, NFR-PERF-01 | Server-side query validation, server-side data filtering, secret management, automated tests |
| **High** | NFR-PERF-00/02/03/04/06, NFR-USA-03, NFR-REL-01/02/03/05, NFR-AV-00..03, NFR-THR-01/02, NFR-SRV-01/02, NFR-TST-02, NFR-MNG-01, NFR-SEC-05/07/08, NFR-DEV-01/04/05, NFR-EVO-01/03 | Computation close to the data, persistent single-leader scheduler, observability, CI |
| **Medium** | NFR-PERF-05, NFR-USA-01/02, NFR-REL-00/04/06, NFR-TST-03, NFR-MNG-02, NFR-SEC-06, NFR-DEV-02/03, NFR-EVO-04/05/06 | i18n/a11y, error handling, migrations, portability |
| **Low** | NFR-USA-04/05/06, NFR-EVO-02 | UX refinements, extensibility already adequate |

### 5.1 Trade-off

| Trade-off | Option A | Option B | Impact / cost |
|-----------|-----------|-----------|-----------------|
| Anti-fraud computation in the browser vs server | Browser (as-is): no server cost | Server: results persisted, auditable, testable | B increases the DB/CPU load, but it is a prerequisite for NFR-SEC-08 and NFR-TST-02 |
| Schema-less vs explicit schema | Schema-on-read (as-is): flexible ingestion | Data contract per format | B requires maintenance of the contracts, but enables indexes, validation and stable rules |
| In-process vs distributed cache | `IMemoryCache` (as-is): minimal latency, zero infrastructure | Redis/distributed cache | B adds a component to be managed; it is required for ≥ 2 instances (NFR-AV-01) |
| Availability 99.5% vs 99.9% | 99.5%: ≈ 3.6 h of downtime/month, 2 instances in a single region | 99.9%: ≈ 43 min/month, requires a replicated DB and zero-downtime deploy | B costs significantly more (replica set, release automation); the value must be validated with the client |
| Local LLM vs centralized gateway | Ollama on every workstation (as-is): no server cost | Server-side LLM gateway | B requires a GPU/managed service, but makes the AI deployable and governable |

---

## 6. Out of Scope

Requirements explicitly excluded or not deducible, not to be considered in the R1 targets unless newly requested.

| Item | Reason |
|----------|--------|
| Multi-region geographic high availability | No requirement or hint in the code; cost disproportionate to the batch usage |
| Real-time latency (< 1 s) on the arrival of transactions | The model is batch (60 s polling, files); the vendor's "real-time" claim is not supported by the code (see [02_functional_overview.md](02_functional_overview.md) §7) |
| Native mobile app | No mobile client in the repository |
| Requirements marked [NOT ASSESSABLE] (NFR-PERF-00, NFR-USA-05, NFR-REL-00, NFR-AV-00, NFR-AV-02, NFR-THR-01, NFR-SEC-08) | Data from the client or from production monitoring is needed before setting a binding target |

---

## Reference Documents

- **Deep Dive Analysis**: [00_deep_dive.md](00_deep_dive.md)
- **Context**: [01_context.md](01_context.md)
- **Functional Overview**: [02_functional_overview.md](02_functional_overview.md)
- Related IMPACT documents: [04_constraints.md](04_constraints.md) · [05_principles.md](05_principles.md) · [06_software_architecture.md](06_software_architecture.md) · [07_code.md](07_code.md) · [08_data.md](08_data.md) · [09_infrastructure_architecture.md](09_infrastructure_architecture.md) · [10_deployment.md](10_deployment.md) · [11_development_environment.md](11_development_environment.md) · [12_operation_and_support.md](12_operation_and_support.md) · [13_decision_log.md](13_decision_log.md) · [14_metrics.md](14_metrics.md) · [15_fp_cocomo.md](15_fp_cocomo.md) · [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) · [17_backend_deep_assessment.md](17_backend_deep_assessment.md) · [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md) · [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md)

---

## Change Log

| Version | Date | Author | Changes |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Document creation (replaces run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Header/baseline `dirty`. Introduced the [NOT ASSESSABLE] markers (measured performance, MTBF, SLA, RPO/RTO, volumes, mobile, audit). Targets rewritten in SMART form, priority on every row, IDs for all NFRs, Operational section split into 5 subsections with Security row by row. Added: trade-offs with costs, link to documents 06/13. Corrected: lockfile "present" → not aligned (`npm ci` fails), plus the 12 case-sensitive imports and the unresolvable import of `ScheduleForm.vue`; `IndexService` (file `IndexSuggestionHelper.cs`) invoked only by the console. Quotations of `roadmap.txt` / `09_SECURITY_DOCUMENTATION.md` marked as historical (`593f6de`, removed in `d768cd9`). Reference Documents complete |
