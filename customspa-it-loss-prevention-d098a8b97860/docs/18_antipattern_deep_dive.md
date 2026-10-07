<!-- REVERSE-META
schema: 1
mode: how
step: 18_antipattern_deep_dive
commit: 593f6decec3a642d5567754dd795b19560bb0afb
commit_short: 593f6de
branch: main
worktree: clean
baseline_date: 2026-10-05T10:14:05+00:00
generated_at: 2026-10-07T14:40:00Z
-->
# 🔍 ANTIPATTERN ASSESSMENT REPORT

> **Baseline commit:** `593f6de` (`main`) · worktree `clean`

**Data**: 2026-10-07
**Codebase**: `customspa-it-loss-prevention-d098a8b97860` (LOSS_PREVENTION)
**Health Score**: **52 / 100**

## 📊 RIASSUNTO ESECUTIVO

Il backend è ordinato (layer, REPR, funzioni brevi) ma soffre di *leaky abstraction* verso MongoDB e di un modello anemico; il debito critico è concentrato nel frontend, dove due "god file" incorporano logica di business e di sicurezza generata ed eseguita dinamicamente. Il debito tecnico è **gestibile** come volume ma **critico** come impatto, perché tocca sicurezza e affidabilità delle decisioni antifrode.

## Fase 1 — Scansione e metriche

| Indicatore | Valore |
|-----------|--------|
| File sorgente applicativi | 232 C# + 41 Vue + 29 TS |
| Funzioni | 475 (BE) + 569 (FE) |
| God files (> 500 righe) | `resultsGrid.vue` 2.271, `aiStore.ts` 1.994, `selectFields.vue` 823, `fraudDetectionStore.ts` 728, `queryBuilderTabs.vue` 693, `FraudSettingsDialog.vue` 595, `notifications.vue` 562, `dataingestion.vue` 566 |
| Funzioni CCN > 15 | 20 (max 87) |

## 🚨 TABELLA PRIORITÀ (Top Findings)

| File/Modulo | Anti-Pattern | Severità | Impatto |
| :--- | :--- | :--- | :--- |
| `UI/src/stores/aiStore.ts` | God Object / The Blob | Critica | LLM, classificazione, statistica, 30 euristiche e valutazione in un unico store: non testabile, non riusabile dal backend |
| `UI/src/components/reports/resultsGrid.vue` | God Object + `eval` (Code injection) | Critica | XSS persistente; ogni modifica alla griglia rischia regressioni |
| `API/Endpoints/Data/GetReportDataEndpoint.cs` | Golden Hammer ("la pipeline Mongo dal client risolve tutto") / Trust boundary violation | Critica | Query injection, bypass sicurezza |
| `UI/src/helpers/fieldLock.ts` | Security by obscurity (client-side enforcement) | Critica | Lock aggirabile |
| `Application/Services/Data/Rules/RulesService.cs` | Leaky abstraction + N+1 writes | Alta | Prestazioni e incoerenze dei flag |
| `Infrastructure/Repositories/MongoRepository.cs` | Leaky abstraction (`Collection` pubblica) | Media | Servizi accoppiati al driver |
| 20 endpoint (Dashboard, Groups, Notifications, FraudDetection) | Layer skipping / Fat endpoint | Media | Logica e mapping duplicati negli endpoint |
| `Create/UpdateFraudDetectionSettingsEndpoint.cs` | Copy-paste programming | Media | ~300 righe di mapping duplicate |
| `Domain/Entities/*` | Anemic Domain Model | Media | Regole sparse tra helper e servizi |
| `DataIngestionConfiguration` (`ScheduleType`, `Recurrence`, `SelectedSources` come stringhe) | Primitive Obsession | Bassa | Confronti stringa (`"filesystem"`, `"file system"`) e valori magici |
| `aiStore.ts` (`SEMANTIC_PATTERNS`) | Magic strings / regex su nomi campo | Media | Classificazione fragile, dipendente dai nomi del POS |
| 31 `catch (Exception)` | Error swallowing / generic exceptions | Media | Errori nascosti, messaggi interni esposti |
| `DapperRepository.cs`, `IXmlEnrichmentRule`, `InsertManyAsync` | Dead code / Speculative generality (YAGNI) | Bassa | Rumore |
| AG Grid + Tabulator; `vue-grid-layout-v3` + `grid-layout-plus`; Newtonsoft + System.Text.Json | Stovepipe / duplicazione di librerie | Bassa | Bundle e manutenzione |
| Ollama hardcoded in FE | Vendor/runtime lock-in senza astrazione | Media | Impossibile cambiare provider LLM senza toccare il client |

Fase 2 — macro-level: **nessun Big Ball of Mud** (layer riconoscibili, nessuna dipendenza circolare tra progetti); presente uno **"split-brain" architetturale** — la logica di dominio antifrode è divisa tra backend (regole) e frontend (statistica) senza modello comune.

## 🔍 ANALISI DETTAGLIATA (Top 3 Problemi Critici)

### 1. Trust boundary violation / Golden Hammer in `GetReportDataEndpoint.cs`

**Dove:** `HandleAsync`, costruzione della pipeline.
**Analisi:** il server delega al client la costruzione della query e la esegue senza validazione. Viola *least privilege* e *never trust the client*; rende impossibile applicare filtri di sicurezza e invalida la cache per utente.

**Codice Attuale:**
```csharp
var pipeline = req.QueryPipeline
                  .Where(x => x != null)
                  .Select(x => BsonDocument.Parse(x.ToString()))
                  .ToArray();
var result = await _reportDataService.QueryReportDataAsync(pipeline, req.Skip, req.Take);
```

**Soluzione Proposta:** il client invia il modello di query già esistente (`Tab.SelectedFields`, `Query.Conditions`, `GroupByField`); un `QueryBuilder` server-side genera la pipeline, valida campi contro `Mappings`, ammette solo operatori noti e antepone il filtro di lock dell'utente.

**Esempio Refactoring:**
```csharp
public sealed record ReportQuery(List<FieldDTO> Fields, QueryDTO Filter, string? GroupBy, int Skip, int Take);

public BsonDocument[] Build(ReportQuery q, ClaimsPrincipal user, IReadOnlySet<string> knownFields)
{
    var stages = new List<BsonDocument>();
    var lockField = user.FindFirst("LockField")?.Value;
    var lockValue = user.FindFirst("LockValue")?.Value;
    if (lockField is not null) stages.Add(new("$match", new BsonDocument(lockField, lockValue)));
    stages.Add(new("$match", ConditionTranslator.Translate(q.Filter, knownFields)));   // whitelist operatori: $eq,$ne,$gt,$gte,$lt,$lte,$in,$regex(escaped)
    if (q.GroupBy is not null) stages.Add(GroupStage(q, knownFields));
    stages.Add(new("$project", Projection(q.Fields, knownFields)));
    return stages.ToArray();
}
```

### 2. God Object in `aiStore.ts`

**Dove:** intero file (1.994 righe); `buildGenericFraudRuleTemplates` (righe 1203–1530, CCN 87), `analyzeFraudInData` (1667–1922, CCN 34).
**Analisi:** viola SRP (LLM, NLQ, classificazione semantica, statistica, generazione regole, compilazione ed esecuzione, stato UI e timer). Le regole sono stringhe compilate con `new Function`, quindi non tipizzate né testabili.

**Codice Attuale:**
```ts
rules.push({
  fraudType: "High Value Transaction",
  fields: [f],
  condition: `row["${f}_total"] > ${p95}`,
  severity: "medium",
});
// ...
const fn = new Function("row", `try { return (${rule.condition}) } catch { return false }`);
```

**Soluzione Proposta:** estrarre un modello dichiarativo di regola (`{field, op, threshold}`) valutato da un interprete; portare `computeStats` e i `build*Rules` in un servizio C# (`FraudAnalysisService`) eseguito come job, con risultati persistiti; lasciare nello store solo stato UI e chiamate API.

**Esempio Refactoring:**
```ts
type Rule = { fraudType: string; field: string; op: '>' | '<' | '>=' | '<=' | '=='; threshold: number; severity: 'low'|'medium'|'high' };
const ops = { '>': (a: number, b: number) => a > b, '<': (a, b) => a < b, '>=': (a, b) => a >= b, '<=': (a, b) => a <= b, '==': (a, b) => a === b } as const;
export const evaluate = (row: Record<string, unknown>, r: Rule) =>
  typeof row[r.field] === 'number' && ops[r.op](row[r.field] as number, r.threshold);
```

### 3. Code injection via `eval` in `resultsGrid.vue`

**Dove:** riga ~1377 (valueGetter dei campi calcolati).
**Analisi:** l'espressione è definita da un utente e salvata nel workspace (condiviso con tutti); quando un altro utente apre il report, l'espressione viene eseguita nel suo browser con accesso al JWT in `localStorage`.

**Codice Attuale:**
```ts
const expr = expression.replace(/\b(\w+)\b/g, (m) => Object.prototype.hasOwnProperty.call(row, m) ? `row["${m}"]` : m);
const result = eval(expr);
```

**Soluzione Proposta:** usare un parser di espressioni aritmetiche con whitelist (es. `jsep` + interprete, o `expr-eval`), validare lato server al salvataggio del workspace.

**Esempio Refactoring:**
```ts
import { Parser } from 'expr-eval';
const parser = new Parser({ operators: { logical: false, assignment: false, fndef: false } });
const compiled = parser.parse(expression);            // una volta per campo
const result = compiled.evaluate(numericFieldsOf(row)); // solo variabili numeriche note
```

## 🛠️ RACCOMANDAZIONI GENERALI & NEXT STEPS

- [ ] Eliminare `eval` e bloccare la pipeline arbitraria (settimana 1).
- [ ] Spostare lock e motore antifrode sul server (mesi 1–2).
- [ ] Spezzare `aiStore.ts` e `resultsGrid.vue` dopo aver scritto test di caratterizzazione.
- [ ] Chiudere la `Collection` del repository dietro metodi espliciti (`UpdateManyAsync`, `BulkWriteAsync`).
- [ ] Enum/value object per `ScheduleType`, `Recurrence`, `SourceType`, `FileType`.
- [ ] Rimuovere dead code e librerie duplicate.
- [ ] Strumenti: **SonarQube/SonarCloud** (C# + TS), **ESLint** (`plugin:vue/vue3-recommended`, `no-eval`, `no-new-func`), **Roslyn analyzers** + `TreatWarningsAsErrors` sui nuovi progetti, **ArchUnitNET** per vietare `IMongoRepository<>` negli endpoint, **gitleaks** in CI.

---

## Reference Documents
- 07_code.md · 16_frontend_deep_assessment.md · 17_backend_deep_assessment.md

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
