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
# Code - Progetto LOSS_PREVENTION

> **Baseline commit:** `fc7d820` (`main`) · worktree `dirty` (modifiche locali solo in `Docs/` e `.github/`; codice applicativo identico al commit)

**Data**: 2026-10-08  
**Versione**: 1.1  
**Autori**: IMPACT REVERSE how (verifica 2026-10-08)  
**Audience**: Team tecnico, architetti, developer

---

## Sezioni Principali

1. [Code Organization & Structure](#1-code-organization--structure)
2. [Key Implementation Patterns](#2-key-implementation-patterns)
3. [Framework Usage (MVC, ORM, DI)](#3-framework-usage-mvc-orm-di)
4. [Security Implementation](#4-security-implementation)
5. [Exception Handling & Logging](#5-exception-handling--logging)
6. [Transaction Management](#6-transaction-management)
7. [Configuration Management](#7-configuration-management)
8. [Testing Strategy](#8-testing-strategy)

Appendici: [A. Hotspot di complessità](#a-hotspot-di-complessità) · [B. Difetti funzionali](#b-difetti-funzionali-individuati-nel-codice) · [C. Codice morto](#c-codice-morto--residui)

Convenzioni: percorsi relativi a `customspa-it-loss-prevention-d098a8b97860/`; `file:N` = riga al commit `fc7d820`. Conteggi righe = righe non vuote, salvo diversa indicazione.

---

## 1. Code Organization & Structure

**Sintesi.** Il backend è organizzato per progetto/layer e, all'interno, per area funzionale (Users, Data, DataIngestion, Rules, Workspaces…); il frontend per tipo di artefatto (components, stores, helpers). Le convenzioni di naming e namespace non sono uniformi.

### 1.1 Struttura backend (file C#, esclusi `bin/obj`)

| Progetto | Cartella | File | Contenuto |
|----------|----------|------|-----------|
| API (79 file, 4.347 righe) | `Program.cs` | 1 | Composition root: config, CORS, JWT, DI, pipeline |
| | `Endpoints/` | 78 | Una classe FastEndpoints per endpoint: Dashboard 5, Data 3, DataIngestion 8, FraudDetection 3, Groups 7, Mappings 4, Notifications 5, Rules 6, User/Permissions 10, User/Roles 10, User/Users 12, Workspaces 5 |
| Application (121 file, 5.005 righe) | `Handlers/` | 52 | Request/response model degli endpoint |
| | `DTO/` | 21 | DTO di risposta |
| | `Services/` | 20 | Servizi applicativi (Data, Data/Rules, DataIngestion, Users, Workspaces) |
| | `Interfaces/` | 17 | Interfacce servizi (+ `DatabaseInitializationService.cs`, classe concreta collocata qui) |
| | `Helpers/` | 6 | `BsonHelper`, `RuleHelper`, `DistanceHelper`, `XmlToBsonConverterHelper`, `JsonHelper`, `MappingHelper` |
| | `Mappings/` | 3 | Mapping statici Dashboard/Group/Notification ↔ DTO |
| | `Validators/` | 2 | FluentValidation Dashboard |
| Domain (21 file, 519 righe) | `Entities/` | 19 | Entità POCO con attributi BSON |
| | `Exceptions/` | 2 | `DeleteUserRoleException`, `InvalidCustomerException` (mai usate) |
| Infrastructure (10 file, 784 righe) | root | 1 | `InfrastructureServiceExtensions.cs` (DI Mongo + repository) |
| | `Configuration/` | 2 | `MongoDbSettings`, `JwtSettings` |
| | `Repositories/` | 4 | `IMongoRepository`, `MongoRepository` (179 righe), `IDapperRepository`, `DapperRepository` (205 righe, interamente commentato) |
| | `Helpers/`, `Services/`, `Interfaces/` | 3 | `PasswordHasher`, `EmailService`, `IEmailService` |
| DataIngestionService (1 file, 105 righe) | `Program.cs` | 1 | Console di bulk load |

### 1.2 Struttura frontend (`LossPrevention.UI/src`)

| Cartella | File | Note |
|----------|------|------|
| `components/` | 40 | SFC per area (`reports/`, `dashboard/`, `manage/`, …); con `App.vue` = 41 SFC |
| `stores/` | 15 | Store Pinia (es. `aiStore.ts` 1.994 righe, `fraudDetectionStore.ts` 728) |
| `helpers/` | 3 | `fieldLock.ts`, `queryUtils.ts`, … |
| `interfaces/` | 5 | Tipi TypeScript |
| `api/`, `plugins/`, `router/` | 1 + 1 + 1 | Axios, Vuetify, vue-router (18 voci di path) |

Totale `src`: 69 file `.vue`/`.ts`, 15.836 righe non vuote.

### 1.3 Convenzioni osservate

| Aspetto | Osservato | Evidenza |
|---------|-----------|----------|
| Namespace ≠ cartella/progetto | `DataIngestionBackgroundService` (Application) → `LossPrevention.Infrastructure.Services`; `XmlEnrichmentService` (`Services/Data`) → `...Services.DataIngestion`; `RuleConfigurationService` in `Services/Data/Rules` → `...Services.Rules`; `MappingHelper` → `FraudDetectionApp.Helpers`; `Workspace`/`Tab`/`Query` (Domain) → `LossPrevention.Application.Entities.Workspaces`; `MongoDbSettings` (`Configuration/`) → `LossPrevention.Infrastructure.Models`; `UpdateDashboardValidator` definito in due namespace | file citati |
| Naming | `ReportDataservice` / `DistanceDataService` / `IDistanceDataservice`; file `RulesService.cs` contiene `RuleConfigurationService`; file `IndexSuggestionHelper.cs` contiene `IndexService` | `Services/Data/*` |
| Identificatori entità | Mix `_id` (stile Mongo: `User`, `Role`, `Permission`, `MappingItem`, `RuleConfiguration`) e `Id` (`Workspace`, `DashboardDocument`, `GroupDocument`, …) | `Domain/Entities/**` |
| Nomi progetto | csproj con prefisso numerico e spazi (`01. LossPrevention.API.csproj`) | root progetti |
| Nullable | Abilitato, ma molte `string` di entità non inizializzate | `Domain/Entities/**` |
| Async | `async/await` diffuso; chiamate SSH.NET sincrone avvolte in `Task.Run` | `SftpFileProcessingService.cs` |

---

## 2. Key Implementation Patterns

**Sintesi.** I pattern ricorrenti sono: endpoint REPR con cache in memoria, repository generico con accesso diretto alla collection, conversione XML→BSON con inferenza dei tipi, rule engine a campo singolo, similarità euclidea e un motore statistico generato a runtime nel browser.

### 2.1 Endpoint FastEndpoints con cache

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

### 2.2 Repository generico + registrazione per factory

`MongoRepository<TDocument>` (`Infrastructure/Repositories/MongoRepository.cs`, 179 righe) espone CRUD, `AggregateAsync` (3 overload, riga 143-170), `CreateIndexesAsync` (max 20 campi, righe 29-40) e la proprietà `Collection` (riga 27), usata dai servizi per operazioni non coperte (es. `UpdateManyAsync`, `ReplaceOneAsync`, `GetCollection("ProcessedFiles")`).

```csharp
// LossPrevention.Infrastructure/InfrastructureServiceExtensions.cs (schema ripetuto 14 volte, righe 42-149)
services.AddScoped<IMongoRepository<User>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return new MongoRepository<User>(client, settings.DatabaseName, settings.CollectionName_Users);
});
```
`IMongoRepository<User>` è registrato due volte (65-70 e 72-77); la collezione `PasswordResetTokens` è un literal (riga 83).

### 2.3 Conversione XML → BSON

`XmlToBsonConverterHelper.ConvertFlattened` analizza l'XML con `XElement.Parse`, appiattisce la gerarchia e inferisce i tipi (bool/int/decimal/datetime/string). Con le impostazioni di default di `XElement.Parse` .NET non risolve DTD/entità esterne (no XXE) e limita l'espansione delle entità: il rischio "XML bomb" è contenuto, ma non esiste un limite esplicito sulla dimensione dei file.

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
Funzione pura e testabile, ma espressività minima (1 campo, nessun operatore logico). `ApplyRulesAsync` (`Services/Data/Rules/RulesService.cs:32-84`) carica tutto `ReportData` (riga 34), rimuove `FraudFlags` (40-41), valuta le regole abilitate e riscrive ogni documento con `ReplaceOneAsync` (58).

### 2.5 Similarità

`DistanceHelper.ComputeDistance` = distanza euclidea sui campi presenti in entrambi i record; `TryNormalize` converte solo il **tipo** (bool→0/1, liste→media), **non** la scala. Score: `0.5 · campiUsati/campiRichiesti + 0.5 · (1 − d/dmax)`; minimo 3 campi in comune; top 10. `DistanceDataService.GetDistanceAsync` (riga 22, 122 NLOC) carica in memoria tutti i documenti dell'intervallo di date (riga 39).

### 2.6 Motore statistico client-side

`UI/src/stores/aiStore.ts`: `classifyFields` (riga 379) → mappa semantica via regex sui nomi campo → statistiche (percentili, media, σ di popolazione) → `build*Rules` (condizioni stringa tipo `row["Amount_total"] > 812.5`) → `new Function` (riga 1769) → valutazione per transazione raggruppata.

### 2.7 Ingestione

`FileProcessingCoordinator` sceglie il processor per `SelectedFileType` tra i 3 `IFileProcessingService` registrati (`Program.cs:102-104`); idempotenza per nome file su `ProcessedFiles` (`FileProcessingCoordinator.cs:385-409`); arricchimento con `_sourceFile`, `_processedAt`, `_sourceType`. Per SFTP ogni file è **scaricato e parsato due volte** e a ogni esecuzione viene rielencata l'intera cartella `processed/`.

---

## 3. Framework Usage (MVC, ORM, DI)

**Sintesi.** Al posto di MVC il backend usa FastEndpoints (REPR); al posto di un ORM usa direttamente MongoDB.Driver con un repository generico; la DI è il container Microsoft, con alcuni errori di lifetime e di registrazione. FluentValidation è referenziato ma di fatto inattivo.

### 3.1 Web layer: FastEndpoints (in luogo di MVC)

| Aspetto | Implementazione | Evidenza |
|---------|-----------------|----------|
| Registrazione | `AddFastEndpoints()` + `SwaggerDocument` ("Loss Prevention API" v1.0); `UseFastEndpoints()` ultimo middleware | `Program.cs:53-61,131` |
| Modello | `Endpoint<TRequest, TResponse>` con `Configure()` (verbo, route, `Permissions`, `AllowAnonymous`, `Summary`) e `HandleAsync` | es. `GetReportDataEndpoint.cs:28-29` |
| Request/response | Classi in `Application/Handlers/{Requests,Responses}` (52 file) | `Application/Handlers` |
| Risposte | `SendOkAsync`, `SendAsync`, `SendStringAsync`, `AddError` + `SendErrorsAsync` | es. `ForgotPasswordEndpoint.cs:41-56` |
| Controller MVC / Razor | Assenti | — |

### 3.2 Accesso dati: MongoDB.Driver (nessun ORM)

| Aspetto | Implementazione |
|---------|-----------------|
| Mapping | Attributi `[BsonId]`, `[BsonElement]`, `[BsonRepresentation]` sulle entità Domain; `ReportData` gestito come `BsonDocument` schema-less |
| Query | `Builders<T>.Filter/Update`, espressioni LINQ (`FindOneAsync(u => u.Username == ...)`), pipeline di aggregazione `BsonDocument[]` |
| Scritture massive | `InsertManyAsync` con `IsOrdered=false, BypassDocumentValidation=true` (`MongoRepository.cs:80-83`); `BulkWriteAsync` in `MappingService.FinalizeTypesAsync` con `Parallel.ForEachAsync` |
| ORM relazionale | `DapperRepository` (Dapper/SQL) interamente commentato: residuo di un precedente accesso SQL |

### 3.3 Dependency Injection

| Registrazione | Lifetime | Evidenza | Osservazione |
|---------------|----------|----------|--------------|
| `IOptions<MongoDbSettings>`, `IOptions<JwtSettings>` | Singleton | `InfrastructureServiceExtensions.cs:26-31` | Bind manuale + `Options.Create` |
| `IMongoClient` | **Scoped** | `InfrastructureServiceExtensions.cs:33-37` | Nuovo `MongoClient` per scope: anti-pattern (il client va condiviso, gestisce il pool) |
| `IMongoRepository<T>` ×14 | Scoped (factory) | `InfrastructureServiceExtensions.cs:42-149` | `User` registrato due volte |
| `MongoDbSettings` (classe concreta) | Singleton **vuoto** | `Program.cs:83` | Iniettato da `CreateRuleEndpoint.cs:15` e `UpdateRuleEndpoint.cs:15`: `CollectionName_ReportData` è stringa vuota (difetto D10) |
| 20 registrazioni di servizi (incl. `EmailService`) | Scoped | `Program.cs:84-110` | 3 implementazioni di `IFileProcessingService` risolte come `IEnumerable` |
| `DataIngestionBackgroundService` | Hosted (singleton) | `Program.cs:111` | Crea uno scope per ciclo (`DataIngestionBackgroundService.cs:49`) |
| `IMemoryCache` | Singleton | `Program.cs:49` | Cache report |
| Container temporaneo | — | `Program.cs:40-47` | Seconda `ServiceCollection` costruita solo per leggere `JwtSettings` (commento fuorviante "Load rules from MongoDB") |
| `IDataIngestionScheduleService` | Scoped | `Program.cs:99` | Mai iniettato: codice non raggiungibile |

### 3.4 Validazione: FluentValidation

`Application/Validators/CreateDashboardValidator.cs` definisce `CreateDashboardValidator`, `UpdateDashboardValidator`, `DashboardBlockValidator` (namespace `LossPrevention.Application.Dashboards.Validation`); `UpdateDashboardValidator.cs` ridefinisce `UpdateDashboardValidator` in `LossPrevention.Application.Handlers.Requests.Dashboard`. Tutti estendono `AbstractValidator<T>`: FastEndpoints registra automaticamente solo i `Validator<T>` propri, mentre gli `AbstractValidator<T>` sono inclusi solo con `IncludeAbstractValidators = true`, opzione non impostata in `Program.cs:53`; nessun endpoint li invoca manualmente. **I validator non sono quindi eseguiti.**

### 3.5 Frontend

Vue 3 Composition API (`<script setup>`), Pinia per lo stato, vue-router con guard su `localStorage.token`, Axios con interceptor che aggiunge l'header `Authorization` (`src/api/api.ts:10-17`). La dipendenza `nuxt` e `nuxt.config.ts` sono presenti ma non usati: l'app è montata con `createApp` in `src/main.ts`.

---

## 4. Security Implementation

**Sintesi.** Autenticazione JWT e hashing PBKDF2 sono implementati correttamente nella sostanza, ma l'autorizzazione dei dati è demandata al client (pipeline arbitrarie, lock di riga), e ci sono esposizioni dirette (hash restituiti, segreti in configurazione, `eval`).

### 4.1 Controlli implementati

| Controllo | Implementazione | Evidenza |
|-----------|-----------------|----------|
| Autenticazione | JWT HMAC-SHA256; validazione issuer, audience, lifetime, firma | `Program.cs:63-76`; `LoginEndpoint.cs:48-80` |
| Autorizzazione | Claim `permissions` (uno per permesso dei ruoli) + `Permissions("CAN_...")` sugli endpoint: 37 permessi distinti, 74 endpoint protetti, 4 anonimi | `LoginEndpoint.cs:57-61` |
| Password | PBKDF2-SHA256, 600.000 iterazioni, salt 16 byte, hash 32 byte, formato `v2:{iter}:{base64}`; hash legacy 10.000 iterazioni aggiornati al login | `Infrastructure/Helpers/PasswordHasher.cs:7-50`; `UserService.cs:206-214` |
| Account inattivi | Login e reset rifiutati se `IsActive=false` | `UserService.cs:194`, `PasswordResetService.cs:42` |
| Reset password | Token 32 byte casuali (base64url), salvato come SHA256 hex, scadenza 1 h, token precedenti invalidati | `PasswordResetService.cs:19,50-66,145-159` |
| Anti-enumerazione | `ForgotPassword` risponde sempre con messaggio generico se l'utente non esiste | `ForgotPasswordEndpoint.cs:41,56` |

### 4.2 Debolezze a livello di codice

| Pattern | Posizione | Rischio |
|---------|-----------|---------|
| Pipeline di aggregazione dal client (`BsonDocument.Parse`) | `GetReportDataEndpoint.cs:68` | Query injection / esfiltrazione (es. `$lookup` su `Users`) |
| Row-level lock solo client | Claim `LockField/LockValue` (`LoginEndpoint.cs:65-68`) mai letti dal backend; applicati in `UI/src/helpers/fieldLock.ts` (`resultsGrid.vue:1687`, `toolbar.vue:180`, `tabularBlock.vue:259`) | Bypass con una chiamata API diretta |
| Hash e salt nei DTO | `GetUsersEndpoint.cs:36` | Offline cracking |
| `eval(expr)` su campi calcolati | `UI/src/components/reports/resultsGrid.vue:1377` | XSS persistente via workspace condiviso |
| `new Function(...)` | `UI/src/stores/aiStore.ts:1769` | Iniezione tramite nomi campo provenienti dai file importati |
| Token in `localStorage` | `loginStore.ts:92`, `api.ts:11`, `fieldLock.ts:2`, `notificationStore.ts:49`, `App.vue:284` | Esfiltrabile da qualsiasi XSS |
| Confronto hash non a tempo costante | `PasswordHasher.cs:46` (`string.Equals`) | Timing attack (rischio basso) |
| Nessun lockout / rate limiting sul login | nessun riferimento nel backend | Brute force |
| Messaggio distinto per account inattivo nel reset | `ForgotPasswordEndpoint.cs:45-47` | Enumerazione account inattivi |
| Segreti in chiaro | `JwtSettings:SecretKey` in `appsettings.json`; `SftpPassword` nel documento `DataIngestionConfigurations` | Compromissione token / SFTP |
| SMTP senza TLS | `EmailService.cs:64-65` (`EnableSsl=false`, `UseDefaultCredentials=true`) | Link di reset in chiaro |
| SFTP senza verifica host key | `SftpFileProcessingService.cs:56` | MITM |
| Notifiche senza filtro destinatario | `ListNotificationsEndpoint.cs:30-31` (`Filter.Empty`) | Ogni utente vede tutte le notifiche |
| `ex.Message` restituito al client | 13 occorrenze negli endpoint | Information disclosure |

---

## 5. Exception Handling & Logging

**Sintesi.** La gestione errori è locale a ogni endpoint/servizio (try/catch generici), senza middleware globale né ProblemDetails personalizzati; il logging strutturato (`ILogger`) è usato solo in 4 classi, il resto usa `Console.WriteLine` o niente.

### 5.1 Eccezioni

| Aspetto | Osservato | Evidenza |
|---------|-----------|----------|
| Catch generici | 31 `catch (Exception …)` nel backend | grep su `*.cs` |
| Messaggi interni al client | 13 `ex.Message` negli endpoint (es. `"Invalid query pipeline: " + ex.Message`) | `GetReportDataEndpoint.cs:87` |
| Eccezioni lanciate | Solo BCL: `InvalidOperationException` (24), `KeyNotFoundException` (9), `ArgumentException` (5), `UnauthorizedAccessException` (3), `DirectoryNotFoundException` (2), `NotImplementedException` (1), `NotSupportedException` (1), `ArgumentNullException` (1) | grep `throw new` |
| Eccezioni di dominio | `DeleteUserRoleException`, `InvalidCustomerException` definite ma mai usate | `Domain/Exceptions/` |
| Handler globale | Assente (nessun `UseExceptionHandler`, nessun `ProblemDetails` custom) | `Program.cs:121-131` |
| Startup | `DatabaseInitializationService` rilancia l'eccezione → l'API non parte | `DatabaseInitializationService.cs:43-116` |
| Background | Il ciclo dello scheduler cattura e logga gli errori, continua al minuto successivo | `DataIngestionBackgroundService.cs` |

### 5.2 Logging

| Aspetto | Osservato |
|---------|-----------|
| Framework | `Microsoft.Extensions.Logging` con provider di default (console/debug); livello `Default: Information` in `appsettings.json` |
| Uso di `ILogger<T>` | Solo `DatabaseInitializationService`, `DataIngestionBackgroundService`, `FileProcessingCoordinator`, `SftpFileProcessingService` |
| `Console.WriteLine` | 6: `DistanceDataservice.cs:86`, `MappingService.cs:336` e `:421`, console `Program.cs:80`, `:108`, `:122`; `MappingService.WriteLog` scrive su console |
| Frontend | 57 `console.log`, anche con dati (mappe semantiche, conteggi, base URL in `api.ts:7`) |
| Correlazione / audit | Nessun correlation id, nessun log di sicurezza (login falliti, modifiche permessi); solo `createdBy/updatedBy` su `FraudDetectionSettings` |

Dettaglio della gestione dei log come dato: [08_data.md §7](08_data.md#7-log-management).

---

## 6. Transaction Management

**Sintesi.** Nessuna transazione: il codice non usa sessioni MongoDB (`StartSession`, `WithTransaction`) né `TransactionScope`. Tutte le operazioni multi-documento sono non atomiche e senza compensazione.

| Operazione multi-step | Passi | Evidenza | Effetto di un errore intermedio |
|-----------------------|-------|----------|---------------------------------|
| Applica regole | `UpdateMany` unset `FraudFlags` → `ReplaceOne` per documento → delete mapping `FraudFlags` → `ProcessMappings` | `RulesService.cs:40-78` | Dataset con flag parziali e mapping mancanti |
| Aggiornamento regola | `Set` flag su ogni documento → aggiornamento mapping | `RulesService.cs:141-167` | Flag e mapping disallineati |
| Ingestione file | `InsertMany` ReportData (unordered) → `InsertOne` ProcessedFiles | `FileProcessingCoordinator.cs` | Crash tra i due passi → file reingerito (duplicati) |
| Eliminazione ruolo | `UpdateOne` per ogni utente → `DeleteOne` ruolo | `UserRoleService.cs:59-79` | Utenti modificati ma ruolo esistente |
| Eliminazione permesso | `UpdateOne` per ogni ruolo → `DeleteById` permesso | `UserPermissionService.cs:117-133` | idem |
| Reset password | Invalida token → inserisce token → invia email; poi aggiorna password → marca token usato | `PasswordResetService.cs:50-66,122-139` | Token valido ma email non inviata |
| Eliminazione workspace | `DeleteOne` workspace | `WorkspaceService.cs:75-87` | Dashboard collegate restano orfane |

Concorrenza: nessun optimistic locking (nessun campo versione); `ApplyRulesAsync` e l'ingestione possono girare in parallelo sulla stessa collezione. Le transazioni multi-documento MongoDB richiederebbero un replica set; la topologia del server non è ricavabile dal codice (dump da `mongod` 8.3.2).

---

## 7. Configuration Management

**Sintesi.** La configurazione è in un unico `appsettings.json` per host, senza file per ambiente, senza secret store e con una sequenza di caricamento che rende inefficace l'override via variabili d'ambiente; il frontend dipende da `VITE_API_BASE_URL` non versionato e da URL hardcoded.

### 7.1 Sorgenti di configurazione

| Host | File | Chiavi | Caricamento |
|------|------|--------|-------------|
| API | `LossPrevention.API/appsettings.json` | `Logging`, `DataRetention:TransactionRetentionDays=180`, `AllowedHosts`, `MongoDbSettings` (connection string + `DatabaseName` + 13 `CollectionName_*`), `JwtSettings` (`SecretKey`, `Issuer=LossPrevention`, `Audience=User`, `ExpiryHours=1`), `Email` (host, porta 25, mittente, `FrontendUrl`) | `WebApplication.CreateBuilder` + **riaggiunta** di `appsettings.json` (`Program.cs:36-38`) |
| API | `Properties/launchSettings.json` | Porte 5264/7110, IIS Express, `ASPNETCORE_ENVIRONMENT=Development` | Solo sviluppo |
| Console | `LossPrevention.DataIngestionService/appsettings.json` | `Logging`, `MongoDbSettings` con 6 `CollectionName_*` | `ConfigurationBuilder` solo JSON (`DataIngestionService/Program.cs:31-35`), passato ai servizi anche dopo aver creato il Generic Host |
| SPA | `import.meta.env.VITE_API_BASE_URL` | Base URL API | Nessun `.env` nel repository |

### 7.2 Modalità di accesso nel codice

| Modalità | Dove |
|----------|------|
| `IOptions<T>` | `JwtSettings` in `LoginEndpoint.cs:17-21`; `MongoDbSettings` nelle factory dei repository |
| `IConfiguration` diretto | `EmailService.cs:17-24` (con default hardcoded), `DatabaseInitializationService.cs:23` (`GetValue<int>(..., 90)`) |
| Valori hardcoded | CORS `localhost:5173/5174` (`Program.cs:29`), collezione `"PasswordResetTokens"` (`InfrastructureServiceExtensions.cs:83`), `"ProcessedFiles"` (`FileProcessingCoordinator.cs:391,409`), `C:\xmlstore5\xml` (console `Program.cs:76`), Ollama URL e modello (`aiStore.ts:8,88`), polling 1 min (`DataIngestionBackgroundService.cs:13`) |
| Configurazione a runtime nel DB | `DataIngestionConfigurations` (sorgente, SFTP, schedulazione), `FraudDetectionSettings` (soglie), `Rules`, `Mappings` |

### 7.3 Difetti di configurazione

| # | Difetto | Evidenza | Effetto |
|---|---------|----------|---------|
| C1 | `appsettings.json` riaggiunto dopo le sorgenti di default | `Program.cs:36-38` | I valori del file prevalgono su variabili d'ambiente e argomenti: impossibile sovrascrivere `JwtSettings:SecretKey` o la connection string senza modificare il file |
| C2 | `MongoDbSettings` singleton vuoto | `Program.cs:83` | D10 (vedi Appendice B) |
| C3 | Nessun file per ambiente (`appsettings.{Environment}.json`) né user-secrets | ricerca file | Segreti e host di sviluppo nel repository |
| C4 | Default di retention divergenti | codice 90 gg (`DatabaseInitializationService.cs:23`) vs config 180 gg | Comportamento diverso se la chiave manca |
| C5 | Console con 6 nomi collezione su 13 | `DataIngestionService/appsettings.json` | Repository non configurati ricevono nome collezione nullo (non usati dalla console) |

---

## 8. Testing Strategy

**Sintesi.** Non esiste alcuna strategia di test implementata: zero progetti di test, zero file di test, nessuno script npm di test/lint/type-check, nessuna pipeline CI.

| Livello | Stato | Evidenza |
|---------|-------|----------|
| Unit test backend | Assenti | Nessun `*Tests.csproj` nella soluzione (5 progetti) |
| Unit test frontend | Assenti | Nessun file `*.spec.*`/`*.test.*`, nessun `vitest.config.*`; `package.json` ha solo `dev`, `build`, `preview` |
| Integration / API test | Assenti | — |
| E2E | Assenti | Nessuna dipendenza Playwright/Cypress |
| Static analysis | Assente | Nessuna config ESLint/Prettier; `vue-tsc` installato ma non usato in `build`; nessun analyzer .NET aggiuntivo |
| CI | Assente | Nessun workflow/pipeline |
| Dati di test | Dump `Data/LossPrevention` (3.000 transazioni) e seed `Data/MongoDBScripts` utilizzabili come fixture manuali | — |

Nota storica: `Docs/13_TESTING_QA.md` del fornitore (presente solo nel commit `593f6de`, rimosso in `d768cd9`; `git show 593f6de:customspa-it-loss-prevention-d098a8b97860/Docs/13_TESTING_QA.md`) descriveva xUnit, Vitest e target di copertura (unit 80%, integration 70%) con copertura attuale "TBD": nulla di tutto ciò è presente nel codice.

Candidati prioritari per i primi test (funzioni pure o difetti noti): `RuleHelper.EvaluateRule`, `DistanceHelper.ComputeDistance`, `BsonHelper.ConvertToMappedType`/`TryToDateTimeUtc`, `PasswordHasher`, `XmlToBsonConverterHelper`, `queryUtils.buildTypeAwareCondition`; test di regressione su D1-D10.

---

## Appendici

### A. Hotspot di complessità

Rilevazione `lizard` 1.24.1 rieseguita il 2026-10-08 su backend e `LossPrevention.UI/src` (soglia CCN > 15):

| Funzione | File:riga | NLOC | CCN |
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

Altre funzioni sopra soglia (CCN 16-20): `buildVelocityRules` (aiStore.ts:620), `buildSweetheartingRules` (:838), `load` (dataIngestionStore.ts:230), `detectUniqueTransactionFields` (fraudDetectionStore.ts:595), `afterDraw` (heatmapPreview.vue:273), funzione anonima in `manage/dataingestion.vue:123`.

### B. Difetti funzionali individuati nel codice

| # | Difetto | Posizione | Effetto |
|---|---------|-----------|---------|
| D1 | `CreateTransactionEndpoint` chiama `ProcessAsync` senza inserire e legge `bson["_id"]` mai impostato; `XmlProcessingService.InsertManyAsync` lancia `NotImplementedException` | `Endpoints/Data/CreateTransactionEndpoint.cs:49-50`, `XmlProcessingService.cs` | Eccezione o nessuna persistenza |
| D2 | Token JWT restituito come `Task` serializzato (manca `await`) | `LoginEndpoint.cs:39-40` | Il FE dipende da `token.result` (`loginStore.ts:77`) |
| D3 | `UpdateRuleAsync` scrive `Set($"{rule.RuleName}", …)` alla radice e cerca la mapping `Name == RuleName` senza prefisso `FraudFlags.` | `RulesService.cs:147,155` | Flag duplicati in radice; rinomina regola non aggiorna la mapping |
| D4 | `ApplyRulesAsync` cancella tutte le mapping `FraudFlags` e le rigenera campionando 1.000 documenti | `RulesService.cs:64-78` | Alias/visibilità personalizzati persi |
| D5 | `LastRunAt` impostato ma non persistito | `DataIngestionBackgroundService.cs` | Possibile doppia esecuzione nella stessa finestra; nessuno storico |
| D6 | `GET /notifications` senza filtro destinatario | `ListNotificationsEndpoint.cs:30-31` | Ogni utente vede tutte le notifiche |
| D7 | `keyValue.AsString` sul campo chiave | `DistanceDataservice.cs:176` | Eccezione se il key field non è stringa |
| D8 | Filtro visibilità tautologico | `ReportDataservice.cs:75` | I campi "non visibili" vengono restituiti |
| D9 | Statistiche antifrode calcolate solo sulla pagina caricata (≤ 3.000 righe) | `aiStore.ts` | Soglie dipendenti da ordinamento/paginazione |
| D10 | `MongoDbSettings` registrato come singleton vuoto e iniettato in Create/UpdateRule | `Program.cs:83`; `CreateRuleEndpoint.cs:15,56`; `UpdateRuleEndpoint.cs:15,62`; `RulesService.cs:110,167` | Mapping `FraudFlags.*` create con `CollectionName = ""`, quindi escluse dal filtro `CollectionName == "ReportData"` (`DistanceDataservice.cs:49`) |

D8 in dettaglio (`ReportDataservice.cs:75`):
```csharp
else if (visibleFields.Contains(kv.Key) || kv.Key == "count" || !visibleFields.Any() || !visibleFields.Contains(kv.Key))
```
La condizione è una tautologia (`A || … || !A`).

### C. Codice morto / residui

| Elemento | Evidenza |
|----------|----------|
| `DapperRepository.cs` (205 righe), `IDapperRepository.cs` | Interamente commentati |
| `XmlProcessingService.InsertManyAsync` | `throw new NotImplementedException` |
| `IXmlEnrichmentRule` | Nessuna implementazione → `XmlEnrichmentService.Enrich` è un no-op |
| `DataIngestionScheduleService` / collezione `DataIngestionSchedules` | Registrato (`Program.cs:99`) ma mai iniettato |
| `DeleteUserRoleException`, `InvalidCustomerException` | Mai usate |
| Console STEP 1 | Carica le regole e non le usa (`DataIngestionService/Program.cs:38-47`, "Just load rules") |
| `nuxt.config.ts`, dipendenze `nuxt`, `jspdf`, `jspdf-autotable`, `grid-layout-plus`, `vuedraggable` | Non referenziate dal codice |
| Blocco commentato in `DistanceDataservice` (conteggi `*Count`) | — |
| `Mappings_old.bson`, `ReportData_old.bson` nel dump | Vedi [08_data.md §2](08_data.md#2-physical-data-model) |

---

## Reference Documents

- **Deep Dive Analysis**: [00_deep_dive.md](00_deep_dive.md)
- **Context**: [01_context.md](01_context.md)
- **Functional Overview**: [02_functional_overview.md](02_functional_overview.md)
- **Non-Functional Overview**: [03_non_functional_overview.md](03_non_functional_overview.md)
- **Constraints**: [04_constraints.md](04_constraints.md)
- **Principles**: [05_principles.md](05_principles.md)
- **Software Architecture**: [06_software_architecture.md](06_software_architecture.md)
- Documenti successivi correlati: [08_data.md](08_data.md) · [14_metrics.md](14_metrics.md) · [16_frontend_deep_assessment.md](16_frontend_deep_assessment.md) · [17_backend_deep_assessment.md](17_backend_deep_assessment.md) · [18_antipattern_deep_dive.md](18_antipattern_deep_dive.md)

---

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
| 1.1 | 2026-10-08 | IMPACT verify | Ristrutturato nelle 8 sezioni del prompt; aggiunte sezioni Framework Usage (FastEndpoints, MongoDB.Driver, DI con lifetime, FluentValidation inattivo), Transaction Management, Configuration Management (override env inefficace, MongoDbSettings vuoto), Testing Strategy; struttura codice con conteggi verificati; Security Implementation estesa (controlli presenti + 15 debolezze con file:riga); nuovo difetto D10; lizard rieseguito (+4 hotspot); codice morto esteso (ScheduleService, eccezioni di dominio); DapperRepository 204→205 righe; vendor doc test citato come storico (`593f6de`/`d768cd9`); Reference Documents completi |
