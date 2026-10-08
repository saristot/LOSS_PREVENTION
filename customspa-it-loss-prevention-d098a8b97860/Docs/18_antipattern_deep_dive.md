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

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (modifiche locali solo in `Docs/` e `.github/`; codice applicativo identico al commit)

**Data**: 2026-10-08
**Codebase**: `customspa-it-loss-prevention-d098a8b97860` (LOSS_PREVENTION): backend .NET 8 / FastEndpoints / MongoDB (5 progetti) + SPA Vue 3 / Vite / Pinia (`LossPrevention.UI`)
**Health Score**: **45 / 100**: 100 − Σ penalità dei 20 finding (vedi "Derivazione dell'Health Score")

## 📊 RIASSUNTO ESECUTIVO

Il backend è ordinato a livello macro (layer riconoscibili, REPR, funzioni brevi, nessuna dipendenza circolare), ma ha un'astrazione MongoDB che "perde" e un dominio anemico. Il debito più grave è nel frontend: due god file incorporano logica di business e di sicurezza, generata ed eseguita dinamicamente (`eval`, `new Function`). Il debito tecnico è **critico**: 4 finding critici toccano la sicurezza dei dati e l'affidabilità delle decisioni antifrode, anche se il volume complessivo resta gestibile (≈ 20 interventi mirati).

### Derivazione dell'Health Score

Penalità per finding: Critica = 6, Alta = 4, Media = 2, Bassa = 1. Ai pattern assenti (Big Ball of Mud, Circular Dependency) non si applica alcuna penalità.

| Severità | N. finding | Penalità unitaria | Totale |
|---|---|---|---|
| Critica | 4 | 6 | 24 |
| Alta | 2 | 4 | 8 |
| Media | 9 | 2 | 18 |
| Bassa | 5 | 1 | 5 |
| **Totale** | **20** | | **55** |

**Health Score = 100 − 55 = 45 / 100.**

> Nota di versione: la v1.0 riportava 52/100 senza formula. Il valore è stato ricalcolato con la formula esplicita sopra, sui 20 finding verificati in questa versione.

## 🚨 TABELLA PRIORITÀ (Top Findings)

| File/Modulo | Anti-Pattern | Severità | Impatto |
| :--- | :--- | :--- | :--- |
| **AP-01** `LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs:66-71` | Golden Hammer ("la pipeline Mongo dal client risolve tutto") / violazione del trust boundary | Critica | Query injection: lettura di altre collection via `$lookup`/`$unionWith` (utenti, token, password SFTP), DoS; impossibile applicare filtri di sicurezza |
| **AP-02** `LossPrevention.UI/src/stores/aiStore.ts` (1.994 righe, 99 funzioni) | God Object / The Blob + code generation (`new Function`, `:1769`) | Critica | LLM, classificazione, statistica, generazione ed esecuzione regole e stato UI in un solo store; non testabile, non riusabile dal backend |
| **AP-03** `LossPrevention.UI/src/components/reports/resultsGrid.vue` (2.271 righe) | God Object + `eval` (code injection, `:1377`) | Critica | Espressioni salvate nei workspace condivisi ed eseguite nel browser di altri utenti (XSS persistente con accesso al JWT in `localStorage`) |
| **AP-04** `LossPrevention.UI/src/helpers/fieldLock.ts:1-48` | Security by obscurity (enforcement client-side) | Critica | Il row-level lock è applicato solo dal client; basta modificare la richiesta per leggere i dati di tutti i negozi |
| **AP-05** `LossPrevention.Application/Services/Data/Rules/RulesService.cs:34-58, 141-155` | Leaky abstraction + N+1 writes | Alta | Full scan e `ReplaceOne` per documento; flag cancellati in massa prima del ricalcolo; `UpdateRuleAsync` scrive nel path sbagliato |
| **AP-06** BE `RuleHelper` ↔ FE `aiStore.ts`/`fraudDetectionStore.ts:177` | Stovepipe / split-brain della logica antifrode | Alta | Due motori antifrode con modelli diversi; soglie di default definite solo nel frontend; risultati non riproducibili né auditabili lato server |
| **AP-07** `LossPrevention.Infrastructure/Repositories/MongoRepository.cs:27` | Leaky abstraction (`Collection` pubblica) | Media | Servizi ed endpoint accoppiati al driver; accesso ad altre collection aggirando il repository |
| **AP-08** `LossPrevention.Application.csproj` → Infrastructure; `Domain/Entities/*` con 22 attributi `[Bson*]` | Vendor lock-in (MongoDB nel dominio) / inversione delle dipendenze | Media | Dominio non portabile; test e cambi di persistenza costosi |
| **AP-09** 20 endpoint (Dashboard, Groups, Notifications, FraudDetection…) | Fat endpoint / layer skipping | Media | Logica e mapping negli endpoint, duplicati e non testabili in isolamento |
| **AP-10** `Create/UpdateFraudDetectionSettingsEndpoint.cs` (47-388 ≈ 66-407) | Copy-paste programming | Media | 341 righe clonate; 5 metodi di mapping identici da 162 NLOC |
| **AP-11** `Domain/Entities/*`; `AddMemberToGroupEndpoint.cs:56-63` | Anemic Domain Model + Feature Envy | Media | Invarianti sparse in endpoint e helper; lost update sui gruppi |
| **AP-12** soglia antifrode in 8 file (BE + FE) | Shotgun Surgery | Media | Aggiungere una soglia richiede modifiche coordinate in 2 linguaggi |
| **AP-13** `aiStore.ts:8, 88, 111` | Vendor/runtime lock-in (Ollama) + Golden Hammer regex | Media | Provider LLM e URL hardcoded nel client; classificazione dei campi fragile |
| **AP-14** `aiStore.ts:1203-1530` (CCN 87); `ReportDataservice.cs:126-150`; `MappingService.cs:269-288` | Spaghetti / Deeply nested code | Media | 20 funzioni con CCN > 15; manutenzione e test molto costosi |
| **AP-15** 31 `catch (Exception)` + 4 catch vuoti (BE); 115 `catch` / 55 `console.error` (FE) | Error swallowing / generic exceptions | Media | Errori nascosti, job "riusciti" con errori, messaggi interni esposti al client |
| **AP-16** `DataIngestionConfiguration.cs:31,34`; `FileProcessingCoordinator.cs:94` | Primitive Obsession | Bassa | Stati e tipi come stringhe (`"one-time"`, `"daily"`, `"filesystem"`/`"file system"`), 0 enum nel backend |
| **AP-17** `DataIngestionBackgroundService.cs:13,28,100-101`; `MongoRepository.cs:34`; `IndexSuggestionHelper.cs:23` | Magic Numbers | Bassa | Intervalli, limiti e campioni non configurabili né documentati |
| **AP-18** `DapperRepository.cs`; `IXmlEnrichmentRule`; `XmlProcessingService.cs:43` | Dead code / Speculative generality | Bassa | Rumore, falsa percezione di funzionalità disponibili |
| **AP-19** AG Grid + Tabulator; `vue-grid-layout-v3` + `grid-layout-plus`; `nuxt`; Newtonsoft + System.Text.Json | Librerie duplicate / dipendenze inutilizzate | Bassa | Bundle più pesante, superficie di vulnerabilità più ampia |
| **AP-20** `LossPrevention.API/Program.cs:42-47` | Service Locator | Bassa | `BuildServiceProvider()` durante la configurazione; dipendenze nascoste |

## 🔍 ANALISI DETTAGLIATA (Top 3 Problemi Critici)

Per l'analisi dettagliata sono stati scelti i tre finding critici con il maggiore impatto combinato su sicurezza e manutenibilità (AP-01, AP-02, AP-03). AP-04 è descritto nel catalogo (Appendice B) e si risolve insieme ad AP-01.

### 1. Trust boundary violation / Golden Hammer in `GetReportDataEndpoint.cs`

**Dove:** `LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs`, righe 66-71 (`HandleAsync`); esecuzione in `LossPrevention.Application/Services/Data/ReportDataservice.cs:25-86`.

**Analisi:**
Il server delega al client la costruzione della query MongoDB e la esegue senza validazione, convertendo ogni stage con `BsonDocument.Parse`. Viola *least privilege* e *never trust the client* (OWASP A03 Injection, CWE-943):
- un utente con `CAN_VIEW_REPORT` può usare `$lookup`/`$unionWith` per leggere `Users` (hash/salt), `PasswordResetTokens` e `DataIngestionConfigurations` (password SFTP);
- stage pesanti senza limiti causano DoS;
- il server non può anteporre il filtro di lock dell'utente, e la cache (chiave = hash della request, `:92-97`) non distingue gli utenti.

Gli stage di scrittura `$out`/`$merge` non vanno a buon fine: il server accoda sempre `$count` oppure `$skip`/`$limit` (`ReportDataservice.cs:40-52`, con `Take > 0` obbligatorio a `:45-50` dell'endpoint), e `$out`/`$merge` devono essere l'ultimo stage. È anche un *Golden Hammer*: la pipeline di aggregazione diventa l'unico linguaggio di query, persino per l'interfaccia di un form.

**Codice Attuale (Snippet):**
```csharp
// GetReportDataEndpoint.cs:66-71
var pipeline = req.QueryPipeline
                  .Where(x => x != null)
                  .Select(x => BsonDocument.Parse(x.ToString()))
                  .ToArray();

var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
```

**Soluzione Proposta:**
Il client invia il modello di query che già possiede (`Tab.SelectedFields`, `Query.Conditions`, `GroupByField`). Un `ReportQueryBuilder` server-side:
1. valida i campi contro `Mappings`;
2. ammette solo operatori noti;
3. antepone il filtro di lock letto dai claim;
4. impone `$limit` e `maxTimeMS`.

Stage come `$lookup`, `$unionWith`, `$out`, `$merge`, `$function`, `$where` e `$accumulator` non sono generabili. La chiave di cache include utente e lock. Migrazione con feature flag e confronto dei risultati sui workspace salvati.

**Esempio Refactoring:**
```csharp
public sealed record ReportQuery(IReadOnlyList<FieldDTO> Fields, QueryDTO Filter, string? GroupBy, int Skip, int Take);

public BsonDocument[] Build(ReportQuery q, ClaimsPrincipal user, IReadOnlySet<string> knownFields)
{
    var stages = new List<BsonDocument>();
    var lockField = user.FindFirst("LockField")?.Value;
    var lockValue = user.FindFirst("LockValue")?.Value;
    if (lockField is not null && knownFields.Contains(lockField))
        stages.Add(new("$match", new BsonDocument(lockField, lockValue)));
    stages.Add(new("$match", ConditionTranslator.Translate(q.Filter, knownFields))); // whitelist: $eq,$ne,$gt,$gte,$lt,$lte,$in, $regex con escape
    if (q.GroupBy is not null) stages.Add(GroupStage(q, knownFields));
    stages.Add(new("$project", Projection(q.Fields, knownFields)));
    return stages.ToArray(); // $skip/$limit e maxTimeMS aggiunti dal servizio
}
```

### 2. God Object / The Blob in `aiStore.ts`

**Dove:** `LossPrevention.UI/src/stores/aiStore.ts`: intero file (1.994 righe, 99 funzioni). Hotspot:
- `buildGenericFraudRuleTemplates`, righe 1203-1530, CCN 87;
- `enrichWithCrossTransactionMetrics`, righe 986-1142, CCN 43;
- `analyzeFraudInData`, righe 1667-1922, CCN 34;
- compilazione delle regole con `new Function`, righe 1768-1774.

**Analisi:**
Lo store viola SRP: contiene client LLM (`:8`, `:88`), NLQ, classificazione semantica dei campi via regex (`SEMANTIC_PATTERNS`, `:111`), statistica descrittiva, generazione delle regole antifrode, compilazione ed esecuzione, stato UI e timer.

Le regole sono stringhe JavaScript costruite interpolando i **nomi dei campi** dei dati (`row["${f}_total"]`) e compilate con `new Function`:
- non sono tipizzate né testabili;
- non sono riusabili dal backend;
- un nome di campo costruito ad arte nei file ingeriti (es. contenente `"]`) diventa codice eseguito nel browser (CWE-95).

Viola anche KISS e DRY rispetto al motore regole del backend (AP-06).

**Codice Attuale (Snippet):**
```ts
// aiStore.ts:1224-1229 (dentro buildGenericFraudRuleTemplates)
rules.push({
  fraudType: "High Value Transaction",
  fields: [f],
  condition: `row["${f}_total"] > ${p95}`,
  severity: "medium",
});

// aiStore.ts:1768-1774 (dentro analyzeFraudInData)
const compiled = rules.map((rule, index) => {
  const fn = new Function(
    "row",
    `try { return (${rule.condition}) } catch { return false }`
  );
  return { ...rule, fn, index: index + 1 };
});
```

**Soluzione Proposta:**
1. Introdurre un **modello dichiarativo di regola** (`{ field, op, threshold }`) valutato da un interprete con operatori in whitelist.
2. Scomporre lo store in moduli puri e testabili:
   - `llmClient`, dietro un'interfaccia (vedi AP-13);
   - `fieldClassifier`;
   - `statistics`;
   - `ruleGenerator`;
   - `ruleEvaluator`.
3. Portare `computeStats` e i `build*Rules` in un servizio C# (`FraudAnalysisService`) eseguito come job, con risultati persistiti e auditabili.
4. Lasciare nello store solo lo stato UI e le chiamate API.

Prima del refactoring servono test di caratterizzazione (snapshot delle regole generate su un dataset fisso).

**Esempio Refactoring:**
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

**Dove:** `LossPrevention.UI/src/components/reports/resultsGrid.vue`, righe 1360-1382 (`valueGetter` dei campi calcolati; `eval` a riga 1377), in un componente di 2.271 righe. Il componente gestisce anche griglia, export, drill-down, lock (`:1687`) e analisi antifrode.

**Analisi:**
L'espressione del campo calcolato è scritta da un utente e salvata nel workspace, che è condivisibile. Quando un altro utente apre il report, `eval` la esegue nel suo browser con accesso al JWT in `localStorage` (`loginStore.ts:92`): è un XSS persistente (CWE-95).

Il replace a riga 1371 sostituisce solo gli identificatori che coincidono con chiavi della riga e lascia passare tutto il resto, per esempio `fetch(...)` o `localStorage`. La dimensione del file viola SRP: ogni modifica alla griglia rischia regressioni su funzioni non correlate.

**Codice Attuale (Snippet):**
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

**Soluzione Proposta:**
- Sostituire `eval` con un parser di espressioni aritmetiche con whitelist (es. `expr-eval` con operatori logici, assegnazione e definizione di funzioni disabilitati, oppure `jsep` + interprete), compilando una volta per campo.
- Validare le espressioni anche lato server al salvataggio del workspace.
- Scomporre `resultsGrid.vue` in:
  - `useGridColumns` (composable);
  - `useCalculatedFields`;
  - `useExport`;
  - `FraudOverlay`, componente che consuma i risultati del backend (AP-02).
- Aggiungere la regola ESLint `no-eval`.

**Esempio Refactoring:**
```ts
import { Parser } from 'expr-eval';

const parser = new Parser({ operators: { logical: false, assignment: false, fndef: false } });
const compiled = parser.parse(expression);                  // una volta per campo calcolato
valueGetter: (params) => {
  const vars = numericFieldsOf(params.data);                // solo variabili numeriche note
  const result = compiled.evaluate(vars);
  return `${field.prefix ?? ''}${result.toFixed(2)}${field.suffix ?? ''}`;
}
```

## 🛠️ RACCOMANDAZIONI GENERALI & NEXT STEPS

- [ ] **Settimana 1**: eliminare `eval` (AP-03) e `new Function` (AP-02, interprete dichiarativo); bloccare la pipeline arbitraria con una whitelist provvisoria degli stage (AP-01).
- [ ] **Mesi 1-2**: Query DSL server-side con lock applicato dal server (AP-01, AP-04); motore antifrode unico nel backend con soglie persistite e audit (AP-06, AP-12); rule engine con update bulk (AP-05).
- [ ] Scrivere test di caratterizzazione, poi scomporre `aiStore.ts` e `resultsGrid.vue` (AP-02, AP-03, AP-14).
- [ ] Chiudere `Collection` del repository dietro metodi espliciti (`UpdateManyAsync`, `BulkWriteAsync`, `AggregateAsync`) e spostare la logica dei 20 endpoint nei servizi (AP-07, AP-09); mapping soglie unico (AP-10).
- [ ] Enum/value object per `ScheduleType`, `Recurrence`, `SourceType` e `FileType`; costanti/configurazione per intervalli e limiti (AP-16, AP-17).
- [ ] Gestione errori: `IExceptionHandler` + `ProblemDetails`, niente `ex.Message` al client, catch specifici (AP-15).
- [ ] Rimuovere dead code e librerie duplicate o inutilizzate: Dapper, `IXmlEnrichmentRule`, `grid-layout-plus`, `nuxt`, una delle due griglie (AP-18, AP-19); service locator in `Program.cs` (AP-20).
- [ ] Introdurre un adapter `LlmProvider` lato backend al posto della chiamata diretta a Ollama dal browser (AP-13).
- [ ] **Strumenti da integrare in CI**:
  - SonarQube/SonarCloud (C# + TS, quality gate su duplicazione < 5 % e nuove issue critiche = 0);
  - ESLint `plugin:vue/vue3-recommended` con `no-eval`, `no-new-func` e `no-implied-eval`;
  - analizzatori Roslyn con `TreatWarningsAsErrors` sui nuovi progetti;
  - ArchUnitNET (o NetArchTest) per vietare `IMongoRepository<>` e `MongoDB.Driver` negli endpoint e `MongoDB.Bson` nel Domain;
  - jscpd (soglia di duplicazione);
  - gitleaks (secret scanning).

---

## Appendice A — Checklist di analisi per fase

Esito di ogni controllo richiesto dal prompt (Fasi 1-4): ✅ = anti-pattern assente, ⚠️ = presente in forma parziale, ❌ = presente. Ogni voce riporta l'evidenza e il finding di riferimento.

### Fase 1 — Scansione iniziale e metriche

| Controllo | Risultato |
|---|---|
| Stack tecnologico | Backend: .NET 8, ASP.NET Core + FastEndpoints 6, MongoDB.Driver 3.4, FluentValidation, SSH.NET. Frontend: Vue 3 + Vite + Pinia + Vuetify, AG Grid, Tabulator, Chart.js, axios; LLM locale Ollama (`qwen2.5:14b`) chiamato dal browser |
| Struttura directory | `LossPrevention.API` (Endpoints per feature), `LossPrevention.Application` (Services, DTO, Handlers, Helpers, Validators), `LossPrevention.Domain` (Entities, Exceptions), `LossPrevention.Infrastructure` (Repositories, DI), `LossPrevention.DataIngestionService` (console), `LossPrevention.UI/src` (components, stores, helpers, api, router) |
| File sorgente | 232 C# + 41 Vue + 28 TS (in `LossPrevention.UI/src`) |
| Righe non vuote | Backend 10.760; frontend 15.836 |
| Classi / funzioni | Backend 264 classi (0 enum), 475 funzioni; frontend 569 funzioni (lizard) |
| God file (> 500 righe) | `resultsGrid.vue` 2.271 · `aiStore.ts` 1.994 · `selectFields.vue` 823 · `fraudDetectionStore.ts` 728 · `queryBuilderTabs.vue` 694 · `FraudSettingsDialog.vue` 595 · `dataingestion.vue` 567 · `notifications.vue` 562; nessun file C# supera le 500 righe (max `MappingService.cs` 428, `FileProcessingCoordinator.cs` 424, `UpdateFraudDetectionSettingsEndpoint.cs` 407) |
| Funzioni CCN > 15 | 20 (6 BE + 14 FE); max 87 (`aiStore.ts:1203-1530`) |
| Duplicazione (jscpd) | BE 8,58 % (56 cloni, 1.052 righe); FE vedi [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) |

### Fase 2 — Analisi architetturale (macro-level)

**2.1 Big Ball of Mud & Spaghetti Architecture**
- ✅ Struttura caotica senza layer: **assente**. Layer API/Application/Domain/Infrastructure riconoscibili; frontend organizzato per components/stores/helpers.
- ✅ Dipendenze circolari tra moduli principali: **assenti**.
  - Il grafo dei progetti .NET è aciclico (API → Application/Infrastructure, Application → Domain/Infrastructure, Infrastructure → Domain).
  - Tra gli store Pinia ci sono solo dipendenze unidirezionali (`aiStore` → `fraudDetectionStore`, `notificationStore` → `groupStore`).
  - Non è stata fatta un'analisi completa dei cicli di import tra componenti Vue (es. `madge`).
- ⚠️ Flussi non lineari: la catena ingestione → mapping → regole è in parte sincrona (`FileProcessingCoordinator.cs:105-108`) e in parte manuale (`GET /rules/apply`). L'analisi statistica avviene nel browser. Vedi AP-05 e AP-06.

**2.2 Stovepipe System (Silos)**
- ❌ Logica duplicata invece che condivisa:
  - due motori antifrode (regole min/max nel backend, statistica e regole generate nel frontend);
  - due pipeline di ingestione (API `FileProcessingCoordinator` e console `DataIngestionService/Program.cs:85-102`) con logiche diverse (batch vs singolo insert, regole ignorate). Vedi AP-06.
- ⚠️ Integrazione point-to-point: il frontend chiama direttamente Ollama su `http://localhost:11434` (`aiStore.ts:88`) senza passare dal backend (AP-13); per SFTP e SMTP le chiamate dirette sono adeguate alla scala del sistema.

**2.3 Golden Hammer**
- ❌ La pipeline di aggregazione MongoDB inviata dal client è usata come linguaggio di query universale (AP-01).
- ❌ Classificazione semantica dei campi basata su regex sui nomi (`SEMANTIC_PATTERNS`, `aiStore.ts:111`) e generazione di codice JavaScript come motore regole (AP-02, AP-13).

**2.4 Vendor Lock-in**
- ❌ MongoDB nel dominio: entità con 22 attributi `[Bson*]` e tipo `ObjectId` (es. `User.cs:1,7`); endpoint che usano `Builders<T>` del driver (es. `ListNotificationsEndpoint.cs:30`). Vedi AP-08.
- ❌ LLM Ollama hardcoded (modello `aiStore.ts:8`, URL `:88`), senza adapter (AP-13).
- ✅ Cloud: nessuna dipendenza da servizi cloud proprietari nel codice.

### Fase 3 — Analisi design pattern (class/module level)

**3.1 God Object / The Blob**
- ❌ `aiStore.ts` (99 funzioni, 1.994 righe) e `resultsGrid.vue` (2.271 righe) superano tutte le soglie (AP-02, AP-03).
- ⚠️ `fraudDetectionStore.ts` (29 funzioni, 728 righe) e `selectFields.vue` (823 righe).
- ⚠️ Backend: `MappingService` (428 righe, scoperta schema + CRUD + batch) e `FileProcessingCoordinator` (424 righe: orchestrazione, SFTP, parsing, persistenza, idempotenza) sono al limite; nessuna classe ha più di 20 metodi pubblici.
- ⚠️ Nomi generici: `BsonHelper`, `RuleHelper` e `DistanceHelper` sono helper statici. `IndexSuggestionHelper.cs` contiene la classe `IndexService`.

**3.2 Circular Dependency**
- ✅ Assente, vedi 2.1.

**3.3 Shotgun Surgery**
- ❌ Aggiungere o modificare una soglia antifrode richiede modifiche coordinate in **8 file**, in C# e TypeScript (AP-12):
  - `FraudDetectionSettings.cs:29`;
  - `FraudDetectionSettingsDTO.cs:15`;
  - `CreateFraudDetectionSettingsEndpoint.cs` (24 occorrenze);
  - `UpdateFraudDetectionSettingsEndpoint.cs` (24);
  - `GetFraudDetectionSettingsEndpoint.cs` (12);
  - `fraudDetectionStore.ts:10,177`;
  - `FraudSettingsDialog.vue:417`;
  - `aiStore.ts:1219`.

**3.4 Feature Envy**
- ❌ `AddMemberToGroupEndpoint.cs:56-63` manipola direttamente `GroupDocument.Members` e `UpdatedAtUtc` (logica che appartiene a `Group.AddMember`).
- ❌ `RuleHelper.EvaluateRule(doc, rule)` legge i campi di `RuleConfiguration` (comportamento che appartiene alla regola).
- ❌ `BsonHelper` lavora interamente sui dati di `BsonValue`/`MappingItem`.
- Vedi AP-11.

**3.5 Anemic Domain Model**
- ❌ 19 entità con sole proprietà e 0 metodi di comportamento; 2 eccezioni di dominio mai usate (`DeleteUserRoleException`, `InvalidCustomerException`); stati come stringhe (AP-11, AP-16).

### Fase 4 — Analisi codice (implementation level)

**4.1 Complessità e leggibilità**
- ❌ Spaghetti code / metodi lunghi:
  - 18 funzioni backend > 50 NLOC, di cui 5 metodi di mapping da 162 NLOC;
  - `buildGenericFraudRuleTemplates` con 254 NLOC e CCN 87;
  - `DistanceDataService.GetDistanceAsync` con 122 NLOC e CCN 20 (AP-14).
- ❌ Magic numbers/strings:
  - intervalli (`FromMinutes(1)`, `FromSeconds(10)`, 50 s);
  - limiti `.Take(20)`, campione 100, `ProcessMappings(1000)`/`int.MaxValue`, cache 1 h;
  - stringhe `"sftp"`/`"filesystem"`/`"file system"` (AP-16, AP-17).
- ❌ Deeply nested code: `ReportDataservice.cs:117-150` (8 livelli: if → foreach → if → if → foreach → else if → if → for); `MappingService.cs:269-288` (lambda async in `Parallel.ForEachAsync` → foreach → if → if) (AP-14).

**4.2 Violazioni DRY**
- ❌ Copy-paste programming:
  - FraudDetection Create/Update (341 righe);
  - `MapThresholdsToDTO` ripetuto in 3 endpoint;
  - 56 cloni backend in totale (AP-10).

**4.3 Gestione errori**
- ❌ Error swallowing:
  - 4 catch vuoti senza tipo (`BsonHelper.cs:221,280`, `XmlToBsonConverterHelper.cs:149`, `DashboardMapping.cs:89`);
  - errori SFTP registrati nel risultato ma con job completato (`SftpFileProcessingService.cs:107-122`);
  - frontend: 115 `catch`, 55 `console.error`, nessuna notifica centralizzata.
- ❌ Generic exceptions: 31 `catch (Exception)` nel backend; `ex.Message` restituito al client in 13 punti (AP-15).

**4.4 Primitive Obsession**
- ❌ `ScheduleType`, `Recurrence`, sorgenti e tipi file come stringhe; soglie e lock come primitive (`LockField`/`LockValue` stringhe in `User.cs:18-19`); `any` diffuso nel frontend (`buildGenericFraudRuleTemplates(classification: any, stats: any)`, `aiStore.ts:1203`) (AP-16).

---

## Appendice B — Catalogo completo dei finding

Ogni finding riporta location verificata (file:riga al commit `fc7d820`), evidenza, impatto, severità e refactoring raccomandato. AP-01…AP-03 sono dettagliati nella sezione "Analisi dettagliata".

### AP-04 — Security by obscurity: lock applicato solo dal client · **Critica**
- **Location**:
  - `LossPrevention.UI/src/helpers/fieldLock.ts:1-48`;
  - consumer in `resultsGrid.vue:1687`, `toolbar.vue:180`, `tabularBlock.vue:259` (quest'ultimo con mapping vuoti `[]`);
  - claim emessi in `LoginEndpoint.cs:65-69` e mai letti dal backend.
- **Evidenza**:
  ```ts
  // fieldLock.ts:2-8
  const token = localStorage.getItem('token');
  if (!token) return undefined;
  const payload = JSON.parse(atob(token.split('.')[1]));
  if (!payload.LockField || payload.LockValue == null) return undefined;
  ```
- **Impatto**: il filtro per negozio/area esiste solo nella query costruita dal browser. Un utente può rimuoverlo (DevTools o chiamata diretta all'API) e leggere tutti i dati (CWE-602, OWASP API1 BOLA).
- **Refactoring**: applicare il lock server-side come primo `$match` nel Query DSL (AP-01), in distance ed export; mantenere `fieldLock.ts` solo per la UX (mostrare il filtro bloccato).

### AP-05 — Leaky abstraction + N+1 writes nel rule engine · **Alta**
- **Location**: `LossPrevention.Application/Services/Data/Rules/RulesService.cs:34-58` (`ApplyRulesAsync`), `:141-148` (`UpdateRuleAsync`).
- **Evidenza**:
  ```csharp
  var docs = (await _reportDataRepository.GetAllAsync()).ToList();                    // :34 full scan
  await _reportDataRepository.Collection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, unsetUpdate); // :41 rimuove TUTTI i flag
  await _reportDataRepository.Collection.ReplaceOneAsync(filter, doc);                // :58 una scrittura per documento
  var docUpdate = Builders<BsonDocument>.Update.Set($"{rule.RuleName}", isMatch);     // :147 path errato (radice, non FraudFlags.*)
  ```
- **Impatto**:
  - memoria e tempo O(N) con N round-trip;
  - durante il ricalcolo tutti i documenti restano senza flag (lo unset non è atomico rispetto alla riscrittura);
  - dopo `UpdateRuleAsync` i flag sono incoerenti.
- **Refactoring**: una `UpdateMany` con pipeline update per regola (`$set: { "FraudFlags.<rule>": <espressione $and/$gte/$lte> }`), esecuzione come job asincrono post-ingestione e repository con metodi espliciti al posto di `Collection`.

### AP-06 — Stovepipe / split-brain della logica antifrode · **Alta**
- **Location**:
  - BE: `RuleHelper.EvaluateRule` (usato in `RulesService.cs:50`), console `DataIngestionService/Program.cs:38-47` (regole caricate e ignorate), `:85-102`;
  - FE: `aiStore.ts:1203-1530` e `fraudDetectionStore.ts:177`.
- **Evidenza**:
  ```ts
  // fraudDetectionStore.ts:177-181 — unica sorgente dei default delle soglie
  const DEFAULT_FRAUD_THRESHOLDS: FraudThresholdConfig = {
    highValue: {
      percentile: 0.95,
      stdDevMultiplier: 2,
      minimumValue: 500,
  ```
- **Impatto**:
  - le decisioni antifrode dipendono dal browser e dalla pagina di dati caricata, quindi non sono riproducibili, auditabili né eseguibili in batch;
  - backend e frontend usano modelli di regola diversi;
  - due pipeline di ingestione divergenti.
- **Refactoring**:
  - un solo `FraudAnalysisService` nel backend, con soglie persistite (default server-side) e risultati salvati con versione della regola;
  - il frontend visualizza soltanto;
  - la console riusa `FileProcessingCoordinator`.

### AP-07 — Leaky abstraction: `Collection` pubblica nel repository · **Media**
- **Location**: `LossPrevention.Infrastructure/Repositories/MongoRepository.cs:27`; usi in `RulesService.cs:41,58,148` e `FileProcessingCoordinator.cs:390-392,408-410`.
- **Evidenza**:
  ```csharp
  public IMongoCollection<TDocument> Collection => _collection;          // MongoRepository.cs:27
  await _reportDataRepository.Collection.Database
      .GetCollection<BsonDocument>("ProcessedFiles")                      // FileProcessingCoordinator.cs:390-391
  ```
- **Impatto**: il repository non incapsula nulla. I servizi usano il driver direttamente e raggiungono altre collection (`ProcessedFiles` non configurata), rendendo impossibili mock e cambi di persistenza.
- **Refactoring**: rendere `Collection` `internal`/privata; aggiungere `UpdateManyAsync`, `BulkWriteAsync` e `AggregateAsync` tipizzati; creare un `IProcessedFileRepository` dedicato.

### AP-08 — Vendor lock-in MongoDB nel dominio / inversione delle dipendenze · **Media**
- **Location**: `LossPrevention.Application.csproj` (ProjectReference → Infrastructure); `LossPrevention.Domain/Entities/**` (22 attributi `[Bson*]`, `ObjectId` in `User.cs:1,7,16`); `Builders<T>` negli endpoint (es. `ListNotificationsEndpoint.cs:30`).
- **Evidenza**:
  ```csharp
  using MongoDB.Bson;                    // User.cs:1
  public ObjectId _id { get; set; }      // User.cs:7
  ```
- **Impatto**: il dominio dipende dal driver. Application dipende da Infrastructure invece del contrario, quindi test e sostituzione della persistenza costano di più.
- **Refactoring**:
  - spostare le interfacce dei repository in Application/Domain e invertire la reference;
  - usare id `string` nel dominio e mapping con `BsonClassMap` in Infrastructure;
  - regola ArchUnitNET che vieta `MongoDB.*` nel Domain e negli endpoint.

### AP-09 — Fat endpoint / layer skipping · **Media**
- **Location**: 20 endpoint che iniettano `IMongoRepository<T>`, es. `ListNotificationsEndpoint.cs:30`, `CreateGroupEndpoint.cs:34`, `CreateDashboardEndpoint.cs:35`, `CreateFraudDetectionSettingsEndpoint.cs:44`.
- **Evidenza**:
  ```csharp
  var filter = Builders<NotificationDocument>.Filter.Empty;   // ListNotificationsEndpoint.cs:30 — query, nessun filtro per destinatario
  ```
- **Impatto**: logica di business e di autorizzazione negli endpoint, non riusabile e non testabile in isolamento. Nell'esempio, la mancanza del filtro per destinatario è un bug di sicurezza.
- **Refactoring**: servizi applicativi per Groups, Notifications, Dashboard e FraudSettings; gli endpoint si limitano a binding, validazione e mapping HTTP.

### AP-10 — Copy-paste programming · **Media**
- **Location**:
  - `CreateFraudDetectionSettingsEndpoint.cs:47-388` ≈ `UpdateFraudDetectionSettingsEndpoint.cs:66-407` (341 righe, jscpd);
  - `MapThresholdsToDTO` in Create `:226-387`, Get `:60-221`, Update `:245-406`;
  - `MapToEntity` in Create `:50-211` e Update `:69-230`.
- **Evidenza**: 5 metodi da 162 NLOC ciascuno, con CCN 1 (mapping campo per campo).
- **Impatto**: ogni nuova soglia va aggiunta 5 volte (alimenta AP-12); divergenze silenziose tra create e update.
- **Refactoring**: un solo mapper (es. Mapperly, source generator, oppure un `FraudSettingsMapper` statico) usato dai 3 endpoint; soglie come dizionario tipizzato o value object.

### AP-11 — Anemic Domain Model + Feature Envy · **Media**
- **Location**: `LossPrevention.Domain/Entities/**` (19 entità senza metodi); `AddMemberToGroupEndpoint.cs:56-63`.
- **Evidenza**:
  ```csharp
  if (!existing.Members.Contains(userOid))
  {
      existing.Members.Add(userOid);
      existing.UpdatedAtUtc = DateTime.UtcNow;
      var updateDefinition = Builders<GroupDocument>.Update
          .Set(g => g.Members, existing.Members)       // riscrive l'intera lista
  ```
- **Impatto**: invarianti sparse in endpoint e helper. Il read-modify-write senza concorrenza ottimistica causa lost update con richieste concorrenti.
- **Refactoring**: `Group.AddMember(userId)` nel dominio; persistenza con `$addToSet` atomico; `Rule.Evaluate(doc)` al posto di `RuleHelper.EvaluateRule`.

### AP-12 — Shotgun Surgery sulle soglie antifrode · **Media**
- **Location**: 8 file (vedi Fase 3.3): `FraudDetectionSettings.cs:29`, `FraudDetectionSettingsDTO.cs:15`, 3 endpoint FraudDetection, `fraudDetectionStore.ts:10,177`, `FraudSettingsDialog.vue:417`, `aiStore.ts:1219`.
- **Evidenza**: `highValue`/`HighValue` compare in 8 file e 75 righe tra C# e TypeScript.
- **Impatto**: modifiche lente e rischiose; facile dimenticare uno dei punti (es. il default esiste solo nel frontend).
- **Refactoring**: schema di soglie data-driven (lista di `ThresholdDefinition` con chiave, tipo e default) servito dal backend; UI generata dallo schema; mapper unico (AP-10).

### AP-13 — Vendor/runtime lock-in Ollama + Golden Hammer regex · **Media**
- **Location**: `LossPrevention.UI/src/stores/aiStore.ts:8` (modello), `:88` (URL), `:111` (`SEMANTIC_PATTERNS`).
- **Evidenza**:
  ```ts
  const MODEL = "qwen2.5:14b";                                      // :8
  const res = await fetch("http://localhost:11434/api/generate", {  // :88
  export const SEMANTIC_PATTERNS: Record<string, RegExp[]> = {      // :111
  ```
- **Impatto**:
  - ogni postazione richiede Ollama locale;
  - cambiare provider o modello richiede una release del frontend;
  - nessun controllo di accesso, quota o logging sulle richieste LLM;
  - la classificazione dei campi dipende dai nomi usati dal POS.
- **Refactoring**: endpoint backend `/ai/*` dietro un'interfaccia `ILlmProvider` (Ollama, Azure OpenAI…) configurabile; classificazione basata sui `Mappings` (tipi dichiarati) con regex solo come fallback.

### AP-14 — Spaghetti / deeply nested code · **Media**
- **Location**:
  - `aiStore.ts:1203-1530` (CCN 87), `:986-1142` (CCN 43), `:1667-1922` (CCN 34);
  - `dataIngestionStore.ts:164-228` (CCN 36);
  - `UpdateDataIngestionConfigurationEndpoint.cs:23-101` (CCN 24);
  - `ReportDataservice.cs:126-150`;
  - `MappingService.cs:269-288`.
- **Evidenza**:
  ```csharp
  // ReportDataservice.cs:126-146 — livelli 3-8 di 8 (si parte da if :117 e foreach :121)
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
- **Impatto**: 20 funzioni con CCN > 15 sono quasi impossibili da coprire con test e ad alto rischio di regressione.
- **Refactoring**: guard clause ed early return; estrazione di metodi (`CoerceOperatorDocument`, `CoerceArray`); tabelle di strategie al posto delle catene `if` nel generatore di regole; limite CCN ≤ 15 nel quality gate.

### AP-15 — Error swallowing / generic exceptions · **Media**
- **Location**:
  - BE: 31 `catch (Exception)`; catch vuoti in `BsonHelper.cs:221,280`, `XmlToBsonConverterHelper.cs:149`, `DashboardMapping.cs:89`; `SftpFileProcessingService.cs:107-122`; `ex.Message` al client (es. `GetReportDataEndpoint.cs:87`);
  - FE: 115 `catch`, 55 `console.error`.
- **Evidenza**:
  ```csharp
  try { result = Convert.ToDecimal(v.AsDouble, CultureInfo.InvariantCulture); return true; }
  catch { /* avoid throw */ }                                   // BsonHelper.cs:220-221
  AddError("query", "Invalid query pipeline: " + ex.Message);  // GetReportDataEndpoint.cs:87
  ```
- **Impatto**: bug mascherati (conversioni fallite in silenzio), job di ingestione "completati" con errori, informazioni interne esposte al client (CWE-209).
- **Refactoring**: catch di eccezioni specifiche (`OverflowException`, `MongoException`, `SshException`); `IExceptionHandler` + `ProblemDetails` globali; esito del job `PartialSuccess` con metriche; nel frontend, interceptor axios e notifiche centralizzate.

### AP-16 — Primitive Obsession · **Bassa**
- **Location**: `DataIngestionConfiguration.cs:31,34`; `FileProcessingCoordinator.cs:94`; `UpdateDataIngestionConfigurationEndpoint.cs:37,45`; `User.cs:18-19`; `aiStore.ts:1203`.
- **Evidenza**:
  ```csharp
  public string ScheduleType { get; set; } = "one-time"; // "one-time" | "recurring"   (:31)
  public string Recurrence { get; set; } = "daily";      // "daily" | "weekly"          (:34) — ma "monthly" è gestito in DataIngestionBackgroundService.cs:140
  if (normalizedSource == "filesystem" || normalizedSource == "file system")          // FileProcessingCoordinator.cs:94
  ```
- **Impatto**: valori non validati (il commento del modello non include `monthly`, che invece è gestito); confronti fragili.
- **Refactoring**: enum `ScheduleType`, `Recurrence` e `SourceType` con serializzazione stringa (`[BsonRepresentation(BsonType.String)]`/`JsonStringEnumConverter`); value object `RowLock(Field, Value)`; tipi TypeScript al posto di `any`.

### AP-17 — Magic Numbers · **Bassa**
- **Location**:
  - `DataIngestionBackgroundService.cs:13` (`FromMinutes(1)`), `:28` (`FromSeconds(10)`), `:100-101` (50 s);
  - `MongoRepository.cs:34` (`.Take(20)`);
  - `IndexSuggestionHelper.cs:23` (campione 100);
  - `RulesService.cs:78` (`ProcessMappings(1000)`);
  - `GetReportDataEndpoint.cs:81` (cache 1 h).
- **Evidenza**:
  ```csharp
  var now = DateTime.Now;                                                    // :97 ora locale
  if (config.LastRunAt.HasValue && (now - config.LastRunAt.Value).TotalSeconds < 50)  // :100-101, LastRunAt impostato in UTC (:86) e mai persistito (commento :87)
  ```
- **Impatto**: comportamento non configurabile. Nell'esempio, il numero magico si combina con un confronto tra ora locale e UTC che rende inaffidabile la guardia anti-doppia esecuzione sui server con fuso diverso da UTC.
- **Refactoring**: `IngestionSchedulerOptions`, `IndexingOptions` e `ReportCacheOptions` via `IOptions<T>`; `TimeProvider`/`DateTime.UtcNow` ovunque.

### AP-18 — Dead code / Speculative generality · **Bassa**
- **Location**:
  - `LossPrevention.Infrastructure/Repositories/DapperRepository.cs` (205 righe commentate);
  - `IXmlEnrichmentRule` (0 implementazioni);
  - `XmlProcessingService.cs:40-44`;
  - `GetReportDataEndpoint.cs:64` (`GetAllMappings()` inutilizzato);
  - `DataIngestionService/Program.cs:24-25` (costanti non usate);
  - eccezioni di dominio mai lanciate;
  - cartella `Endpoints\Login\` vuota nel csproj.
- **Evidenza**:
  ```csharp
  public Task InsertManyAsync(IEnumerable<BsonDocument> docs)
  {
      // This method is deprecated - FileProcessingCoordinator handles insertion now
      throw new NotImplementedException("Use FileProcessingCoordinator.RunIngestionAsync instead");   // XmlProcessingService.cs:43
  }
  ```
- **Impatto**: rumore e falsa percezione di funzionalità disponibili. L'interfaccia promette un metodo che lancia eccezione (viola LSP).
- **Refactoring**: rimozione (la cronologia resta in Git); separare l'interfaccia di parsing da quella di persistenza.

### AP-19 — Librerie duplicate / dipendenze inutilizzate · **Bassa**
- **Location**: `LossPrevention.UI/package.json`:
  - `ag-grid-community`/`ag-grid-vue3` (`:13-14`, usati in `resultsGrid.vue:504` e `tabularBlock.vue:44`) **e** `tabulator-tables` (`:26`, usato solo in `distanceTable.vue:25`);
  - `vue-grid-layout-v3` (`:28`, usato in `dashboardGrid.vue:84`) **e** `grid-layout-plus` (`:18`, mai importato);
  - `nuxt` (`:22`) e `@nuxt/devtools` (`:35`), mai usati in un progetto Vite.

  Backend: `Newtonsoft.Json` (3 file) e `System.Text.Json` (4 file).
- **Evidenza**: grep degli import in `LossPrevention.UI/src`: nessun import di `grid-layout-plus` né di `nuxt`.
- **Impatto**: bundle più pesante; superficie CVE più ampia (`nuxt` porta con sé molte dipendenze transitive vulnerabili, vedi doc 16); due API da conoscere per lo stesso compito.
- **Refactoring**: rimuovere `grid-layout-plus`, `nuxt` e `@nuxt/devtools`; migrare `distanceTable.vue` ad AG Grid; consolidare il backend su System.Text.Json.

### AP-20 — Service Locator in `Program.cs` · **Bassa**
- **Location**: `LossPrevention.API/Program.cs:42-47`.
- **Evidenza**:
  ```csharp
  var tempServices = new ServiceCollection();
  tempServices.AddInfrastructureServices(builder.Configuration);
  tempServices.AddRepositoryServiceCollection(builder.Configuration);
  using var tempProvider = tempServices.BuildServiceProvider();
  var settings = tempProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
  ```
- **Impatto**: un secondo container DI (con eventuali singleton duplicati) costruito solo per leggere una sezione di configurazione; il commento a riga 41 ("Load rules from MongoDB") non corrisponde al codice.
- **Refactoring**: `builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()`, oppure `IConfigureNamedOptions<JwtBearerOptions>`.

---

## Appendice C — Metodo

- **Strumenti**:
  - `lizard` (CCN, NLOC, funzioni) su C#, TS e Vue;
  - `jscpd` (duplicazione);
  - conteggio di righe e file con PowerShell;
  - grep/ripgrep per pattern (`eval`, `new Function`, `catch`, `Collection`, `Builders<`, import);
  - lettura manuale dei file citati.

  Tutti i numeri di riga sono verificati al commit `fc7d820`.
- **Limiti**:
  - analisi completa dei cicli di import tra componenti Vue non eseguita;
  - Maintainability Index non calcolato: N/A — non ricavabile dal codice senza analizzatori Roslyn/SonarQube.
- **Documentazione vendor storica**: presente solo nel commit `593f6de` e rimossa in `d768cd9` (`git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/<file>`, da `C:\repository\LOSS_PREVENTION`). Non usata come evidenza: tutti i finding derivano dal codice.

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

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Health Score ricalcolato con formula esplicita (52 → 45/100, 20 finding: 4 Critica, 2 Alta, 9 Media, 5 Bassa); tabella priorità con ID e file:riga verificati; analisi dettagliate con righe esatte (`eval` `resultsGrid.vue:1377`, `new Function` `aiStore.ts:1769`, pipeline `GetReportDataEndpoint.cs:66-71`) e rischio di injection via nomi di campo; checklist Fasi 1-4 con esito ed evidenza; catalogo AP-04…AP-20 con snippet, impatto e refactoring; correzioni: file TS 29 → 28, `queryBuilderTabs.vue` 693 → 694 e `dataingestion.vue` 566 → 567 righe; nuovi finding: unset massivo non atomico dei flag, confronto ora locale/UTC nello scheduler, lost update sui gruppi, `grid-layout-plus`/`nuxt` inutilizzati; riferimenti completi |
