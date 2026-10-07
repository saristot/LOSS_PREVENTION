# REVERSE how (FULL) — Indice documentazione — LOSS_PREVENTION

> **Baseline commit:** `593f6de` (`main`) · worktree `clean` · generato il 2026-10-07
> Questa raccolta **sostituisce integralmente** l'output della run del 2026-10-05 (deep dive per endpoint, C4, WBS, final report, analysis report), che conteneva informazioni non verificate (es. frontend React, stime WBS non derivate dal codice).

Repository: `saristot/LOSS_PREVENTION` → cartella `customspa-it-loss-prevention-d098a8b97860/docs/` (file `00`–`19`).

| # | Documento | Atlas uniqueName | Contenuto |
|---|-----------|------------------|-----------|
| 00 | Deep Dive (Executive) | `REVERSE_FINAL_REPORT` | Stack, struttura, metriche, sicurezza, critical observations |
| 01 | Context | `REVERSE_HOW_01_CONTEXT` | Scope, attori, landscape, compliance |
| 02 | Functional Overview + verifica funzionalità dichiarate | `REVERSE_HOW_02_FUNCTIONAL_OVERVIEW` | Use case, feature catalog, **§6 fraud detection / statistica / AI: presenti?** |
| 03 | Non-Functional Overview | `REVERSE_HOW_03_NON_FUNCTIONAL` | NFR AS-IS vs TARGET |
| 04 | Constraints | `REVERSE_HOW_04_CONSTRAINTS` | Vincoli |
| 05 | Principles | `REVERSE_HOW_05_PRINCIPLES` | Principi impliciti e target |
| 06 | Software Architecture (C4) | `REVERSE_ARCHITECTURE_C4` | Container, componenti, integrazioni, rischi |
| 07 | Code | `REVERSE_HOW_07_CODE` | Pattern, hotspot, 9 difetti, dead code |
| 08 | Data (ERD) | `REVERSE_HOW_08_DATA` | 15 collezioni, ERD, struttura ReportData |
| 09 | Infrastructure Architecture | `REVERSE_HOW_09_INFRASTRUCTURE` | AS-IS locale, target |
| 10 | Deployment | `REVERSE_HOW_10_DEPLOYMENT` | Unità di deploy, CI/CD proposta |
| 11 | Development Environment | `REVERSE_HOW_11_DEV_ENVIRONMENT` | Setup, build, troubleshooting |
| 12 | Operation and Support | `REVERSE_HOW_12_OPERATIONS` | Monitoring, logging, manutenzione |
| 13 | Decision Log (ADR) | `REVERSE_HOW_13_DECISION_LOG` | ADR ricostruiti e proposti |
| 14 | Metrics | `REVERSE_ANALYSIS_REPORT` | cloc, lizard, dipendenze, qualità |
| 15 | FP & COCOMO II | `REVERSE_HOW_15_FP_COCOMO` | 406–486 FP, valore di ricostruzione |
| 16 | Frontend Deep Assessment | `REVERSE_HOW_16_FRONTEND_ASSESSMENT` | Vue 3 health 4,5/10 |
| 17 | Backend Deep Assessment | `REVERSE_HOW_17_BACKEND_ASSESSMENT` | .NET 8 health 5/10, debt register |
| 18 | Antipattern Deep Dive | `REVERSE_HOW_18_ANTIPATTERN` | Health 52/100, refactoring |
| 19 | Modernization Estimation | `REVERSE_WBS_MODERNIZATION` | Scenari + **§7 effort feature in sviluppo/pianificate** |

## Risposte rapide

**Le funzionalità dichiarate sono presenti?** (doc 02 §6)
- Fraud detection engine: **sì, in forma base** (regole su singolo campo, applicazione manuale; non real-time; applicazione automatica in ingestione non trovata).
- Analisi statistica: **sì, ma solo nel browser** (percentili/σ, 30 euristiche), risultati non persistiti né auditabili; similarity euclidea senza normalizzazione.
- Componente AI: **sì, limitato** a NLQ e generazione template report via LLM Ollama `qwen2.5:14b` su `localhost` del browser; nessun ML; l'"analisi frodi" non usa AI.

**Effort per completare** (doc 19 §7, incl. PM 15%, 400 €/gg):
- In sviluppo: **43–72 gg** (17–29 k€)
- Pianificate: **209–313 gg** (84–125 k€)
- Gap per rendere affidabili le funzioni già dichiarate + hardening: **106–164 gg** (43–66 k€)
- Totale: **358–550 gg** (143–220 k€)
