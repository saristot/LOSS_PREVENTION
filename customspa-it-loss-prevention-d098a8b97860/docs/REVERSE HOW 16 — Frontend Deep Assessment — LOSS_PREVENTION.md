<!-- REVERSE-META
schema: 1
mode: how
step: 16_frontend_deep_assessment
commit: 593f6decec3a642d5567754dd795b19560bb0afb
commit_short: 593f6de
branch: main
worktree: clean
baseline_date: 2026-10-05T10:14:05+00:00
generated_at: 2026-10-07T14:40:00Z
-->
# Frontend Deep Assessment - Progetto LOSS_PREVENTION

> **Baseline commit:** `593f6de` (`main`) · worktree `clean`

**Data**: 2026-10-07 · **Versione**: 1.1 · **Autori**: REVERSE how · **Audience**: frontend architect, tech lead, senior frontend developer

---

## Executive Summary

**Salute frontend: 4,5 / 10.** La SPA (Vue 3.5 + Vite 6 + Vuetify 3 + Pinia, 14,7 KSLOC in `src/`) offre un report builder e dashboard ricchi, ma concentra in due file (`resultsGrid.vue` 2.271 righe, `aiStore.ts` 1.994 righe) la logica più critica del prodotto — **motore antifrode statistico, integrazione LLM, valutazione di espressioni** — senza test, lint o type-check in build. Al commit analizzato **la build non riesce su Linux/macOS** (import case-sensitive).

**Top 3 issue**
1. **Sicurezza nel client**: row-level lock applicato solo dal browser; `eval()` su espressioni salvate nei workspace (XSS persistente → furto del JWT in `localStorage`); `new Function()` per le regole.
2. **Logica di business nel browser**: risultati antifrode non persistiti né auditabili; LLM raggiunto su `http://localhost:11434`.
3. **Supply chain e build**: 71 vulnerabilità npm (6 critical); 5 dipendenze inutilizzate (incluse `jspdf*` critical e `nuxt`); build impossibile in CI Linux.

**Top 3 raccomandazioni**
1. Rimuovere `eval`, spostare lock e motore antifrode sul backend (ADR-101/102 nel Decision Log).
2. Introdurre AI gateway backend e configurazione per ambiente.
3. Correggere gli import, pulire le dipendenze, `npm audit fix`, Vitest/ESLint/`vue-tsc` in CI.

---

## 1. Architettura (Micro Frontend / Routing)

- **Non** è un micro-frontend: SPA monolitica, entry `src/main.ts` (`createApp` + Pinia + router + Vuetify + toast + registrazione moduli AG Grid).
- `nuxt.config.ts` (sintassi Nuxt 2 `buildModules`) e la dipendenza `nuxt` **non sono usati**: README e Docs ("Nuxt 3 SSR") sono disallineati.
- Router (`src/router/index.ts`): 17 route, history mode, guard globale che chiama `loginStore.validateToken()` e reindirizza a login se `requiresAuth`. **Nessun controllo di permesso per route**: un utente senza `CAN_MANAGE_*` può aprire `/manage/*` (il backend risponderà 403, ma l'UI è esposta) — roadmap: "Remove features that the user does not have access to on the UI" è ancora da fare.
- Nessun lazy loading delle route: tutti i componenti importati staticamente → bundle iniziale unico.

## 2. State Management

| Store | Righe | Ruolo | Note |
|-------|-------|-------|------|
| `aiStore` | 1.994 | LLM + motore statistico | God store; usa `markRaw` per array grandi |
| `fraudDetectionStore` | 728 | Soglie, metadati frodi, mapping txId | Default soglie hardcoded duplicati rispetto al backend |
| `notificationStore` | 259 | Notifiche | Fetch all'avvio e su refresh |
| `dataIngestionStore` | 275 | Config ingestione | `save` CCN 36 |
| `loginStore` | 150 | JWT, scadenza, logout automatico | Legge `response.data.token.result` |
| altri 10 | 71–145 | CRUD per entità | Pattern uniforme |

Lo stato antifrode (`fraudMetadataMap`, `fraudRulesCache`) vive solo in memoria: un refresh della pagina perde l'analisi.

## 3. Component Architecture

- 41 SFC, `<script setup>` + TypeScript; Vuetify per layout e form.
- Componenti "god": `resultsGrid.vue` (griglia, export 3 formati, campi calcolati, evidenziazione frodi, paginazione dell'analisi, menu contestuali), `selectFields.vue` (823), `queryBuilderTabs.vue` (693).
- Doppie librerie con stessa responsabilità: AG Grid **e** Tabulator; `vue-grid-layout-v3` **e** `grid-layout-plus` (quest'ultima non usata).
- Stili: `style.css` globale + `<style>` per componente (1.296 righe), nessun design token oltre al tema Vuetify.

## 4. Services & HTTP

- Unico client Axios (`src/api/api.ts`) con interceptor che aggiunge `Authorization: Bearer <token>` da `localStorage`; nessun interceptor di risposta (401 → logout gestito altrove, nessun retry).
- `console.log('API base URL', …)` in produzione.
- Chiamata LLM con `fetch` diretto a `http://localhost:11434/api/generate`, URL e modello hardcoded (i Docs citano `VITE_OLLAMA_URL`/`VITE_OLLAMA_MODEL`, **non usati** dal codice).
- RxJS: non applicabile (non usato).

## 5. Security Frontend

| # | Problema | Evidenza | Severità |
|---|---------|----------|----------|
| FE-S1 | `eval(expr)` su espressioni definite dall'utente e salvate nei workspace | `components/reports/resultsGrid.vue:1377` | 🔴 Critica (XSS persistente tra utenti) |
| FE-S2 | Row-level lock calcolato dal client leggendo il JWT | `helpers/fieldLock.ts` | 🔴 Critica (bypass banale) |
| FE-S3 | JWT in `localStorage` | `api.ts`, `loginStore.ts` | 🟠 Alta (combinata con FE-S1) |
| FE-S4 | `new Function` su condizioni che incorporano **nomi di campo** provenienti dai file importati | `stores/aiStore.ts:1769` | 🟠 Media |
| FE-S5 | Dati sensibili nei log (`console.log` di mappe e risultati) | 57 occorrenze | 🟡 Bassa |
| FE-S6 | Nessuna Content-Security-Policy (index.html) | `index.html` | 🟠 Media |

## 6. Performance Optimization

- **Loading**: nessun code splitting; librerie pesanti (AG Grid all modules, pdfmake + vfs_fonts, xlsx, Chart.js) caricate all'avvio.
- **Runtime**: analisi antifrode = per ogni pagina fetch + valutazione di tutte le regole su tutte le transazioni (O(pagine × regole × righe)); statistiche calcolate sulla prima pagina (≤ 3.000 righe). `markRaw` riduce l'overhead di reattività (buona pratica).
- Il report query è cacheato lato server 1 h: l'analisi pagina-per-pagina lo sfrutta, ma rischia dati stantii.

## 7. Testing Strategy

Nessun test (unit, component, E2E). Priorità: Vitest sulle funzioni pure di `aiStore` (`computeStats`, `classifyFields`, `build*Rules`) e `queryUtils.buildTypeAwareCondition`; Playwright sui journey principali.

## 8. Build & Deployment

- **La build fallisce su Linux/macOS** (verificato con `npm ci && npm run build`, Node 24): 12 import con maiuscole/minuscole diverse dal nome file (es. `../stores/passwordResetStore` vs `PasswordResetStore.ts`, `@/stores/workspaceStore` vs `workspacestore.ts`, `./chartBlock.vue` vs `chartblock.vue`) funzionano solo su file system case-insensitive (Windows). Qualsiasi pipeline CI/container Linux è bloccata finché non vengono corretti.
- `ScheduleForm.vue` importa `@/store/useDataIngestionStore`, file inesistente: il componente non è referenziato da nessuna parte (dead code) e quindi non rompe la build.
- Correggendo **temporaneamente** gli import (solo a fini di verifica, modifica non applicata al codice) la build riesce: bundle JS unico **5,7 MB (2,0 MB gzip)** + CSS 1,1 MB, nessun code splitting.
- `vite build` senza `vue-tsc --noEmit` → errori di tipo non bloccano la build.
- Nessun `.env.example`; `VITE_API_BASE_URL` obbligatoria ma non documentata nel repo UI.
- Deploy statico (SWA/Nginx) proposto nei Docs.

## 9. Accessibility (a11y)

Nessuna verifica WCAG. Vuetify fornisce componenti accessibili di base; rischi su heatmap (solo colore), grid con menu contestuali, dialog complessi. Raccomandato audit Lighthouse/axe.

## 10. Developer Experience

Nessun ESLint/Prettier/EditorConfig; nome pacchetto `workflowbuilder`, README template Vite; alias `@` configurato; TypeScript con molti `any` (es. `rows: any[]`, `classification: any`).

## 11. Third-Party Dependencies

| Pacchetto | Versione | Stato | Azione |
|-----------|----------|-------|--------|
| `xlsx` | 0.18.5 | High, nessun fix su npm | Sostituire con `exceljs` o SheetJS da CDN ufficiale |
| `jspdf`, `jspdf-autotable` | 3.0.1 / 5.0.2 | Critical, **non usate** | Rimuovere |
| `nuxt`, `@nuxt/devtools` | 3.x / 1.6 | High/Critical, **non usate** | Rimuovere |
| `grid-layout-plus`, `vuedraggable` | — | Non usate | Rimuovere |
| `axios`, `vite`, `vue` | 1.13 / 6.4 / 3.5 | High, fix disponibile | Aggiornare |
| `tabulator-tables` | 6.3 | Duplica AG Grid | Valutare rimozione |

## 12. Metrics & KPI

| Metrica | Valore |
|---------|--------|
| SLOC `src/` | 14.663 |
| Bundle produzione | JS 5,7 MB (2,0 MB gzip), CSS 1,1 MB |
| Build su Linux | ❌ fallisce (12 import case-sensitive) |
| Componenti / store / route | 41 / 15 / 17 |
| Funzioni / CCN medio | 569 / 3,1 |
| Funzioni CCN > 15 | 14 (max 87) |
| Vulnerabilità npm | 71 (6 C / 46 H / 15 M / 4 L) |
| Test | 0 |

## 13. Issues & Technical Debt

| ID | Debito | Impatto | Effort |
|----|--------|---------|--------|
| FE-TD1 | `aiStore.ts` god store (LLM + statistica + regole) | Manutenibilità, testabilità | 8–12 gg (split in moduli puri + porting server) |
| FE-TD2 | `resultsGrid.vue` god component | Manutenibilità | 6–10 gg |
| FE-TD3 | `eval` per campi calcolati | Sicurezza | 2–3 gg (parser espressioni sicuro) |
| FE-TD4 | Lock dati client-side | Sicurezza | 1 gg FE + backend |
| FE-TD5 | Dipendenze vulnerabili/inutilizzate | Sicurezza | 1–2 gg |
| FE-TD6 | Assenza test/lint/type-check | Qualità | 10–15 gg baseline |
| FE-TD7 | Nessun guard di permesso su route/menu | UX/sicurezza percepita | 2–3 gg |
| FE-TD8 | Import case-sensitive: build impossibile su Linux | Bloccante per CI/CD e container | 0,5 gg |

## 14. Recommendations

### 14.1 Quick Wins (< 1 settimana)
- Correggere i 12 import case-sensitive.
- Rimuovere `jspdf`, `jspdf-autotable`, `nuxt`, `@nuxt/devtools`, `grid-layout-plus`, `vuedraggable`, `nuxt.config.ts`; `npm audit fix`.
- Sostituire `eval` con un valutatore di espressioni whitelisted.
- Aggiungere `vue-tsc --noEmit` allo script build, ESLint + Prettier.
- Rendere URL/modello LLM configurabili via `VITE_*` come già documentato.

### 14.2 Short Term (1–3 mesi)
- Guard di permesso su route e menu (claim `permissions` del JWT).
- Spostare l'analisi antifrode su API asincrona; il FE mostra progress e risultati persistiti.
- Lazy loading delle route; split di `resultsGrid.vue` e `aiStore.ts`.
- Vitest + Playwright sui flussi principali.

### 14.3 Long Term (6–12 mesi)
- Cookie HttpOnly + refresh token al posto di `localStorage`.
- i18n e audit WCAG 2.1 AA.
- Design system interno e libreria grid unica.

---

## Reference Documents
- 07_code.md · 13_decision_log.md · 14_metrics.md

## Change Log

| Versione | Data | Autore | Modifiche |
|----------|------|--------|-----------|
| 1.0 | 2026-10-07 | REVERSE how (FULL) | Creazione documento (sostituisce run 2026-10-05) |
| 1.1 | 2026-10-07 | REVERSE how (FULL) | Esito build reale: import case-sensitive, dimensione bundle |
