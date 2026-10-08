# Copilot instructions

## Repository scope and commands

The application repository is rooted here. It contains a .NET solution and a separate Vue application in `LossPrevention.UI`.

### Backend

Run these commands from this repository root:

```powershell
dotnet restore .\LossPrevention.sln
dotnet build .\LossPrevention.sln
dotnet run --project ".\LossPrevention.API\01. LossPrevention.API.csproj"
dotnet run --project ".\LossPrevention.DataIngestionService\02. LossPrevention.DataIngestionService.csproj"
```

There are currently no .NET test projects, so no backend full-suite or single-test command exists.

### Frontend

Run these commands from `LossPrevention.UI`:

```powershell
npm install
npm run dev
npm run build
npm run preview
```

`package-lock.json` is currently out of sync with `package.json`; use `npm install` rather than `npm ci` until the lockfile is reconciled. No frontend test or lint script is configured, so there is no single-test or lint command.

## Architecture

- The backend is a .NET 8 layered modular monolith: `LossPrevention.API` exposes FastEndpoints and JWT-protected HTTP APIs; `LossPrevention.Application` contains application DTOs, handlers, validation, and services; `LossPrevention.Domain` owns the domain model; and `LossPrevention.Infrastructure` provides MongoDB repositories and external-service integrations.
- `LossPrevention.DataIngestionService` is a separate executable for ingestion work. API endpoints also expose ingestion configuration, scheduling, and manual run operations.
- MongoDB is the system of record. Collection data and initialization/maintenance scripts live in `Data\LossPrevention` and `Data\MongoDBScripts`; model new persistence work as document-oriented data, not relational schema.
- The Vue 3/Vite SPA (not Nuxt/SSR, despite the `nuxt` dependency and `nuxt.config.ts`) uses Composition API SFCs, Vuetify, Pinia, Vue Router, AG Grid, and Chart.js. Components are organized by feature under `src\components`; feature state and API calls are concentrated in the corresponding Pinia stores under `src\stores`.
- Reporting, fraud detection, dashboards, mappings, and field-level access controls span API endpoints, application/infrastructure services, and matching frontend stores/components. Trace a feature across all of those layers before changing its request shape or behavior.
- AI-assisted natural-language querying integrates with a local Ollama service at `http://localhost:11434`; retain the local integration model unless an explicit configuration change is requested.

## Codebase conventions

- Add backend HTTP operations as one FastEndpoints endpoint class per file in `LossPrevention.API\Endpoints\<feature>`. Follow the feature directory and endpoint naming already used there rather than adding controller-based APIs.
- Preserve the existing project references: API → Application + Infrastructure; Application → Domain + Infrastructure; Infrastructure → Domain. Domain entities carry `MongoDB.Bson` attributes, so they double as persistence documents; do not add references from Domain or Infrastructure back to Application/API.
- Keep MongoDB access behind `IMongoRepository<T>`/Infrastructure and route business logic through Application services. About 20 endpoints (Dashboard, Groups, Notifications, FraudDetection) inject repositories directly; do not extend that pattern in new endpoints. Vue components never talk to MongoDB.
- Reuse the existing request/response DTO and validation patterns when changing endpoint contracts. Keep authorization consistent with the JWT/RBAC and granular-permission model rather than introducing endpoint-local authorization schemes.
- Frontend views should obtain state and side effects through the relevant Pinia store, while `src\api\api.ts` remains the shared HTTP configuration boundary. Keep router access requirements aligned with the guards in `src\router\index.ts`.
- The repository contains mixed filename casing (for example, `rolestore.ts`, `workspacestore.ts`, `chartblock.vue`, `PasswordResetStore.ts`), and several existing imports already use the wrong casing, so the UI only builds on case-insensitive file systems (Windows/macOS default). In new or touched code, match the on-disk filename casing exactly.
- Treat report-query construction, fraud rules, JWT/authentication, permissions, and the Ollama integration as cross-cutting sensitive areas: preserve their existing contracts and validate affected API and UI paths together.

## Reference documentation

The full codebase analysis is maintained in `Docs\index.md`; use the linked architecture, code, data, frontend, and backend assessment documents for feature-specific context.
