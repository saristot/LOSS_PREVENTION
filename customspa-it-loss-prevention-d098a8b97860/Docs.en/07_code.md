<!-- IMPACT-META
schema: 1
mode: how
step: 07_code
commit: fc7d820908b8b230fb47a2669920ba7efdb413a8
commit_short: fc7d820
branch: main
worktree: dirty
baseline_date: 2026-10-07T18:19:51+02:00
generated_at: 2026-10-08T09:25:30+02:00
-->
# Code - LOSS_PREVENTION Project

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (local changes only in `Docs/` and `.github/`; application code identical to the commit)

**Date**: 2026-10-08  
**Version**: 1.1  
**Authors**: IMPACT REVERSE how (verification 2026-10-08)  
**Audience**: Technical team, architects, developers

---

## Main Sections

1. [Code Organization & Structure](#1-code-organization--structure)
2. [Key Implementation Patterns](#2-key-implementation-patterns)
3. [Framework Usage (MVC, ORM, DI)](#3-framework-usage-mvc-orm-di)
4. [Security Implementation](#4-security-implementation)
5. [Exception Handling & Logging](#5-exception-handling--logging)
6. [Transaction Management](#6-transaction-management)
7. [Configuration Management](#7-configuration-management)
8. [Testing Strategy](#8-testing-strategy)

Appendices: [A. Complexity hotspots](#a-complexity-hotspots) · [B. Functional defects](#b-functional-defects-identified-in-the-code) · [C. Dead code](#c-dead-code--leftovers)

Conventions: paths relative to `customspa-it-loss-prevention-d098a8b97860/`; `file:N` = line at commit `fc7d820`. Line counts = non-empty lines, unless otherwise stated.

---

## 1. Code Organization & Structure

**Summary.** The backend is organized by project/layer and, within each, by functional area (Users, Data, DataIngestion, Rules, Workspaces…); the frontend by artifact type (components, stores, helpers). Naming and namespace conventions are not uniform.

### 1.1 Backend structure (C# files, excluding `bin/obj`)

| Project | Folder | Files | Content |
|----------|----------|------|-----------|
| API (79 files, 4,347 lines) | `Program.cs` | 1 | Composition root: config, CORS, JWT, DI, pipeline |
| | `Endpoints/` | 78 | One FastEndpoints class per endpoint: Dashboard 5, Data 3, DataIngestion 8, FraudDetection 3, Groups 7, Mappings 4, Notifications 5, Rules 6, User/Permissions 10, User/Roles 10, User/Users 12, Workspaces 5 |
| Application (121 files, 5,005 lines) | `Handlers/` | 52 | Endpoint request/response models |
| | `DTO/` | 21 | Response DTOs |
| | `Services/` | 20 | Application services (Data, Data/Rules, DataIngestion, Users, Workspaces) |
| | `Interfaces/` | 17 | Service interfaces (+ `DatabaseInitializationService.cs`, a concrete class placed here) |
| | `Helpers/` | 6 | `BsonHelper`, `RuleHelper`, `DistanceHelper`, `XmlToBsonConverterHelper`, `JsonHelper`, `MappingHelper` |
| | `Mappings/` | 3 | Static Dashboard/Group/Notification ↔ DTO mappings |
| | `Validators/` | 2 | Dashboard FluentValidation |
| Domain (21 files, 519 lines) | `Entities/` | 19 | POCO entities with BSON attributes |
| | `Exceptions/` | 2 | `DeleteUserRoleException`, `InvalidCustomerException` (never used) |
| Infrastructure (10 files, 784 lines) | root | 1 | `InfrastructureServiceExtensions.cs` (Mongo DI + repositories) |
| | `Configuration/` | 2 | `MongoDbSettings`, `JwtSettings` |
| | `Repositories/` | 4 | `IMongoRepository`, `MongoRepository` (179 lines), `IDapperRepository`, `DapperRepository` (205 lines, entirely commented out) |
| | `Helpers/`, `Services/`, `Interfaces/` | 3 | `PasswordHasher`, `EmailService`, `IEmailService` |
| DataIngestionService (1 file, 105 lines) | `Program.cs` | 1 | Bulk load console |

### 1.2 Frontend structure (`LossPrevention.UI/src`)

| Folder | Files | Notes |
|----------|------|------|
| `components/` | 40 | SFCs by area (`reports/`, `dashboard/`, `manage/`, …); with `App.vue` = 41 SFCs |
| `stores/` | 15 | Pinia stores (e.g. `aiStore.ts` 1,994 lines, `fraudDetectionStore.ts` 728) |
| `helpers/` | 3 | `fieldLock.ts`, `queryUtils.ts`, … |
| `interfaces/` | 5 | TypeScript types |
| `api/`, `plugins/`, `router/` | 1 + 1 + 1 | Axios, Vuetify, vue-router (18 path entries) |

`src` total: 69 `.vue`/`.ts` files, 15,836 non-empty lines.

### 1.3 Observed conventions

| Aspect | Observed | Evidence |
|---------|-----------|----------|
| Namespace ≠ folder/project | `DataIngestionBackgroundService` (Application) → `LossPrevention.Infrastructure.Services`; `XmlEnrichmentService` (`Services/Data`) → `...Services.DataIngestion`; `RuleConfigurationService` in `Services/Data/Rules` → `...Services.Rules`; `MappingHelper` → `FraudDetectionApp.Helpers`; `Workspace`/`Tab`/`Query` (Domain) → `LossPrevention.Application.Entities.Workspaces`; `MongoDbSettings` (`Configuration/`) → `LossPrevention.Infrastructure.Models`; `UpdateDashboardValidator` defined in two namespaces | cited files |
| Naming | `ReportDataservice` / `DistanceDataService` / `IDistanceDataservice`; file `RulesService.cs` contains `RuleConfigurationService`; file `IndexSuggestionHelper.cs` contains `IndexService` | `Services/Data/*` |
| Entity identifiers | Mix of `_id` (Mongo style: `User`, `Role`, `Permission`, `MappingItem`, `RuleConfiguration`) and `Id` (`Workspace`, `DashboardDocument`, `GroupDocument`, …) | `Domain/Entities/**` |
| Project names | csproj with numeric prefix and spaces (`01. LossPrevention.API.csproj`) | project roots |
| Nullable | Enabled, but many entity `string` properties not initialized | `Domain/Entities/**` |
| Async | `async/await` widespread; synchronous SSH.NET calls wrapped in `Task.Run` | `SftpFileProcessingService.cs` |

---

## 2. Key Implementation Patterns

**Summary.** The recurring patterns are: REPR endpoints with in-memory cache, generic repository with direct access to the collection, XML→BSON conversion with type inference, single-field rule engine, Euclidean similarity and a statistical engine generated at runtime in the browser.

### 2.1 FastEndpoints endpoint with cache

```csharp
// LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs (estratto, righe 28-87)
public override void Configure()
{
    Post("/data/report/query");
    Permissions("CAN_VIEW_REPORT");
}
public override async Task HandleAsync(GetReportDataRequest req, CancellationToken ct)
{
    var cacheKey = GenerateCacheKey(req);                                  // :55 SHA256 di pipeline+skip+take
    if (_cache.TryGetValue(cacheKey, out GetReportDataResponse cached)) { ... }
    var pipeline = req.QueryPipeline
        .Select(x => BsonDocument.Parse(x.ToString())).ToArray();          // :68 input non validato
    var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
    _cache.Set(cacheKey, response, TimeSpan.FromHours(1));                 // :81
    // catch (Exception ex) -> AddError("query", "Invalid query pipeline: " + ex.Message)  :85-87
}
```

### 2.2 Generic repository + factory registration

`MongoRepository<TDocument>` (`Infrastructure/Repositories/MongoRepository.cs`, 179 lines) exposes CRUD, `AggregateAsync` (3 overloads, lines 143-170), `CreateIndexesAsync` (max 20 fields, lines 29-40) and the `Collection` property (line 27), used by the services for operations not covered (e.g. `UpdateManyAsync`, `ReplaceOneAsync`, `GetCollection("ProcessedFiles")`).

```csharp
// LossPrevention.Infrastructure/InfrastructureServiceExtensions.cs (schema ripetuto 14 volte, righe 42-149)
services.AddScoped<IMongoRepository<User>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return new MongoRepository<User>(client, settings.DatabaseName, settings.CollectionName_Users);
});
```
`IMongoRepository<User>` is registered twice (65-70 and 72-77); the `PasswordResetTokens` collection is a literal (line 83).

### 2.3 XML → BSON conversion

`XmlToBsonConverterHelper.ConvertFlattened` parses the XML with `XElement.Parse`, flattens the hierarchy and infers the types (bool/int/decimal/datetime/string). With the default settings of `XElement.Parse`, .NET does not resolve external DTDs/entities (no XXE) and limits entity expansion: the "XML bomb" risk is contained, but there is no explicit limit on file size.

### 2.4 Rule engine

```csharp
// LossPrevention.Application/Helpers/RuleHelper.cs (estratto)
public static bool EvaluateRule(BsonDocument document, RuleConfiguration rule)
{
    var values = GetValuesByPath(document, rule.FieldPath);   // attraversa array
    if (rule.SumValues) return CompareValue(values.Where(v => v.IsNumeric).Sum(v => v.ToDecimal()), rule);
    return values.Any(v => CompareValue(v, rule));             // range numerico o uguaglianza case-insensitive
}
```
A pure and testable function, but with minimal expressiveness (1 field, no logical operators). `ApplyRulesAsync` (`Services/Data/Rules/RulesService.cs:32-84`) loads the whole of `ReportData` (line 34), removes `FraudFlags` (40-41), evaluates the enabled rules and rewrites each document with `ReplaceOneAsync` (58).

### 2.5 Similarity

`DistanceHelper.ComputeDistance` = Euclidean distance over the fields present in both records; `TryNormalize` converts only the **type** (bool→0/1, lists→mean), **not** the scale. Score: `0.5 · campiUsati/campiRichiesti + 0.5 · (1 − d/dmax)`; minimum 3 fields in common; top 10. `DistanceDataService.GetDistanceAsync` (line 22, 122 NLOC) loads into memory all documents in the date range (line 39).

### 2.6 Client-side statistical engine

`UI/src/stores/aiStore.ts`: `classifyFields` (line 379) → semantic map via regex on field names → statistics (percentiles, mean, population σ) → `build*Rules` (string conditions such as `row["Amount_total"] > 812.5`) → `new Function` (line 1769) → evaluation per grouped transaction.

### 2.7 Ingestion

`FileProcessingCoordinator` chooses the processor by `SelectedFileType` among the 3 registered `IFileProcessingService` (`Program.cs:102-104`); idempotency by file name on `ProcessedFiles` (`FileProcessingCoordinator.cs:385-409`); enrichment with `_sourceFile`, `_processedAt`, `_sourceType`. For SFTP each file is **downloaded and parsed twice** and at every execution the entire `processed/` folder is listed again.

---

## 3. Framework Usage (MVC, ORM, DI)

**Summary.** Instead of MVC the backend uses FastEndpoints (REPR); instead of an ORM it uses MongoDB.Driver directly with a generic repository; DI is the Microsoft container, with some lifetime and registration errors. FluentValidation is referenced but effectively inactive.

### 3.1 Web layer: FastEndpoints (instead of MVC)

| Aspect | Implementation | Evidence |
|---------|-----------------|----------|
| Registration | `AddFastEndpoints()` + `SwaggerDocument` ("Loss Prevention API" v1.0); `UseFastEndpoints()` last middleware | `Program.cs:53-61,131` |
| Model | `Endpoint<TRequest, TResponse>` with `Configure()` (verb, route, `Permissions`, `AllowAnonymous`, `Summary`) and `HandleAsync` | e.g. `GetReportDataEndpoint.cs:28-29` |
| Request/response | Classes in `Application/Handlers/{Requests,Responses}` (52 files) | `Application/Handlers` |
| Responses | `SendOkAsync`, `SendAsync`, `SendStringAsync`, `AddError` + `SendErrorsAsync` | e.g. `ForgotPasswordEndpoint.cs:41-56` |
| MVC controllers / Razor | Absent | — |

### 3.2 Data access: MongoDB.Driver (no ORM)

| Aspect | Implementation |
|---------|-----------------|
| Mapping | `[BsonId]`, `[BsonElement]`, `[BsonRepresentation]` attributes on the Domain entities; `ReportData` handled as a schema-less `BsonDocument` |
| Queries | `Builders<T>.Filter/Update`, LINQ expressions (`FindOneAsync(u => u.Username == ...)`), `BsonDocument[]` aggregation pipelines |
| Bulk writes | `InsertManyAsync` with `IsOrdered=false, BypassDocumentValidation=true` (`MongoRepository.cs:80-83`); `BulkWriteAsync` in `MappingService.FinalizeTypesAsync` with `Parallel.ForEachAsync` |
| Relational ORM | `DapperRepository` (Dapper/SQL) entirely commented out: leftover of a previous SQL access |

### 3.3 Dependency Injection

| Registration | Lifetime | Evidence | Observation |
|---------------|----------|----------|--------------|
| `IOptions<MongoDbSettings>`, `IOptions<JwtSettings>` | Singleton | `InfrastructureServiceExtensions.cs:26-31` | Manual bind + `Options.Create` |
| `IMongoClient` | **Scoped** | `InfrastructureServiceExtensions.cs:33-37` | New `MongoClient` per scope: anti-pattern (the client should be shared, it manages the pool) |
| `IMongoRepository<T>` ×14 | Scoped (factory) | `InfrastructureServiceExtensions.cs:42-149` | `User` registered twice |
| `MongoDbSettings` (concrete class) | **Empty** singleton | `Program.cs:83` | Injected by `CreateRuleEndpoint.cs:15` and `UpdateRuleEndpoint.cs:15`: `CollectionName_ReportData` is an empty string (defect D10) |
| 20 service registrations (incl. `EmailService`) | Scoped | `Program.cs:84-110` | 3 implementations of `IFileProcessingService` resolved as `IEnumerable` |
| `DataIngestionBackgroundService` | Hosted (singleton) | `Program.cs:111` | Creates a scope per cycle (`DataIngestionBackgroundService.cs:49`) |
| `IMemoryCache` | Singleton | `Program.cs:49` | Report cache |
| Temporary container | — | `Program.cs:40-47` | Second `ServiceCollection` built only to read `JwtSettings` (misleading comment "Load rules from MongoDB") |
| `IDataIngestionScheduleService` | Scoped | `Program.cs:99` | Never injected: unreachable code |

### 3.4 Validation: FluentValidation

`Application/Validators/CreateDashboardValidator.cs` defines `CreateDashboardValidator`, `UpdateDashboardValidator`, `DashboardBlockValidator` (namespace `LossPrevention.Application.Dashboards.Validation`); `UpdateDashboardValidator.cs` redefines `UpdateDashboardValidator` in `LossPrevention.Application.Handlers.Requests.Dashboard`. All extend `AbstractValidator<T>`: FastEndpoints automatically registers only its own `Validator<T>`, while `AbstractValidator<T>` classes are included only with `IncludeAbstractValidators = true`, an option not set in `Program.cs:53`; no endpoint invokes them manually. **The validators are therefore not executed.**

### 3.5 Frontend

Vue 3 Composition API (`<script setup>`), Pinia for state, vue-router with a guard on `localStorage.token`, Axios with an interceptor that adds the `Authorization` header (`src/api/api.ts:10-17`). The `nuxt` dependency and `nuxt.config.ts` are present but not used: the app is mounted with `createApp` in `src/main.ts`.

---

## 4. Security Implementation

**Summary.** JWT authentication and PBKDF2 hashing are substantially implemented correctly, but data authorization is delegated to the client (arbitrary pipelines, row lock), and there are direct exposures (hashes returned, secrets in configuration, `eval`).

### 4.1 Implemented controls

| Control | Implementation | Evidence |
|-----------|-----------------|----------|
| Authentication | JWT HMAC-SHA256; validation of issuer, audience, lifetime, signature | `Program.cs:63-76`; `LoginEndpoint.cs:48-80` |
| Authorization | `permissions` claim (one per permission of the roles) + `Permissions("CAN_...")` on the endpoints: 37 distinct permissions, 74 protected endpoints, 4 anonymous | `LoginEndpoint.cs:57-61` |
| Password | PBKDF2-SHA256, 600,000 iterations, 16-byte salt, 32-byte hash, format `v2:{iter}:{base64}`; legacy 10,000-iteration hashes upgraded at login | `Infrastructure/Helpers/PasswordHasher.cs:7-50`; `UserService.cs:206-214` |
| Inactive accounts | Login and reset rejected if `IsActive=false` | `UserService.cs:194`, `PasswordResetService.cs:42` |
| Password reset | Random 32-byte token (base64url), stored as SHA256 hex, 1 h expiry, previous tokens invalidated | `PasswordResetService.cs:19,50-66,145-159` |
| Anti-enumeration | `ForgotPassword` always responds with a generic message if the user does not exist | `ForgotPasswordEndpoint.cs:41,56` |

### 4.2 Code-level weaknesses

| Pattern | Location | Risk |
|---------|-----------|---------|
| Aggregation pipeline from the client (`BsonDocument.Parse`) | `GetReportDataEndpoint.cs:68` | Query injection / exfiltration (e.g. `$lookup` on `Users`) |
| Client-only row-level lock | `LockField/LockValue` claims (`LoginEndpoint.cs:65-68`) never read by the backend; applied in `UI/src/helpers/fieldLock.ts` (`resultsGrid.vue:1687`, `toolbar.vue:180`, `tabularBlock.vue:259`) | Bypass with a direct API call |
| Hash and salt in the DTOs | `GetUsersEndpoint.cs:36` | Offline cracking |
| `eval(expr)` on calculated fields | `UI/src/components/reports/resultsGrid.vue:1377` | Persistent XSS via shared workspace |
| `new Function(...)` | `UI/src/stores/aiStore.ts:1769` | Injection through field names coming from imported files |
| Token in `localStorage` | `loginStore.ts:92`, `api.ts:11`, `fieldLock.ts:2`, `notificationStore.ts:49`, `App.vue:284` | Exfiltrable by any XSS |
| Non-constant-time hash comparison | `PasswordHasher.cs:46` (`string.Equals`) | Timing attack (low risk) |
| No lockout / rate limiting on login | no reference in the backend | Brute force |
| Distinct message for inactive account in reset | `ForgotPasswordEndpoint.cs:45-47` | Enumeration of inactive accounts |
| Secrets in clear text | `JwtSettings:SecretKey` in `appsettings.json`; `SftpPassword` in the `DataIngestionConfigurations` document | Token / SFTP compromise |
| SMTP without TLS | `EmailService.cs:64-65` (`EnableSsl=false`, `UseDefaultCredentials=true`) | Reset link in clear text |
| SFTP without host key verification | `SftpFileProcessingService.cs:56` | MITM |
| Notifications without recipient filter | `ListNotificationsEndpoint.cs:30-31` (`Filter.Empty`) | Every user sees all notifications |
| `ex.Message` returned to the client | 13 occurrences in the endpoints | Information disclosure |

---

## 5. Exception Handling & Logging

**Summary.** Error handling is local to each endpoint/service (generic try/catch), with no global middleware or custom ProblemDetails; structured logging (`ILogger`) is used only in 4 classes, the rest use `Console.WriteLine` or nothing.

### 5.1 Exceptions

| Aspect | Observed | Evidence |
|---------|-----------|----------|
| Generic catches | 31 `catch (Exception …)` in the backend | grep on `*.cs` |
| Internal messages to the client | 13 `ex.Message` in the endpoints (e.g. `"Invalid query pipeline: " + ex.Message`) | `GetReportDataEndpoint.cs:87` |
| Exceptions thrown | BCL only: `InvalidOperationException` (24), `KeyNotFoundException` (9), `ArgumentException` (5), `UnauthorizedAccessException` (3), `DirectoryNotFoundException` (2), `NotImplementedException` (1), `NotSupportedException` (1), `ArgumentNullException` (1) | grep `throw new` |
| Domain exceptions | `DeleteUserRoleException`, `InvalidCustomerException` defined but never used | `Domain/Exceptions/` |
| Global handler | Absent (no `UseExceptionHandler`, no custom `ProblemDetails`) | `Program.cs:121-131` |
| Startup | `DatabaseInitializationService` rethrows the exception → the API does not start | `DatabaseInitializationService.cs:43-116` |
| Background | The scheduler loop catches and logs errors, continues at the next minute | `DataIngestionBackgroundService.cs` |

### 5.2 Logging

| Aspect | Observed |
|---------|-----------|
| Framework | `Microsoft.Extensions.Logging` with default providers (console/debug); level `Default: Information` in `appsettings.json` |
| Use of `ILogger<T>` | Only `DatabaseInitializationService`, `DataIngestionBackgroundService`, `FileProcessingCoordinator`, `SftpFileProcessingService` |
| `Console.WriteLine` | 6: `DistanceDataservice.cs:86`, `MappingService.cs:336` and `:421`, console `Program.cs:80`, `:108`, `:122`; `MappingService.WriteLog` writes to the console |
| Frontend | 57 `console.log`, including with data (semantic maps, counts, base URL in `api.ts:7`) |
| Correlation / audit | No correlation id, no security logging (failed logins, permission changes); only `createdBy/updatedBy` on `FraudDetectionSettings` |

Details on log management as data: [08_data.md §7](08_data.md#7-log-management).

---

## 6. Transaction Management

**Summary.** No transactions: the code uses neither MongoDB sessions (`StartSession`, `WithTransaction`) nor `TransactionScope`. All multi-document operations are non-atomic and without compensation.

| Multi-step operation | Steps | Evidence | Effect of an intermediate error |
|-----------------------|-------|----------|---------------------------------|
| Apply rules | `UpdateMany` unset `FraudFlags` → `ReplaceOne` per document → delete `FraudFlags` mappings → `ProcessMappings` | `RulesService.cs:40-78` | Dataset with partial flags and missing mappings |
| Rule update | `Set` flag on every document → mapping update | `RulesService.cs:141-167` | Flags and mappings misaligned |
| File ingestion | `InsertMany` ReportData (unordered) → `InsertOne` ProcessedFiles | `FileProcessingCoordinator.cs` | Crash between the two steps → file re-ingested (duplicates) |
| Role deletion | `UpdateOne` for each user → `DeleteOne` role | `UserRoleService.cs:59-79` | Users modified but role still existing |
| Permission deletion | `UpdateOne` for each role → `DeleteById` permission | `UserPermissionService.cs:117-133` | same |
| Password reset | Invalidate tokens → insert token → send email; then update password → mark token used | `PasswordResetService.cs:50-66,122-139` | Valid token but email not sent |
| Workspace deletion | `DeleteOne` workspace | `WorkspaceService.cs:75-87` | Linked dashboards remain orphaned |

Concurrency: no optimistic locking (no version field); `ApplyRulesAsync` and ingestion can run in parallel on the same collection. MongoDB multi-document transactions would require a replica set; the server topology is not derivable from the code (dump from `mongod` 8.3.2).

---

## 7. Configuration Management

**Summary.** Configuration is in a single `appsettings.json` per host, with no per-environment files, no secret store and with a loading sequence that makes environment-variable overrides ineffective; the frontend depends on a non-versioned `VITE_API_BASE_URL` and on hardcoded URLs.

### 7.1 Configuration sources

| Host | File | Keys | Loading |
|------|------|--------|-------------|
| API | `LossPrevention.API/appsettings.json` | `Logging`, `DataRetention:TransactionRetentionDays=180`, `AllowedHosts`, `MongoDbSettings` (connection string + `DatabaseName` + 13 `CollectionName_*`), `JwtSettings` (`SecretKey`, `Issuer=LossPrevention`, `Audience=User`, `ExpiryHours=1`), `Email` (host, port 25, sender, `FrontendUrl`) | `WebApplication.CreateBuilder` + **re-adding** of `appsettings.json` (`Program.cs:36-38`) |
| API | `Properties/launchSettings.json` | Ports 5264/7110, IIS Express, `ASPNETCORE_ENVIRONMENT=Development` | Development only |
| Console | `LossPrevention.DataIngestionService/appsettings.json` | `Logging`, `MongoDbSettings` with 6 `CollectionName_*` | JSON-only `ConfigurationBuilder` (`DataIngestionService/Program.cs:31-35`), passed to the services even after the Generic Host has been created |
| SPA | `import.meta.env.VITE_API_BASE_URL` | API base URL | No `.env` in the repository |

### 7.2 Access modes in the code

| Mode | Where |
|----------|------|
| `IOptions<T>` | `JwtSettings` in `LoginEndpoint.cs:17-21`; `MongoDbSettings` in the repository factories |
| Direct `IConfiguration` | `EmailService.cs:17-24` (with hardcoded defaults), `DatabaseInitializationService.cs:23` (`GetValue<int>(..., 90)`) |
| Hardcoded values | CORS `localhost:5173/5174` (`Program.cs:29`), collection `"PasswordResetTokens"` (`InfrastructureServiceExtensions.cs:83`), `"ProcessedFiles"` (`FileProcessingCoordinator.cs:391,409`), `C:\xmlstore5\xml` (console `Program.cs:76`), Ollama URL and model (`aiStore.ts:8,88`), 1 min polling (`DataIngestionBackgroundService.cs:13`) |
| Runtime configuration in the DB | `DataIngestionConfigurations` (source, SFTP, scheduling), `FraudDetectionSettings` (thresholds), `Rules`, `Mappings` |

### 7.3 Configuration defects

| # | Defect | Evidence | Effect |
|---|---------|----------|---------|
| C1 | `appsettings.json` re-added after the default sources | `Program.cs:36-38` | The file values take precedence over environment variables and arguments: impossible to override `JwtSettings:SecretKey` or the connection string without modifying the file |
| C2 | Empty `MongoDbSettings` singleton | `Program.cs:83` | D10 (see Appendix B) |
| C3 | No per-environment file (`appsettings.{Environment}.json`) nor user-secrets | file search | Secrets and development hosts in the repository |
| C4 | Divergent retention defaults | code 90 days (`DatabaseInitializationService.cs:23`) vs config 180 days | Different behavior if the key is missing |
| C5 | Console with 6 collection names out of 13 | `DataIngestionService/appsettings.json` | Unconfigured repositories receive a null collection name (not used by the console) |

---

## 8. Testing Strategy

**Summary.** No testing strategy is implemented at all: zero test projects, zero test files, no npm test/lint/type-check scripts, no CI pipeline.

| Level | Status | Evidence |
|---------|-------|----------|
| Backend unit tests | Absent | No `*Tests.csproj` in the solution (5 projects) |
| Frontend unit tests | Absent | No `*.spec.*`/`*.test.*` files, no `vitest.config.*`; `package.json` has only `dev`, `build`, `preview` |
| Integration / API tests | Absent | — |
| E2E | Absent | No Playwright/Cypress dependency |
| Static analysis | Absent | No ESLint/Prettier config; `vue-tsc` installed but not used in `build`; no additional .NET analyzers |
| CI | Absent | No workflow/pipeline |
| Test data | `Data/LossPrevention` dump (3,000 transactions) and `Data/MongoDBScripts` seeds usable as manual fixtures | — |

Historical note: the vendor's `Docs/13_TESTING_QA.md` (present only in commit `593f6de`, removed in `d768cd9`; `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/13_TESTING_QA.md`) described xUnit, Vitest and coverage targets (unit 80%, integration 70%) with current coverage "TBD": none of this is present in the code.

Priority candidates for the first tests (pure functions or known defects): `RuleHelper.EvaluateRule`, `DistanceHelper.ComputeDistance`, `BsonHelper.ConvertToMappedType`/`TryToDateTimeUtc`, `PasswordHasher`, `XmlToBsonConverterHelper`, `queryUtils.buildTypeAwareCondition`; regression tests on D1-D10.

---

## Appendices

### A. Complexity hotspots

`lizard` 1.24.1 measurement re-run on 2026-10-08 on the backend and `LossPrevention.UI/src` (threshold CCN > 15):

| Function | File:line | NLOC | CCN |
|----------|-----------|------|-----|
| `buildGenericFraudRuleTemplates` | `UI/src/stores/aiStore.ts:1203` | 254 | **87** |
| `enrichWithCrossTransactionMetrics` | `aiStore.ts:986` | 124 | 43 |
| `save` | `UI/src/stores/dataIngestionStore.ts:164` | 55 | 36 |
| `analyzeFraudInData` | `aiStore.ts:1667` | 131 | 34 |
| `buildPipeline` | `UI/src/components/dashboard/chartblock.vue:69` | 44 | 28 |
| `HandleAsync` | `API/Endpoints/DataIngestion/UpdateDataIngestionConfigurationEndpoint.cs:23` | 68 | 24 |
| `ConvertToMappedType` / `TryToDateTimeUtc` | `Application/Helpers/BsonHelper.cs:115` / `:269` | 35 / 66 | 21 / 21 |
| `buildTypeAwareCondition` | `UI/src/helpers/queryUtils.ts:63` | 43 | 21 |
| `classifyFields` | `aiStore.ts:379` | 61 | 21 |
| `GetDistanceAsync` | `Application/Services/Data/DistanceDataservice.cs:22` | 122 | 20 |
| `TryToBoolean` | `BsonHelper.cs:236` | 29 | 20 |
| `handleDrill` | `UI/src/components/reports/heatmapPreview.vue:400` | 33 | 20 |
| `ToDocument` | `Application/Mappings/NotificationMapping.cs:25` | 17 | 19 |

Other functions above the threshold (CCN 16-20): `buildVelocityRules` (aiStore.ts:620), `buildSweetheartingRules` (:838), `load` (dataIngestionStore.ts:230), `detectUniqueTransactionFields` (fraudDetectionStore.ts:595), `afterDraw` (heatmapPreview.vue:273), anonymous function in `manage/dataingestion.vue:123`.

### B. Functional defects identified in the code

| # | Defect | Location | Effect |
|---|---------|-----------|---------|
| D1 | `CreateTransactionEndpoint` calls `ProcessAsync` without inserting and reads `bson["_id"]`, which is never set; `XmlProcessingService.InsertManyAsync` throws `NotImplementedException` | `Endpoints/Data/CreateTransactionEndpoint.cs:49-50`, `XmlProcessingService.cs` | Exception or no persistence |
| D2 | JWT token returned as a serialized `Task` (missing `await`) | `LoginEndpoint.cs:39-40` | The FE depends on `token.result` (`loginStore.ts:77`) |
| D3 | `UpdateRuleAsync` writes `Set($"{rule.RuleName}", …)` at the root and looks up the mapping `Name == RuleName` without the `FraudFlags.` prefix | `RulesService.cs:147,155` | Duplicated flags at the root; renaming a rule does not update the mapping |
| D4 | `ApplyRulesAsync` deletes all `FraudFlags` mappings and regenerates them by sampling 1,000 documents | `RulesService.cs:64-78` | Custom aliases/visibility lost |
| D5 | `LastRunAt` set but not persisted | `DataIngestionBackgroundService.cs` | Possible double execution in the same window; no history |
| D6 | `GET /notifications` without recipient filter | `ListNotificationsEndpoint.cs:30-31` | Every user sees all notifications |
| D7 | `keyValue.AsString` on the key field | `DistanceDataservice.cs:176` | Exception if the key field is not a string |
| D8 | Tautological visibility filter | `ReportDataservice.cs:75` | "Non-visible" fields are returned |
| D9 | Anti-fraud statistics computed only on the loaded page (≤ 3,000 rows) | `aiStore.ts` | Thresholds dependent on sorting/pagination |
| D10 | `MongoDbSettings` registered as an empty singleton and injected into Create/UpdateRule | `Program.cs:83`; `CreateRuleEndpoint.cs:15,56`; `UpdateRuleEndpoint.cs:15,62`; `RulesService.cs:110,167` | `FraudFlags.*` mappings created with `CollectionName = ""`, hence excluded by the `CollectionName == "ReportData"` filter (`DistanceDataservice.cs:49`) |

D8 in detail (`ReportDataservice.cs:75`):
```csharp
else if (visibleFields.Contains(kv.Key) || kv.Key == "count" || !visibleFields.Any() || !visibleFields.Contains(kv.Key))
```
The condition is a tautology (`A || … || !A`).

### C. Dead code / leftovers

| Element | Evidence |
|----------|----------|
| `DapperRepository.cs` (205 lines), `IDapperRepository.cs` | Entirely commented out |
| `XmlProcessingService.InsertManyAsync` | `throw new NotImplementedException` |
| `IXmlEnrichmentRule` | No implementation → `XmlEnrichmentService.Enrich` is a no-op |
| `DataIngestionScheduleService` / `DataIngestionSchedules` collection | Registered (`Program.cs:99`) but never injected |
| `DeleteUserRoleException`, `InvalidCustomerException` | Never used |
| Console STEP 1 | Loads the rules and does not use them (`DataIngestionService/Program.cs:38-47`, "Just load rules") |
| `nuxt.config.ts`, dependencies `nuxt`, `jspdf`, `jspdf-autotable`, `grid-layout-plus`, `vuedraggable` | Not referenced by the code |
| Commented-out block in `DistanceDataservice` (`*Count` counts) | — |
| `Mappings_old.bson`, `ReportData_old.bson` in the dump | See [08_data.md §2](08_data.md#2-physical-data-model) |

---

## Reference Documents

- **Deep Dive Analysis**: [00_deep_dive.md](00_deep_dive.md)
- **Context**: [01_context.md](01_context.md)
- **Functional Overview**: [02_functional_overview.md](02_functional_overview.md)
- **Non-Functional Overview**: [03_non_functional_overview.md](03_non_functional_overview.md)
- **Constraints**: [04_constraints.md](04_constraints.md)
- **Principles**: [05_principles.md](05_principles.md)
- **Software Architecture**: [06_software_architecture.md](06_software_architecture.md)
- Related downstream documents: [08_data.md](08_data.md) · [14_metrics.md](14_metrics.md) · [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) · [17_backend_deep_assessment.md](17_backend_deep_assessment.md) · [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md)

---

## Change Log

| Version | Date | Author | Changes |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Document creation (replaces run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Restructured into the 8 sections of the prompt; added sections Framework Usage (FastEndpoints, MongoDB.Driver, DI with lifetimes, inactive FluentValidation), Transaction Management, Configuration Management (ineffective env override, empty MongoDbSettings), Testing Strategy; code structure with verified counts; extended Security Implementation (controls present + 15 weaknesses with file:line); new defect D10; lizard re-run (+4 hotspots); extended dead code (ScheduleService, domain exceptions); DapperRepository 204→205 lines; vendor test doc cited as historical (`593f6de`/`d768cd9`); complete Reference Documents |
