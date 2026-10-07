<!-- REVERSE-META
schema: 1
mode: how
step: 07_code
commit: 593f6decec3a642d5567754dd795b19560bb0afb
commit_short: 593f6de
branch: main
worktree: clean
baseline_date: 2026-10-05T10:14:05+00:00
generated_at: 2026-10-07T14:40:00Z
-->
# Code - Progetto LOSS_PREVENTION

> **Baseline commit:** `593f6de` (`main`) · worktree `clean`

**Data**: 2026-10-07 · **Versione**: 1.0 · **Autori**: REVERSE how · **Audience**: team tecnico, architetti, developer

---

## 1. Convenzioni e organizzazione del codice

| Aspetto | Osservato |
|---------|-----------|
| Namespace | Non sempre allineati alle cartelle: `DataIngestionBackgroundService` (in Application) ha namespace `LossPrevention.Infrastructure.Services`; `DatabaseInitializationService` è in `Interfaces/Data`; `XmlEnrichmentService` è in `Services/Data` con namespace `Services.DataIngestion`; `RuleEvaluationRequest` ha namespace `LossPrevention.API.Handlers…` ma vive in Application |
| Naming | Mix `ReportDataservice` / `DistanceDataService` / `IDistanceDataservice`; proprietà `_id` sulle entità (stile Mongo) accanto a `Id` |
| Nomi progetto | csproj con prefissi numerici e spazi (`01. LossPrevention.API.csproj`) |
| Nullable | Abilitato, ma molte proprietà `string` non inizializzate nelle entità (warning soppressi di fatto) |
| Async | Uso diffuso di `async/await`; eccezioni: `client.Connect()`/`DownloadFile` avvolti in `Task.Run` |

## 2. Pattern implementativi chiave

### 2.1 Endpoint FastEndpoints
```csharp
// LossPrevention.API/Endpoints/Data/GetReportDataEndpoint.cs
public override void Configure()
{
    Post("/data/report/query");
    Permissions("CAN_VIEW_REPORT");
}
public override async Task HandleAsync(GetReportDataRequest req, CancellationToken ct)
{
    var cacheKey = GenerateCacheKey(req);                 // SHA256 di pipeline+skip+take
    if (_cache.TryGetValue(cacheKey, out GetReportDataResponse cached)) { await SendOkAsync(cached, ct); return; }
    var pipeline = req.QueryPipeline.Select(x => BsonDocument.Parse(x.ToString())).ToArray();  // ⚠️ input non validato
    var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
    _cache.Set(cacheKey, response, TimeSpan.FromHours(1));
}
```

### 2.2 Repository generico
`MongoRepository<TDocument>` (179 righe) espone CRUD, `AggregateAsync`, `CreateIndexesAsync` e **la proprietà `Collection`**, usata dai servizi per operazioni non coperte dall'astrazione. Registrazioni in `InfrastructureServiceExtensions.cs` (una factory per entità, `IMongoRepository<User>` registrato **due volte**).

### 2.3 Conversione XML → BSON
`XmlToBsonConverterHelper.ConvertFlattened` analizza l'XML con `XElement.Parse`, appiattisce la gerarchia e inferisce i tipi (bool/int/decimal/datetime/string). In .NET le impostazioni di default di `XElement.Parse` non risolvono entità esterne (no XXE) e limitano l'espansione delle entità; il rischio "XML bomb" è quindi **contenuto** (non critico come indicato nella run precedente), ma non esiste un limite esplicito sulla dimensione dei file.

### 2.4 Rule engine
```csharp
// LossPrevention.Application/Helpers/RuleHelper.cs
public static bool EvaluateRule(BsonDocument document, RuleConfiguration rule)
{
    var values = GetValuesByPath(document, rule.FieldPath);   // attraversa array
    if (rule.SumValues) return CompareValue(values.Where(v => v.IsNumeric).Sum(v => v.ToDecimal()), rule);
    return values.Any(v => CompareValue(v, rule));             // range numerico o uguaglianza case-insensitive
}
```
Funzione pura e testabile, ma espressività minima (1 campo, nessun operatore logico).

### 2.5 Similarity
`DistanceHelper.ComputeDistance` = distanza euclidea sui campi presenti in entrambi i record; `TryNormalize` converte solo il **tipo** (bool→0/1, liste→media) — **non** normalizza la scala. Score finale: `0.5 · campiUsati/campiRichiesti + 0.5 · (1 − d/dmax)`; minimo 3 campi in comune; top 10.

### 2.6 Motore statistico client-side
`aiStore.ts`: `classifyFields` → `buildSemanticFieldMap` (regex su nomi campo) → `computeStats` (percentili sul campione ordinato, media, σ di popolazione) → `build*Rules` (condizioni stringa tipo `row["Amount_total"] > 812.5`) → `new Function` → valutazione per transazione raggruppata.

### 2.7 Ingestione
`FileProcessingCoordinator` seleziona il processor per `SelectedFileType`; per SFTP il file viene **scaricato e parsato due volte** (una in `SftpFileProcessingService` per decidere processed/failed, scartando il risultato, e una nel coordinator per inserirlo) e a ogni esecuzione viene rielencata l'intera cartella `processed/`.

## 3. Hotspot di complessità (lizard, CCN > 15)

| Funzione | File | NLOC | CCN |
|----------|------|------|-----|
| `buildGenericFraudRuleTemplates` | `UI/src/stores/aiStore.ts:1203` | 254 | **87** |
| `enrichWithCrossTransactionMetrics` | `aiStore.ts:986` | 124 | 43 |
| `save` | `UI/src/stores/dataIngestionStore.ts:164` | 55 | 36 |
| `analyzeFraudInData` | `aiStore.ts:1667` | 131 | 34 |
| `buildPipeline` | `UI/src/components/dashboard/chartblock.vue:69` | 44 | 28 |
| `HandleAsync` | `API/Endpoints/DataIngestion/UpdateDataIngestionConfigurationEndpoint.cs:23` | 68 | 24 |
| `ConvertToMappedType` / `TryToDateTimeUtc` | `Application/Helpers/BsonHelper.cs` | 35 / 66 | 21 / 21 |
| `buildTypeAwareCondition` | `UI/src/helpers/queryUtils.ts:63` | 43 | 21 |
| `classifyFields` | `aiStore.ts:379` | 61 | 21 |
| `GetDistanceAsync` | `Application/Services/Data/DistanceDataservice.cs:22` | 122 | 20 |

## 4. Difetti funzionali individuati nel codice

| # | Difetto | Posizione | Effetto |
|---|---------|-----------|---------|
| D1 | `CreateTransactionEndpoint` non inserisce i documenti e legge `bson["_id"]` mai impostato | `Endpoints/Data/CreateTransactionEndpoint.cs` + `XmlProcessingService.ProcessAsync` | Eccezione o nessuna persistenza (da confermare a runtime) |
| D2 | Token JWT restituito come `Task` serializzato | `LoginEndpoint.HandleAsync` (manca `await`) | Il FE dipende da `token.result` |
| D3 | `UpdateRuleAsync` scrive `Set($"{rule.RuleName}", …)` alla radice e cerca la mapping `Name == RuleName` (senza `FraudFlags.`) | `RulesService.cs` | Flag duplicati in radice; rinomina regola non aggiorna la mapping |
| D4 | `ApplyRulesAsync` cancella tutte le mapping che contengono "FraudFlags" e le rigenera campionando 1.000 documenti | `RulesService.cs` | Alias/visibilità personalizzati sui flag vengono persi |
| D5 | `LastRunAt` non persistito | `DataIngestionBackgroundService` | Possibile doppia esecuzione; nessuno storico |
| D6 | `GET /notifications` senza filtro destinatario | `ListNotificationsEndpoint` | Ogni utente vede le notifiche di tutti |
| D7 | `DistanceDataService`: `keyValue.AsString` | `DistanceDataservice.cs` | Eccezione se il key field non è stringa |
| D8 | `ReportDataservice` filtro visibilità sempre vero (`… || !visibleFields.Contains(kv.Key)`) | `ReportDataservice.cs` | I campi "non visibili" vengono comunque restituiti |
| D9 | Statistiche antifrode calcolate solo sulla prima pagina (≤ 3.000 righe) e messe in cache | `aiStore.generateFraudRules` | Soglie dipendenti dall'ordinamento del report |

D8 in dettaglio:
```csharp
else if (visibleFields.Contains(kv.Key) || kv.Key == "count" || !visibleFields.Any() || !visibleFields.Contains(kv.Key))
```
La condizione è una tautologia (`A || !A`).

## 5. Codice morto / residui

| Elemento | Evidenza |
|----------|----------|
| `DapperRepository.cs`, `IDapperRepository.cs` | 204 + 21 righe interamente commentate |
| `XmlProcessingService.InsertManyAsync` | `throw new NotImplementedException` |
| `IXmlEnrichmentRule` | Nessuna implementazione registrata → `XmlEnrichmentService.Enrich` non fa nulla |
| Console `DataIngestionService` STEP 1 | Carica le regole e non le usa ("Just load rules") |
| `nuxt.config.ts`, dipendenze `nuxt`, `jspdf`, `jspdf-autotable`, `grid-layout-plus`, `vuedraggable` | Non referenziate dal codice |
| Blocco commentato in `DistanceDataservice` (conteggi `*Count`) | |
| `Mappings_old.bson`, `ReportData_old.bson` nel dump | |

## 6. Gestione errori e logging

- 31 `catch (Exception …)` generici nel backend; molti endpoint restituiscono `ex.Message` al client (es. "Invalid query pipeline: " + messaggio driver) → information disclosure.
- 6 `Console.WriteLine` nel backend (es. "Target not found." in `DistanceDataService`).
- 57 `console.log` nel frontend, anche con dati (mappe semantiche, conteggi).
- Nessun middleware globale di eccezioni/ProblemDetails personalizzato.

## 7. Sicurezza a livello di codice

| Pattern | Posizione | Rischio |
|---------|-----------|---------|
| `eval(expr)` su espressioni dei campi calcolati | `resultsGrid.vue:1377` | XSS persistente: un'espressione salvata nel workspace viene eseguita nel browser di chi apre il report |
| `new Function(...)` su condizioni generate | `aiStore.ts:1769` | Basso se le condizioni sono generate internamente; i nomi campo provengono dai dati importati (iniezione possibile via nomi campo malevoli nei file) |
| `BsonDocument.Parse` su input utente | `GetReportDataEndpoint.cs` | Query injection / data exfiltration |
| Token in `localStorage` | `api.ts`, `loginStore.ts` | Esfiltrabile da qualsiasi XSS (vedi `eval`) |
| Hash password nei DTO | `GetUsersEndpoint.cs` | Offline cracking |

---

## Reference Documents
- 00_deep_dive.md · 05_principles.md · 06_software_architecture.md

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
