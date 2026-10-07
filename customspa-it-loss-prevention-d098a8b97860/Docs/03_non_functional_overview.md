<!-- REVERSE-META
schema: 1
mode: how
step: 03_non_functional_overview
commit: 593f6decec3a642d5567754dd795b19560bb0afb
commit_short: 593f6de
branch: main
worktree: clean
baseline_date: 2026-10-05T10:14:05+00:00
generated_at: 2026-10-07T14:40:00Z
-->
# Non-Functional Overview - Progetto LOSS_PREVENTION

> **Baseline commit:** `593f6de` (`main`) · worktree `clean`

**Data**: 2026-10-07 · **Versione**: 1.0 · **Autori**: REVERSE how · **Audience**: architetti, tech lead, sviluppatori, QA

> Il repository non contiene requisiti non funzionali formalizzati. Per ogni categoria si riporta **AS-IS** (comportamento osservato nel codice) e **TARGET** (requisito SMART proposto). I target sono proposte da validare con il committente.

---

## 1. MANIFEST Service Level Requirements

### 1.1 Performance

| ID | AS-IS (evidenza) | TARGET proposto | Priorità |
|----|------------------|-----------------|----------|
| NFR-PERF-01 | Query report: aggregazione eseguita **2 volte** (count + pagina) per richiesta; cache in-memory 1 h (`GetReportDataEndpoint`) | P95 `POST /data/report/query` < 3 s su 1 M documenti con indici sui campi filtrati | Critical |
| NFR-PERF-02 | `ApplyRulesAsync`: `GetAllAsync()` su `ReportData` + 1 `ReplaceOne` per documento | Applicazione regole su 1 M documenti < 10 min, memoria < 1 GB (bulk write / pipeline update) | High |
| NFR-PERF-03 | `GetDistanceAsync`: carica in memoria tutti i documenti dell'intervallo date + tutte le mapping; O(N·F) | P95 < 5 s su 100 k documenti nell'intervallo | High |
| NFR-PERF-04 | Analisi statistica nel browser pagina per pagina (fetch + valutazione JS) | Analisi server-side di 1 M transazioni < 15 min, asincrona con progress | High |
| NFR-PERF-05 | Ingestione SFTP sequenziale file-per-file con file temporaneo; batch console `Parallel.ForEachAsync` con insert singolo | ≥ 50 k transazioni/min | Medium |
| NFR-PERF-06 | Nessun indice oltre `_id` e `ttl_BeginDateTime` (metadata dump) | Indici gestiti da codice su campi di filtro/ordinamento ricorrenti (`IndexSuggestionHelper` esiste ma solo nel job console) | High |

### 1.2 Usability

| ID | AS-IS | TARGET | Priorità |
|----|-------|--------|----------|
| NFR-USA-01 | UI Vuetify Material, solo inglese, stringhe hardcoded | i18n (IT/EN) per tutte le label | Medium |
| NFR-USA-02 | Nessuna verifica di accessibilità | WCAG 2.1 AA sulle schermate principali | Medium |
| NFR-USA-03 | Feedback via toast; errori ingestione visibili solo nei log | Stato e storico esecuzioni ingestione visibili in UI | High |

### 1.3 Reliability

| ID | AS-IS | TARGET | Priorità |
|----|-------|--------|----------|
| NFR-REL-01 | `ApplyRulesAsync` fa `$unset FraudFlags` su tutto e poi riscrive documento per documento: un errore a metà lascia flag incoerenti | Operazione idempotente e atomica per batch, con stato di esecuzione | High |
| NFR-REL-02 | Ingestione: insert + `MarkFileAsProcessed` non transazionali; `LastRunAt` non persistito | Exactly-once per file (hash contenuto + stato), scheduler persistente | High |
| NFR-REL-03 | 31 `catch (Exception)` generici, alcuni che restituiscono solo il messaggio | Gestione errori centralizzata (ProblemDetails) con correlation-id | Medium |

### 1.4 Availability

| ID | AS-IS | TARGET | Priorità |
|----|-------|--------|----------|
| NFR-AV-01 | Singola istanza; `IMemoryCache` e scheduler in-process impediscono lo scale-out sicuro (job duplicati) | 99,5% mensile in orario lavorativo; ≥ 2 istanze API (come da `roadmap.txt`) con scheduler single-leader | High |
| NFR-AV-02 | Nessuna strategia di backup nel codice; esiste un dump mongodump di sviluppo | RPO ≤ 24 h, RTO ≤ 4 h | High |

---

## 2. OPERATIONAL Service Level Requirements

| Categoria | AS-IS | TARGET | Priorità |
|-----------|-------|--------|----------|
| **Throughput** | Nessun limite su upload (default Kestrel 30 MB), nessun rate limiting | Rate limit login (es. 5/min/IP) e API (100 req/min/utente) | High |
| **Serviceability** | Nessun health check; log su console | `/health/live`, `/health/ready` (Mongo, SMTP), log strutturati JSON | High |
| **Testability** | 0 test; dipendenze statiche (`RuleHelper`, `DistanceHelper` testabili ma non testati); logica di business nel browser | ≥ 60% coverage servizi Application, test E2E sui 5 journey principali | Critical |
| **Manageability** | Config in `appsettings.json` unico; CORS hardcoded | Config per ambiente + segreti da Key Vault/variabili d'ambiente | High |
| **Security** | Pipeline arbitraria, lock client-side, hash esposti, secret versionato, no HTTPS/HSTS | OWASP ASVS L2; 0 vulnerabilità High/Critical aperte; secret rotation | Critical |

---

## 3. DEVELOPMENT Service Level Requirements

| Categoria | AS-IS | TARGET |
|-----------|-------|--------|
| **Realizability** | Stack mainstream (.NET 8, Vue 3): skill facilmente reperibili | Mantenere stack; aggiornare a .NET 10 LTS |
| **Planability** | Roadmap testuale senza stime; nessun backlog | Backlog con stime (vedi documenti 15 e 19) |
| **Build reproducibility** | Nessuna pipeline; `package.json` con range `^` ma lockfile presente | Build CI deterministica su ogni PR |

---

## 4. EVOLUTIONARY Service Level Requirements

| Categoria | AS-IS | Valutazione |
|-----------|-------|-------------|
| **Scalability** | Dati schema-less + aggregazioni dinamiche: scalano bene in Mongo; logica nel browser no | Spostare il calcolo su server/DB |
| **Extensibility** | Strategy per formati file (`IFileProcessingService`), `IXmlEnrichmentRule` (estensione prevista ma **nessuna implementazione registrata**) | Buona predisposizione |
| **Maintainability** | CCN medio basso, ma hotspot: `buildGenericFraudRuleTemplates` CCN 87, `resultsGrid.vue` 2.271 righe, `aiStore.ts` 1.994 righe | Refactoring hotspot necessario |
| **Flexibility** | Mapping dinamici permettono nuovi formati senza deploy | Punto di forza |
| **Portability** | Path Windows hardcoded nel job (`C:\xmlstore5\xml`); per il resto cross-platform | Rimuovere path hardcoded |
| **Reusability** | `MongoRepository<T>` generico riusato | Ok |

---

## 5. NFR Priority Matrix

| Priorità | NFR |
|----------|-----|
| **Critical** | Security (ASVS L2), Testability, Performance query report |
| **High** | Performance regole/distance/analisi, Reliability ingestione e regole, Availability ≥ 2 istanze, Serviceability, Manageability, Throughput/rate limit, Backup |
| **Medium** | Usability i18n/a11y, error handling, ingestion throughput |
| **Low** | Portabilità job console |

**Trade-off principali**
- *Calcolo nel browser vs server*: oggi zero costo server ma risultati non affidabili/auditabili; spostarlo su server aumenta carico DB ma è prerequisito per compliance.
- *Schema-less vs schema esplicito*: flessibilità di ingestione contro difficoltà di indicizzazione e validazione.
- *Cache in-memory*: riduce latenza ma impedisce lo scale-out e serve dati stantii fino a 1 h dopo ingestione/regole.

---

## 6. Out of Scope
Alta disponibilità geografica multi-region, latenza real-time (< 1 s) sull'arrivo delle transazioni (il modello è batch), mobile app nativa.

---

## Reference Documents
- 00_deep_dive.md · 01_context.md · 02_functional_overview.md

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
