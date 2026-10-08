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
# IMPACT how (FULL) — Indice documentazione — LOSS_PREVENTION

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (modifiche locali solo in `Docs/` e `.github/`; codice applicativo identico al commit)

Repository: `saristot/LOSS_PREVENTION` → cartella `customspa-it-loss-prevention-d098a8b97860/Docs/`.

Il codice applicativo è stato importato nel commit `593f6de` e non è più cambiato: i commit successivi toccano solo la documentazione. Tutti i deliverable sono stati **verificati punto per punto** rispetto ai prompt `IMPACT_DISCOVERY_HOW_*` e al sorgente al baseline `fc7d820`, completati dove mancavano sezioni e riallineati tra loro sui valori canonici (revisione 1.1, 2026-10-08).

## Artifact set HOW

| # | Documento | File | Stato |
|---|-----------|------|-------|
| 00 | Deep Dive | [00_deep_dive.md](00_deep_dive.md) | verificato e completato (1.1) |
| 01 | Context | [01_context.md](01_context.md) | verificato e completato (1.1) |
| 02 | Functional Overview | [02_functional_overview.md](02_functional_overview.md) | verificato e completato (1.1) |
| 03 | Non-Functional Overview | [03_non_functional_overview.md](03_non_functional_overview.md) | verificato e completato (1.1) |
| 04 | Constraints | [04_constraints.md](04_constraints.md) | verificato e completato (1.1) |
| 05 | Principles | [05_principles.md](05_principles.md) | verificato e completato (1.1) |
| 06 | Software Architecture | [06_software_architecture.md](06_software_architecture.md) | verificato e completato (1.1) |
| 07 | Code | [07_code.md](07_code.md) | verificato e completato (1.1) |
| 08 | Data | [08_data.md](08_data.md) | verificato e completato (1.1) |
| 09 | Infrastructure Architecture | [09_infrastructure_architecture.md](09_infrastructure_architecture.md) | verificato e completato (1.1) |
| 10 | Deployment | [10_deployment.md](10_deployment.md) | verificato e completato (1.1) |
| 11 | Development Environment | [11_development_environment.md](11_development_environment.md) | verificato e completato (1.1); sostituisce `11_development_environment.md.md` |
| 12 | Operation and Support | [12_operation_and_support.md](12_operation_and_support.md) | verificato e completato (1.1) |
| 13 | Decision Log | [13_decision_log.md](13_decision_log.md) | verificato e completato (1.1) |
| 14 | Metrics | [14_metrics.md](14_metrics.md) | verificato e completato (1.1) — fonte canonica delle metriche |
| 15 | FP & COCOMO II | [15_fp_cocomo.md](15_fp_cocomo.md) | verificato e completato (1.1) |
| 16 | Frontend Deep Assessment | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) | verificato e completato (1.1) |
| 17 | Backend Deep Assessment | [17_backend_deep_assessment.md](17_backend_deep_assessment.md) | verificato e completato (1.1) |
| 18 | Antipattern Deep Dive | [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md) | verificato e completato (1.1) |
| 19 | Modernization Estimation | [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md) | verificato e completato (1.1) |

## Valori canonici (usati in modo coerente in tutti i documenti)

| Area | Valore | Fonte |
|------|--------|-------|
| Stack backend | .NET 8, FastEndpoints 6.0.0, MongoDB.Driver 3.4.0, FluentValidation 11.11.0, SSH.NET | `*.csproj` |
| Stack frontend | Vue 3.5 + Vite 6 + Vuetify + Pinia (SPA; `nuxt` presente ma inutilizzato) | `LossPrevention.UI/package.json` |
| Endpoint | 78 (4 anonimi, 74 con permesso); 20 iniettano `IMongoRepository` direttamente | `LossPrevention.API/Endpoints/` |
| Permessi | 37 `CAN_*` usati dagli endpoint, 41 negli script di seed | [04_constraints.md](04_constraints.md), [17_backend_deep_assessment.md](17_backend_deep_assessment.md) |
| Frontend | 41 SFC `.vue`, 15 store Pinia, 17 route + 1 alias di redirect | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |
| Collezioni MongoDB | 13 configurate in `appsettings.json`, 15 referenziate dal codice, 12 nel dump `Data/LossPrevention` (10 + `Mappings_old`, `ReportData_old`) | [08_data.md](08_data.md) |
| Import FE | 12 con maiuscole/minuscole errate (build rotta su FS case-sensitive) + 1 irrisolvibile nell'orfano `ScheduleForm.vue` | [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |
| Dimensione | 25.261 SLOC (cloc 2.10): C# 10.143, Vue 10.773, TS 3.868; baseline di stima 19.774 | [14_metrics.md](14_metrics.md) |
| Stima | 435 UFP; COCOMO II 78,3 PM, 14,7 mesi, ≈626 k€ | [15_fp_cocomo.md](15_fp_cocomo.md), [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md) |
| Salute | FE 3,4/10, BE 3,9/10, antipattern 45/100; debito BE 67,5–97,5 gg | [16](16_frontend_deep_assessment.md), [17](17_backend_deep_assessment.md), [18](18_antipattern_deep_dive.md) |
| Dipendenze FE | `npm audit`: 71 vulnerabilità (6 critical, 46 high, 15 moderate, 4 low) | [14_metrics.md](14_metrics.md) |

## Principali evidenze

- **Architettura**: monolite .NET 8 a strati (API → Application/Infrastructure → Domain) più un `DataIngestionService` separato; il Domain dipende da `MongoDB.Bson`, quindi non è indipendente dalla persistenza.
- **Persistenza**: `IMongoClient` registrato Scoped e `new MongoClient` per ogni repository; indice TTL su `BeginDateTime`, campo assente nel dump.
- **Configurazione**: `Program.cs:36-38` ricarica `appsettings` per ultimo, quindi le variabili d'ambiente non possono sovrascriverlo.
- **Sicurezza**: password SFTP salvata in chiaro e restituita dall'API; segreti in `appsettings.json` (non riportati nei documenti).
- **Ingestion e regole**: lo scheduler confronta l'ora locale con UTC; le regole antifrode si applicano solo manualmente.
- **AI e analytics**: NLQ e generazione assistita via Ollama locale (`http://localhost:11434`); analisi statistiche lato browser.
- **Qualità**: nessun test, lint o CI; `npm ci` fallisce per il lockfile non allineato (usare `npm install`).

## Note di validazione (2026-10-08)

- Presenti tutti i 20 deliverable HOW più questo indice; ognuno inizia con il blocco `IMPACT-META` (commit `fc7d820…`), la riga Baseline e un Change Log con la revisione 1.1.
- Nessun link relativo `.md` interrotto; sezione Reference Documents con i collegamenti 00–19 in ogni documento.
- 52 diagrammi Mermaid validati con il parser `mermaid@11`: 0 errori.
- Numeri incrociati tra documenti verificati (endpoint, permessi, SFC, collezioni, import, SLOC, FP/COCOMO, health score).
- Nessun segreto riportato: l'unica stringa di connessione citata è `mongodb://localhost:27017`; la `SecretKey` JWT non compare.
- **Documenti vendor storici**: i riferimenti a `Docs/01_EXECUTIVE_OVERVIEW.md`, `roadmap.txt`, `04_DEPLOYMENT_GUIDE.md` e simili riguardano file presenti solo nel commit `593f6de` e rimossi in `d768cd9`. Sono marcati come storici e consultabili con `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`.
- **Non ricavabili dal codice** (marcati N/A o come ipotesi): SLA/RPO/RTO, owner e budget, volumi di produzione, metriche runtime, CVE NuGet (serve il .NET SDK, non disponibile nell'ambiente di analisi).
- **Ipotesi da confermare**: tariffa 400 €/gg e risposte al Wizard di stima in [15_fp_cocomo.md](15_fp_cocomo.md) e [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md).