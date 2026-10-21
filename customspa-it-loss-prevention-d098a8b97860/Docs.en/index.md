<!-- IMPACT-META
schema: 1
mode: how
step: index
commit: fc7d820908b8b230fb47a2669920ba7efdb413a8
commit_short: fc7d820
branch: main
worktree: dirty
baseline_date: 2026-10-07T18:19:51+02:00
generated_at: 2026-10-08T10:48:09.375+02:00
-->
# IMPACT how (FULL) — Documentation index — LOSS_PREVENTION

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (local changes only in `Docs/` and `.github/`; application code identical to the commit)

Repository: `saristot/LOSS_PREVENTION` → folder `customspa-it-loss-prevention-d098a8b97860/Docs/`.

The application code was imported in commit `593f6de` and has not changed since: subsequent commits touch only the documentation. All deliverables have been **verified point by point** against the `IMPACT_DISCOVERY_HOW_*` prompts and the source at baseline `fc7d820`, completed where sections were missing, and realigned with each other on the canonical values (revision 1.1, 2026-10-08).

## HOW artifact set

| # | Document | File | Status |
|---|-----------|------|-------|
| 00 | Deep Dive | [00_deep_dive.md](00_deep_dive.md) | verified and completed (1.1) |
| 01 | Context | [01_context.md](01_context.md) | verified and completed (1.1) |
| 02 | Functional Overview | [02_functional_overview.md](02_functional_overview.md) | verified and completed (1.1) |
| 03 | Non-Functional Overview | [03_non_functional_overview.md](03_non_functional_overview.md) | verified and completed (1.1) |
| 04 | Constraints | [04_constraints.md](04_constraints.md) | verified and completed (1.1) |
| 05 | Principles | [05_principles.md](05_principles.md) | verified and completed (1.1) |
| 06 | Software Architecture | [06_software_architecture.md](06_software_architecture.md) | verified and completed (1.1) |
| 07 | Code | [07_code.md](07_code.md) | verified and completed (1.1) |
| 08 | Data | [08_data.md](08_data.md) | verified and completed (1.1) |
| 09 | Infrastructure Architecture | [09_infrastructure_architecture.md](09_infrastructure_architecture.md) | verified and completed (1.1) |
| 10 | Deployment | [10_deployment.md](10_deployment.md) | verified and completed (1.1) |
| 11 | Development Environment | [11_development_environment.md](11_development_environment.md) | verified and completed (1.1); replaces `11_development_environment.md.md` |
| 12 | Operation and Support | [12_operation_and_support.md](12_operation_and_support.md) | verified and completed (1.1) |
| 13 | Decision Log | [13_decision_log.md](13_decision_log.md) | verified and completed (1.1) |
| 14 | Metrics | [14_metrics.md](14_metrics.md) | verified and completed (1.1) — canonical source of the metrics |
| 15 | FP & COCOMO II | [15_fp_cocomo.md](15_fp_cocomo.md) | verified and completed (1.1) |
| 16 | Frontend Deep Assessment | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) | verified and completed (1.1) |
| 17 | Backend Deep Assessment | [17_backend_deep_assessment.md](17_backend_deep_assessment.md) | verified and completed (1.1) |
| 18 | Antipattern Deep Dive | [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md) | verified and completed (1.1) |
| 19 | Modernization Estimation | [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md) | verified and completed (1.1) |

## Canonical values (used consistently across all documents)

| Area | Value | Source |
|------|--------|-------|
| Backend stack | .NET 8, FastEndpoints 6.0.0, MongoDB.Driver 3.4.0, FluentValidation 11.11.0, SSH.NET | `*.csproj` |
| Frontend stack | Vue 3.5 + Vite 6 + Vuetify + Pinia (SPA; `nuxt` present but unused) | `LossPrevention.UI/package.json` |
| Endpoint | 78 (4 anonymous, 74 with permission); 20 inject `IMongoRepository` directly | `LossPrevention.API/Endpoints/` |
| Permissions | 37 `CAN_*` used by the endpoints, 41 in the seed scripts | [04_constraints.md](04_constraints.md), [17_backend_deep_assessment.md](17_backend_deep_assessment.md) |
| Frontend | 41 `.vue` SFCs, 15 Pinia stores, 17 routes + 1 redirect alias | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |
| MongoDB collections | 13 configured in `appsettings.json`, 15 referenced by the code, 12 in the `Data/LossPrevention` dump (10 + `Mappings_old`, `ReportData_old`) | [08_data.md](08_data.md) |
| FE imports | 12 with wrong upper/lower case (build broken on case-sensitive FS) + 1 unresolvable in the orphan `ScheduleForm.vue` | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |
| Size | 25,261 SLOC (cloc 2.10): C# 10,143, Vue 10,773, TS 3,868; estimation baseline 19,774 | [14_metrics.md](14_metrics.md) |
| Estimate | 435 UFP; COCOMO II 78.3 PM, 14.7 months, ≈626 k€ | [15_fp_cocomo.md](15_fp_cocomo.md), [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md) |
| Health | FE 3.4/10, BE 3.9/10, antipattern 45/100; BE debt 67.5–97.5 days | [16](16_frontend_deep_assessment.md), [17](17_backend_deep_assessment.md), [18](18_antipattern_deep_dive.md) |
| FE dependencies | `npm audit`: 71 vulnerabilities (6 critical, 46 high, 15 moderate, 4 low) | [14_metrics.md](14_metrics.md) |

## Main evidence

- **Architecture**: layered .NET 8 monolith (API → Application/Infrastructure → Domain) plus a separate `DataIngestionService`; the Domain depends on `MongoDB.Bson`, so it is not persistence-independent.
- **Persistence**: `IMongoClient` registered as Scoped and `new MongoClient` for each repository; TTL index on `BeginDateTime`, a field absent from the dump.
- **Configuration**: `Program.cs:36-38` reloads `appsettings` last, so environment variables cannot override it.
- **Security**: SFTP password stored in clear text and returned by the API; secrets in `appsettings.json` (not reported in the documents).
- **Ingestion and rules**: the scheduler compares local time with UTC; the anti-fraud rules are applied only manually.
- **AI and analytics**: NLQ and assisted generation via local Ollama (`http://localhost:11434`); statistical analyses on the browser side.
- **Quality**: no tests, lint or CI; `npm ci` fails because the lockfile is not aligned (use `npm install`).

## Validation notes (2026-10-08)

- All 20 HOW deliverables are present plus this index; each one starts with the `IMPACT-META` block (commit `fc7d820…`), the Baseline line and a Change Log with revision 1.1.
- No broken relative `.md` links; Reference Documents section with the 00–19 links in every document.
- 52 Mermaid diagrams validated with the `mermaid@11` parser: 0 errors.
- Cross-document numbers verified (endpoints, permissions, SFCs, collections, imports, SLOC, FP/COCOMO, health score).
- No secrets reported: the only connection string cited is `mongodb://localhost:27017`; the JWT `SecretKey` does not appear.
- **Historical vendor documents**: the references to `Docs/01_EXECUTIVE_OVERVIEW.md`, `roadmap.txt`, `04_DEPLOYMENT_GUIDE.md` and similar concern files present only in commit `593f6de` and removed in `d768cd9`. They are marked as historical and can be consulted with `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`.
- **Not derivable from the code** (marked N/A or as assumptions): SLA/RPO/RTO, owner and budget, production volumes, runtime metrics, NuGet CVEs (the .NET SDK is required, not available in the analysis environment).
- **Assumptions to be confirmed**: rate of 400 €/day and answers to the estimation Wizard in [15_fp_cocomo.md](15_fp_cocomo.md) and [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md).