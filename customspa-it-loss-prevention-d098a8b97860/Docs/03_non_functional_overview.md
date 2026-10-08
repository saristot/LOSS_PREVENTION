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
# Non-Functional Overview - Progetto LOSS_PREVENTION

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (modifiche locali solo in `Docs/` e `.github/`; codice applicativo identico al commit)

**Data**: 2026-10-08  
**Versione**: 1.1  
**Autori**: IMPACT REVERSE how (analisi statica del codice + verifica IMPACT)  
**Audience**: Architetti software, tech lead, team di sviluppo, QA

---

### Metodo e convenzioni

- Il repository **non contiene requisiti non funzionali formalizzati**: nessun SLA, benchmark, test di carico, telemetria o configurazione di monitoraggio.
- Ogni requisito riporta:
  - **AS-IS**: comportamento osservato nel codice al commit `fc7d820`, con evidenza `file:riga`;
  - **TARGET SMART**: requisito proposto, misurabile e con orizzonte temporale. I target sono **proposte da validare** con il committente; l'orizzonte "R1" indica il primo rilascio di modernizzazione (vedi [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md));
  - **Priorità**: Critical / High / Medium / Low.
- **[NON VALUTABILE]** marca i requisiti per cui il codice e la storia del repository non forniscono elementi espliciti (regola del prompt IMPACT 03).
- La documentazione del fornitore (`Docs/roadmap.txt`, `Docs/09_SECURITY_DOCUMENTATION.md`, ecc.) **non esiste al baseline**: era presente solo nel commit `593f6de` ed è stata rimossa in `d768cd9`. Si consulta con `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`.

---

## 1. MANIFEST Service Level Requirements

Requisiti percepiti direttamente dall'utente: prestazioni, usabilità, affidabilità e disponibilità. Il codice mostra diversi colli di bottiglia strutturali (scansioni complete della collezione, calcolo nel browser), ma nessuna misura reale.

### 1.1 Performance

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-PERF-00 | **[NON VALUTABILE]** Tempi di risposta reali (P50/P95): nessun benchmark, test di carico o telemetria nel repository | Definire una baseline con test di carico prima di R1 | High |
| NFR-PERF-01 | Query report: l'aggregazione gira **2 volte** per richiesta (`$count` + pagina, `ReportDataservice.cs:40-52`); cache in-memory 1 h (`GetReportDataEndpoint.cs:81`); nessun limite superiore a `Take` (solo `Take > 0`, riga 45; la heatmap chiede 100.000 righe, `components/reports/toolbar.vue:193`) | P95 `POST /data/report/query` < 3 s con 1 M documenti e `Take ≤ 1.000`, indici sui campi filtrati; misurato con test di carico in R1 | Critical |
| NFR-PERF-02 | `ApplyRulesAsync`: `GetAllAsync()` su tutta `ReportData` (`RulesService.cs:34`) + un `ReplaceOneAsync` per documento (riga 58) | Applicazione regole su 1 M documenti < 10 min e memoria processo < 1 GB (bulk write / pipeline update), entro R1 | High |
| NFR-PERF-03 | `GetDistanceAsync`: carica in memoria tutti i documenti dell'intervallo (`DistanceDataservice.cs:39`) e tutte le mapping; costo O(N·F) | P95 `POST /distance` < 5 s con 100 k documenti nell'intervallo, entro R1 | High |
| NFR-PERF-04 | Analisi statistica nel browser: una `POST /data/report/query` per pagina + valutazione JS (`resultsGrid.vue:1175-1203`) | Analisi server-side di 1 M transazioni < 15 min, asincrona con avanzamento, entro R1 | High |
| NFR-PERF-05 | Ingestione: SFTP sequenziale file per file, in due passi (download + rilettura da `processed/`, `FileProcessingCoordinator.cs:236-331`); console con `Parallel.ForEachAsync` e `InsertOneAsync` per documento (`DataIngestionService/Program.cs:85, 102`) | ≥ 50.000 transazioni/min per istanza con insert in batch, misurato su un dataset reale | Medium |
| NFR-PERF-06 | Indici su `ReportData`: solo `_id` e `ttl_BeginDateTime` (`Data/LossPrevention/ReportData.metadata.json`). `IndexService.ProcessIndexesAsync` (`Services/Data/IndexSuggestionHelper.cs:21-39`) crea un indice per **ogni** campo dei primi 100 documenti, ma è invocato solo dalla console (`DataIngestionService/Program.cs:117`) | Indici gestiti da codice/migrazione sui campi di filtro e ordinamento ricorrenti; ≤ 15 indici per collezione; verifica `explain()` delle query principali | High |

### 1.2 Usability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-USA-01 | Solo inglese, stringhe hardcoded nei 41 SFC; nessuna libreria i18n in `package.json` | 100% delle label esternalizzate e traduzione IT/EN entro R1 | Medium |
| NFR-USA-02 | Accessibilità non indirizzata: 1 solo attributo `aria-` (`App.vue:19`) e 1 solo `alt=` in `src/`; nessun test a11y | WCAG 2.1 AA sulle 6 schermate principali (Login, Home, Query, Dashboard, Rules, Users); 0 violazioni "serious" con axe-core in CI | Medium |
| NFR-USA-03 | Feedback via toast/snackbar; gli errori dell'ingestione schedulata sono visibili solo nei log; nel run manuale l'API risponde 500 e la UI mostra un messaggio generico (`dataingestion.vue:458-472`) | Storico esecuzioni ingestione in UI (esito, file, record, errori) per ≥ 30 giorni | High |
| NFR-USA-04 | Logout automatico alla scadenza del token (1 h, `loginStore.ts:54-62`) senza preavviso né refresh token | Avviso 5 min prima della scadenza e rinnovo silenzioso della sessione | Low |
| NFR-USA-05 | **[NON VALUTABILE]** Uso da mobile/tablet: Vuetify è responsive, ma non esistono requisiti o test su dispositivi | — | Low |
| NFR-USA-06 | Debug lasciato nel codice: 57 `console.log` in `src/` (es. ogni `computed` del menu in `App.vue:314-369`) | 0 `console.log` nel build di produzione (regola lint) | Low |

### 1.3 Reliability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-REL-00 | **[NON VALUTABILE]** MTBF / tasso di errore: nessun dato operativo, log persistente o monitoraggio | Misurare il tasso di errori 5xx dopo l'introduzione dei log strutturati | Medium |
| NFR-REL-01 | `ApplyRulesAsync` esegue `$unset FraudFlags` su tutti i documenti (`RulesService.cs:40-41`) e poi riscrive documento per documento: un errore a metà lascia i flag incoerenti | Operazione idempotente per batch con stato di esecuzione persistito; ripresa dopo errore senza perdita di flag | High |
| NFR-REL-02 | Ingestione: `InsertMany` e `MarkFileAsProcessed` non transazionali (`FileProcessingCoordinator.cs:374-377`); dedup solo per nome file; nessuna sessione/transazione Mongo nel codice (0 occorrenze di `StartSession`) | Exactly-once per file (hash contenuto + stato) verificato da test di integrazione | High |
| NFR-REL-03 | Scheduler: `LastRunAt` impostato ma non salvato (`DataIngestionBackgroundService.cs:86-87`); schedule `one-time` mai eseguito (riga 55); flag `ManualLoad` mai letto | Scheduler persistente con stato e storico; 100% delle schedule configurate eseguite o segnalate | High |
| NFR-REL-04 | 31 blocchi `catch (Exception …)` nel backend, molti restituiscono `ex.Message` al client (es. `GetReportDataEndpoint.cs:85-88`); nessuna policy di retry (0 riferimenti a Polly/Retry) | Gestione errori centralizzata (ProblemDetails) con correlation-id; retry con backoff verso Mongo/SFTP/SMTP | Medium |
| NFR-REL-05 | `POST /data/create-transactions` risponde "Inserted" ma non persiste (`CreateTransactionEndpoint.cs:49-53`: solo `ProcessAsync`, nessun insert); `XmlProcessingService.InsertManyAsync` lancia `NotImplementedException` (riga 43) | 0 endpoint che dichiarano successo senza effetto, verificato da test | High |
| NFR-REL-06 | Coerenza dati: la cache 1 h non viene invalidata dopo ingestione o `Apply Rules` (chiave = SHA-256 di pipeline+skip+take, `GetReportDataEndpoint.cs:92-98`) | Dati visibili entro 1 min da ingestione/regole (invalidazione esplicita) | Medium |

### 1.4 Availability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-AV-00 | **[NON VALUTABILE]** SLA %: nessun accordo né obiettivo nel repository | Proposta: 99,5% mensile in orario lavorativo, da validare | High |
| NFR-AV-01 | Singola istanza: `IMemoryCache` (`Program.cs:49`) e scheduler in-process (`Program.cs:111`) impediscono uno scale-out sicuro (job duplicati). La roadmap fornitore storica (`roadmap.txt`, commit `593f6de`) indicava "Load balancing - SOA - At least 2 applications for uptime" | ≥ 2 istanze API dietro load balancer con scheduler single-leader e cache distribuita, entro R1 | High |
| NFR-AV-02 | **[NON VALUTABILE]** RPO/RTO: nessuna strategia di backup/restore nel codice; esiste solo un mongodump di sviluppo in `Data/LossPrevention` | Proposta: RPO ≤ 24 h, RTO ≤ 4 h con restore provato ogni trimestre | High |
| NFR-AV-03 | Failover: nessuno. Nessun health check (`AddHealthChecks` assente in `Program.cs`); il DB è inizializzato in modo sincrono all'avvio (`Program.cs:115-119`): se Mongo non è raggiungibile l'API non parte | Endpoint `/health/live` e `/health/ready`; riavvio automatico gestito dalla piattaforma | High |

---

## 2. OPERATIONAL Service Level Requirements

Requisiti di esercizio: throughput, serviceability, testability, manageability e security. La sicurezza è l'area con più lacune ed è trattata riga per riga.

### 2.1 Throughput

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-THR-01 | **[NON VALUTABILE]** Volume atteso (transazioni/giorno, utenti concorrenti): nessun dato. Il dump di sviluppo contiene ≈ 3.000 documenti `ReportData` | Raccogliere i volumi reali dal committente (la roadmap storica riportava "Real world data … Waiting for Custom") | High |
| NFR-THR-02 | Nessun rate limiting (`AddRateLimiter` assente), nessun limite esplicito sul body (si applica il default Kestrel di ~30 MB) | Login ≤ 5 tentativi/min per IP; API ≤ 100 richieste/min per utente; `Take` ≤ 1.000 | High |

### 2.2 Serviceability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-SRV-01 | Log solo con il provider console di default (`appsettings.json` → `Logging:LogLevel:Default = Information`); `ILogger<T>` usato in 4 classi (`DatabaseInitializationService`, `DataIngestionBackgroundService`, `FileProcessingCoordinator`, `SftpFileProcessingService`); 6 `Console.WriteLine` | Log strutturati JSON con correlation-id su 100% delle richieste; retention ≥ 30 giorni | High |
| NFR-SRV-02 | Nessun health check, metrica o tracing | Health check + metriche (latenza P95, errori, durata ingestione) esposte entro R1 | High |

### 2.3 Testability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-TST-01 | 0 progetti di test .NET; nessuno script `test` in `package.json` (solo `dev/build/preview`); nessuna CI | ≥ 60% line coverage sui servizi Application e ≥ 80% su `RuleHelper` / `DistanceHelper` entro R1; build + test su ogni PR | Critical |
| NFR-TST-02 | Logica di business nel browser (`aiStore.ts` 1.994 righe, `analyzeFraudInData` alla riga 1667) e regole compilate con `new Function` (riga 1769): non testabile lato server | Motore statistico server-side con test unitari deterministici | High |
| NFR-TST-03 | 20 endpoint usano direttamente `IMongoRepository<…>`, quindi servono test di integrazione con Mongo reale | Endpoint senza accesso diretto al repository (verificabile con ArchUnitNET) | Medium |

### 2.4 Manageability

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-MNG-01 | Un solo `appsettings.json` (nessun `appsettings.{Environment}.json`); CORS hardcoded su `localhost:5173/5174` (`Program.cs:24-34`); URL LLM hardcoded (`aiStore.ts:88`); base URL API da `VITE_API_BASE_URL`, senza file `.env` versionato | Configurazione per ambiente (Dev/UAT/Prod); 0 URL o host hardcoded nel codice | High |
| NFR-MNG-02 | Seed e migrazioni con script `mongosh` manuali ordinati `00..04` (`Data/MongoDBScripts`) | Migrazioni versionate eseguite automaticamente al deploy | Medium |

### 2.5 Security

| ID | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|------------------|-----------------------|----------|
| NFR-SEC-01 | Pipeline di aggregazione arbitraria dal client (`GetReportDataRequest.QueryPipeline: List<object>`, `BsonDocument.Parse` in `GetReportDataEndpoint.cs:66-69`) | Whitelist di stage/operatori; test che rifiutano `$lookup`, `$out`, `$merge`, `$unionWith`, `$function`, `$where` | Critical |
| NFR-SEC-02 | Row-level lock (`LockField/LockValue`) applicato solo nel browser (`fieldLock.ts`); la cache non è per utente (`GetReportDataEndpoint.cs:92-98`) | Filtro imposto lato server su 100% degli endpoint dati; chiave di cache con identità/lock | Critical |
| NFR-SEC-03 | `PasswordHash`/`PasswordSalt` restituiti da `GetUsersEndpoint.cs:36`, `GetUserByIdEndpoint.cs:56-57`, `GetUserByUsernameEndpoint.cs:55-56` | 0 campi credenziali nelle risposte API (test di contratto) | Critical |
| NFR-SEC-04 | `JwtSettings.SecretKey` in chiaro in `appsettings.json`; firma HMAC-SHA256 simmetrica (`Program.cs:63-76`) | 0 segreti nel repository (secret scanning in CI); chiave da secret store con rotazione | Critical |
| NFR-SEC-05 | Nessun `UseHttpsRedirection`/HSTS; SMTP con `EnableSsl = false` (`EmailService.cs:64`); SFTP solo password, senza verifica host key | TLS 1.2+ end-to-end; SMTP STARTTLS; SFTP con chiave e host key pinning | High |
| NFR-SEC-06 | ✅ Password PBKDF2-SHA256 600.000 iterazioni con upgrade degli hash legacy da 10.000 (`PasswordHasher.cs:9-10, 21`; `UserService.cs:205-213`); token di reset hashati SHA-256 e validi 1 h | Mantenere; aggiungere lockout dopo 5 tentativi falliti | Medium |
| NFR-SEC-07 | `GET /notifications` restituisce tutte le notifiche (`ListNotificationsEndpoint.cs:30-31`) | Ogni utente vede solo le notifiche destinate a sé, al proprio gruppo o ruolo | High |
| NFR-SEC-08 | **[NON VALUTABILE]** Audit log: assente nel codice. La documentazione fornitore storica (`09_SECURITY_DOCUMENTATION.md:596`, commit `593f6de`) lo dichiarava "✅" | Audit di login, modifiche a regole/soglie/utenti e analisi eseguite, conservato ≥ 1 anno | High |

---

## 3. DEVELOPMENT Service Level Requirements

Requisiti sulla realizzabilità e pianificabilità dello sviluppo. Lo stack è mainstream, ma mancano tutti gli strumenti di qualità e di build riproducibile.

| ID | Categoria | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|-----------|------------------|-----------------------|----------|
| NFR-DEV-01 | Realizability | Stack mainstream (.NET 8, FastEndpoints 6, MongoDB.Driver 3.4, Vue 3.5, Vuetify 3.7, Vite 6): skill facilmente reperibili. .NET 8 è LTS con fine supporto 10/11/2026 | Migrazione a .NET 10 LTS prima del 10/11/2026 | High |
| NFR-DEV-02 | Realizability | Pacchetto obsoleto `Microsoft.AspNetCore.Http.Features 5.0.17`; dipendenza `nuxt ^3.0.0` presente ma non usata; `xlsx ^0.18.5` (ultima versione pubblicata su npm) | 0 pacchetti deprecati o inutilizzati; audit dipendenze in CI | Medium |
| NFR-DEV-03 | Planability | Nessun backlog, stima o milestone nel repository. La roadmap fornitore storica (`roadmap.txt`, solo commit `593f6de`) indicava "Within 3 Months Time frame" e "Initial Roadmap - 3-6 Months", senza stime per attività | Backlog stimato (vedi [15_fp_cocomo.md](15_fp_cocomo.md) e [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md)) | Medium |
| NFR-DEV-04 | Build reproducibility | Nessuna pipeline CI/CD né Dockerfile; `package.json` con range `^` e `package-lock.json` **non allineato** (`npm ci` fallisce); nessuno script `test`/`lint`/`type-check` | Build CI deterministica (`npm ci`, `dotnet build`) verde su ogni PR | High |
| NFR-DEV-05 | Build reproducibility | 12 import con maiuscole/minuscole non corrispondenti al nome file (es. `@/stores/workspaceStore` vs `workspacestore.ts` in 6 file) e un import irrisolvibile su ogni OS (`ScheduleForm.vue:27` → `@/store/useDataIngestionStore`) | Build verde su Linux; 0 import con case errato | High |

---

## 4. EVOLUTIONARY Service Level Requirements

Requisiti di evoluzione nel tempo. Il modello schema-on-read dà flessibilità di ingestione; la concentrazione della logica in pochi file molto grandi ne limita la manutenibilità.

| ID | Categoria | AS-IS (evidenza) | TARGET SMART proposto | Priorità |
|----|-----------|------------------|-----------------------|----------|
| NFR-EVO-01 | Scalability | Dati schema-less + aggregazioni Mongo: scalano lato DB. Logica antifrode nel browser e scheduler/cache in-process: non scalano | Calcolo antifrode server-side; API stateless scalabile orizzontalmente (≥ 2 istanze) | High |
| NFR-EVO-02 | Extensibility | Strategy per formati file (`IFileProcessingService`: XML, CSV, JSON registrati in `Program.cs:102-104`); `IXmlEnrichmentRule` previsto ma **senza implementazioni registrate** | Nuovo formato aggiungibile con 1 classe + 1 registrazione, senza modifiche al coordinator | Low |
| NFR-EVO-03 | Maintainability | Hotspot: `buildGenericFraudRuleTemplates` CCN 87 / 254 NLOC (`aiStore.ts:1203-1530`, misurato con lizard); `resultsGrid.vue` 2.271 righe; `aiStore.ts` 1.994 righe; endpoint FraudDetection 388/407 righe con mapping duplicato | Nessuna funzione con CCN > 15; nessun file > 500 righe nei moduli toccati da R1 | High |
| NFR-EVO-04 | Flexibility | Mapping generati dai dati: nuovi campi senza deploy; ma regole, report salvati, TTL ed euristiche dipendono dai nomi dei campi sorgente | Contratto dati versionato per formato sorgente; regole riferite ad alias stabili | Medium |
| NFR-EVO-05 | Portability | Path Windows hardcoded nella console (`C:\xmlstore5\xml`, `DataIngestionService/Program.cs:76`); 12 import case-sensitive che rompono il build su Linux/macOS; per il resto runtime cross-platform (.NET 8, Node) | Build ed esecuzione su container Linux, 0 path assoluti nel codice | Medium |
| NFR-EVO-06 | Reusability | `MongoRepository<T>` generico riusato da tutti i servizi, ma espone `Collection` (`IMongoRepository.cs:170`); logica statistica non riusabile dal backend | Repository senza `Collection` esposta; motore regole/statistiche in libreria condivisa | Medium |

---

## 5. NFR Priority Matrix

La matrice riassume le priorità e le collega alle decisioni architetturali documentate in [06_software_architecture.md](06_software_architecture.md) e [13_decision_log.md](13_decision_log.md).

| Priorità | NFR | Driver architetturale |
|----------|-----|-----------------------|
| **Critical** | NFR-SEC-01..04, NFR-TST-01, NFR-PERF-01 | Validazione server-side delle query, filtro dati server-side, gestione segreti, test automatici |
| **High** | NFR-PERF-00/02/03/04/06, NFR-USA-03, NFR-REL-01/02/03/05, NFR-AV-00..03, NFR-THR-01/02, NFR-SRV-01/02, NFR-TST-02, NFR-MNG-01, NFR-SEC-05/07/08, NFR-DEV-01/04/05, NFR-EVO-01/03 | Calcolo vicino ai dati, scheduler persistente single-leader, osservabilità, CI |
| **Medium** | NFR-PERF-05, NFR-USA-01/02, NFR-REL-00/04/06, NFR-TST-03, NFR-MNG-02, NFR-SEC-06, NFR-DEV-02/03, NFR-EVO-04/05/06 | i18n/a11y, error handling, migrazioni, portabilità |
| **Low** | NFR-USA-04/05/06, NFR-EVO-02 | Rifiniture UX, estendibilità già adeguata |

### 5.1 Trade-off

| Trade-off | Opzione A | Opzione B | Impatto / costo |
|-----------|-----------|-----------|-----------------|
| Calcolo antifrode nel browser vs server | Browser (as-is): nessun costo server | Server: risultati persistiti, auditabili, testabili | B aumenta il carico DB/CPU, ma è prerequisito per NFR-SEC-08 e NFR-TST-02 |
| Schema-less vs schema esplicito | Schema-on-read (as-is): ingestione flessibile | Contratto dati per formato | B richiede manutenzione dei contratti, ma abilita indici, validazione e regole stabili |
| Cache in-process vs distribuita | `IMemoryCache` (as-is): latenza minima, zero infrastruttura | Redis/cache distribuita | B aggiunge un componente da gestire; è necessaria per ≥ 2 istanze (NFR-AV-01) |
| Disponibilità 99,5% vs 99,9% | 99,5%: ≈ 3,6 h di downtime/mese, 2 istanze in una sola region | 99,9%: ≈ 43 min/mese, richiede DB replicato e deploy zero-downtime | B costa sensibilmente di più (replica set, automazione rilasci); il valore va validato con il committente |
| LLM locale vs gateway centralizzato | Ollama su ogni postazione (as-is): nessun costo server | Gateway LLM lato server | B richiede GPU/servizio gestito, ma rende l'AI deployabile e governabile |

---

## 6. Out of Scope

Requisiti esplicitamente esclusi o non deducibili, da non considerare nei target di R1 salvo nuova richiesta.

| Elemento | Motivo |
|----------|--------|
| Alta disponibilità geografica multi-region | Nessun requisito o indizio nel codice; costo sproporzionato rispetto all'uso batch |
| Latenza real-time (< 1 s) sull'arrivo delle transazioni | Il modello è batch (polling 60 s, file); la dichiarazione "real-time" del fornitore non è supportata dal codice (vedi [02_functional_overview.md](02_functional_overview.md) §7) |
| App mobile nativa | Nessun client mobile nel repository |
| Requisiti marcati [NON VALUTABILE] (NFR-PERF-00, NFR-USA-05, NFR-REL-00, NFR-AV-00, NFR-AV-02, NFR-THR-01, NFR-SEC-08) | Servono dati dal committente o da monitoraggio in esercizio prima di fissare un target vincolante |

---

## Reference Documents

- **Deep Dive Analysis**: [00_deep_dive.md](00_deep_dive.md)
- **Context**: [01_context.md](01_context.md)
- **Functional Overview**: [02_functional_overview.md](02_functional_overview.md)
- Documenti IMPACT correlati: [04_constraints.md](04_constraints.md) · [05_principles.md](05_principles.md) · [06_software_architecture.md](06_software_architecture.md) · [07_code.md](07_code.md) · [08_data.md](08_data.md) · [09_infrastructure_architecture.md](09_infrastructure_architecture.md) · [10_deployment.md](10_deployment.md) · [11_development_environment.md](11_development_environment.md) · [12_operation_and_support.md](12_operation_and_support.md) · [13_decision_log.md](13_decision_log.md) · [14_metrics.md](14_metrics.md) · [15_fp_cocomo.md](15_fp_cocomo.md) · [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) · [17_backend_deep_assessment.md](17_backend_deep_assessment.md) · [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md) · [19_modernization_estimation_spec.md](19_modernization_estimation_spec.md)

---

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Header/baseline `dirty`. Introdotti i marker [NON VALUTABILE] (prestazioni misurate, MTBF, SLA, RPO/RTO, volumi, mobile, audit). Target riscritti in forma SMART, priorità su ogni riga, ID per tutti gli NFR, sezione Operational divisa in 5 sottosezioni con Security riga per riga. Aggiunti: trade-off con costi, collegamento ai documenti 06/13. Corretti: lockfile "presente" → non allineato (`npm ci` fallisce), più i 12 import case-sensitive e l'import irrisolvibile di `ScheduleForm.vue`; `IndexService` (file `IndexSuggestionHelper.cs`) invocato solo dalla console. Citazioni di `roadmap.txt` / `09_SECURITY_DOCUMENTATION.md` marcate come storiche (`593f6de`, rimosse in `d768cd9`). Reference Documents completi |
