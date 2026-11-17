# WordWave Project Overview

This document describes the repository as implemented on 2026-07-20. It is the primary architecture and development reference for WordWave. Statements marked **Not confirmed from the current codebase** identify information for which the repository contains no supporting implementation or configuration.

## Contents

1. [Project overview](#1-project-overview)
2. [Technology stack](#2-technology-stack)
3. [Repository structure](#3-repository-structure)
4. [Architecture](#4-architecture)
5. [Main modules and business flows](#5-main-modules-and-business-flows)
6. [End-to-end execution flow](#6-end-to-end-execution-flow)
7. [Authentication and authorization flow](#7-authentication-and-authorization-flow)
8. [Database and persistence flow](#8-database-and-persistence-flow)
9. [Background processing flow](#9-background-processing-flow)
10. [External integration flow](#10-external-integration-flow)
11. [Local development](#11-local-development)
12. [Build and deployment](#12-build-and-deployment)
13. [Testing strategy](#13-testing-strategy)
14. [Common development workflows and conventions](#14-common-development-workflows-and-conventions)
15. [Findings, risks, and unclear areas](#15-findings-risks-and-unclear-areas)

## 1. Project overview

WordWave is an English-learning application. Its static browser SPA provides vocabulary browsing, grammar and sentence-pattern reference material, flashcards, quizzes, matching, sentence building, fill-in-the-blank exercises, listening practice, spaced review, progress summaries, IELTS practice tests, and a TOEIC import administration screen.

The .NET API exposes vocabulary, grammar, sentence-pattern, review, TOEIC, and IELTS endpoints. PostgreSQL stores learning content, TOEIC imports/logs, and IELTS attempts. Review progress and most short practice-session state are not persisted in the database.

The repository contains no user/account module, authentication provider, authorization policy, email service, object-storage adapter, worker process, message broker, or general-purpose AI provider.

## 2. Technology stack

| Area | Implemented technology | Evidence |
| --- | --- | --- |
| Backend runtime | .NET 8 / ASP.NET Core Web API | `backend/WordWave.Api/WordWave.Api.csproj`, `Program.cs` |
| Backend language | C# with nullable reference types and implicit usings | All backend `.csproj` files |
| Persistence | Entity Framework Core and Npgsql | `WordWave.Infrastructure.csproj`, `AppDbContext.cs` |
| Database | PostgreSQL; connection is named `Supabase` | `Program.cs`, API appsettings files |
| API format | JSON, multipart uploads, RFC-style Problem Details responses | Controllers and `Program.cs` |
| Frontend | Static HTML/CSS and native ES modules; no UI framework | `frontend/index.html`, `frontend/js/` |
| Frontend state | Module-scoped objects plus browser `localStorage` | `appState.js`, page modules |
| Browser APIs | Fetch, History API, Speech Synthesis, and optional Speech Recognition | `apiClient.js`, `router.js`, `utils.js`, `ielts.js` |
| Tests | xUnit and Microsoft.NET.Test.Sdk | `WordWave.Application.Tests.csproj` |
| Container | Multi-stage .NET 8 Dockerfile for the API | `backend/WordWave.Api/Dockerfile` |

There is a `frontend/package-lock.json`, but it declares no packages. The frontend has no bundler, transpiler, lint script, or build script.

The API and Infrastructure projects reference different EF Core patch versions (`8.0.6` design tooling in Api and `8.0.25` runtime/design packages in Infrastructure), while Npgsql is `8.0.11` in both. This mismatch is a maintenance risk, not a separate architecture choice.

## 3. Repository structure

```text
.
|-- README.md
|-- TOEIC_DATA_IMPORT.md
|-- .codex/skills/wordwave-development/
|   |-- SKILL.md
|   `-- agents/openai.yaml
|-- docs/
|   `-- PROJECT_OVERVIEW.md
|-- frontend/
|   |-- index.html
|   |-- css/style.css
|   |-- assets/
|   `-- js/
|       |-- application/state/appState.js
|       |-- domain/vocabulary.js
|       |-- infrastructure/api/apiClient.js
|       |-- presentation/app.js
|       |-- presentation/components/pagination.js
|       |-- presentation/routing/router.js
|       |-- presentation/pages/*.js
|       `-- shared/utils.js
`-- backend/
    |-- WordWave.sln
    |-- WordWave.Api/
    |   |-- Controllers/
    |   |-- Models/Requests/
    |   |-- Data/                    # Startup seeders
    |   |-- Program.cs
    |   |-- appsettings*.json
    |   `-- Dockerfile
    |-- WordWave.Application/
    |   |-- Contracts/
    |   |-- Interfaces/
    |   |-- Interfaces/Repositories/
    |   `-- Services/
    |-- WordWave.Domain/Models/
    |-- WordWave.Infrastructure/
    |   |-- Persistence/AppDbContext.cs
    |   |-- Repositories/
    |   |-- Toeic/
    |   `-- Migrations/
    `-- WordWave.Application.Tests/Services/
```

Generated/dependency directories such as `bin`, `obj`, `node_modules`, and `.git` are not part of the application architecture. `.csproj.lscache` files exist locally but are not tracked by Git.

## 4. Architecture

### 4.1 Backend layers

```text
HTTP request
    |
    v
WordWave.Api
  Controller + request model + DI/runtime configuration
    |
    v
WordWave.Application
  Service + DTO/contract + repository interface
    |
    v
WordWave.Infrastructure
  EF repository / external adapter -> AppDbContext -> PostgreSQL
    |
    v
WordWave.Domain
  Entity model shared by Application and Infrastructure
```

Project references enforce most of this shape:

| Project | Responsibility | Project dependencies |
| --- | --- | --- |
| `WordWave.Domain` | Persistence-oriented domain entities | None |
| `WordWave.Application` | Use cases, mappings, scoring/business rules, contracts, ports | Domain |
| `WordWave.Infrastructure` | EF Core context/repositories, migrations, TOEIC import/crawl adapters | Application, Domain |
| `WordWave.Api` | HTTP composition root, controllers, validation response, error middleware, CORS, startup seeding | Application, Infrastructure |
| `WordWave.Application.Tests` | Unit tests with hand-written repository fakes | Application, Domain |

`Program.cs` is the backend composition root. It registers `AppDbContext`, feature services/repositories, all `IToeicImporter` implementations, and a typed `HttpClient` for `WebsiteToeicCrawler`.

### 4.2 Frontend layers

| Directory/file | Responsibility |
| --- | --- |
| `frontend/index.html` | Static shell, sidebar, page containers, inline active bootstrap module |
| `presentation/routing/router.js` | URL aliases, canonical paths, History API navigation, page initialization |
| `presentation/pages/` | Feature-specific loading, rendering, browser interaction, and event handlers |
| `presentation/components/` | Reusable DOM rendering; currently pagination only |
| `infrastructure/api/apiClient.js` | API base selection and all implemented HTTP client methods |
| `application/state/appState.js` | Shared in-memory practice state and XP persistence |
| `domain/vocabulary.js` | Vocabulary level/topic constants and labels |
| `shared/utils.js` | Shuffle, speech synthesis, toast, confetti, badge/color, and DOM helpers |
| `css/style.css` | Global theme tokens and component/page styles |

`index.html` currently boots the application through a large inline `<script type="module">`. `presentation/app.js` contains a similar bootstrap but is not imported from `index.html`; it is therefore not the active browser entry point in the checked-in shell.

### 4.3 Where to add a feature

- Add persisted state as a Domain entity/mapping and Infrastructure migration.
- Add business behavior and response mapping in an Application service.
- Add database queries in an Infrastructure repository behind an Application interface.
- Add HTTP validation and transport handling in an Api request model/controller.
- Add browser transport methods in `apiClient.js` and UI behavior in a page module.
- Register a page in `router.js` and provide its `page-...` container in `index.html`.

Do not inject `AppDbContext` into controllers or put backend URLs directly into page modules. Existing code follows direct constructor injection rather than a mediator or generic repository abstraction.

## 5. Main modules and business flows

### 5.1 Vocabulary catalog

- **Purpose:** Browse, filter, search, select random words, list topics, and fetch a word by ID.
- **Entry points:** `VocabularyController`; `vocabulary.js`; dashboard global search and random word; several practice pages.
- **Core files:** `VocabularyService`, `IVocabularyRepository`, `VocabularyRepository`, `VocabWord`, `VocabularyQueryRequest`, and `apiClient.js`.
- **Data flow:** The controller maps query parameters into `VocabularyQuery`; the service delegates; the repository filters `AppDbContext.Vocabulary`, orders by ID, and applies pagination. Search lowercases and checks `Word` and `Meaning`. Random selection uses PostgreSQL random ordering.
- **Validation/rules:** Page must be at least 1; limit is 1-200; random count is 1-100. Vocabulary level/topic values are not constrained by request validation. Filters are exact matches; search is substring matching.
- **Persistence:** `vocabulary` table via `VocabWord`.
- **Output:** The list endpoint returns `{ total, page, limit, data }`; other endpoints return arrays or one entity. Entity objects are returned directly rather than mapped DTOs.
- **Errors:** Invalid query models use the configured 400 validation Problem Details. Missing ID uses `NotFoundProblem`. Frontend pages often replace failures with empty/error UI, while some calls intentionally swallow errors.
- **Permission:** None.

`apiClient.js` defines `getLevels()` for `/api/vocabulary/levels`, but no matching controller endpoint exists and the current page does not call it.

### 5.2 Grammar reference

- **Purpose:** List grammar lessons, optionally by CEFR level, and show lesson details/examples.
- **Entry points:** `GrammarController`; `grammar.js`.
- **Core files:** `GrammarService`, `GrammarRepository`, `GrammarLesson`, `GrammarExample`, and grammar DTOs.
- **Data flow:** The repository loads lessons with examples; the service performs case-insensitive level filtering in memory and maps entities to DTOs.
- **Validation/rules:** The optional level must match `A1`, `A2`, `B1`, `B2`, or `C1`.
- **Persistence:** `grammar_lessons` with cascade-related `grammar_examples` through `lesson_id`.
- **Startup behavior:** `GrammarDataSeeder` reads existing lessons by title, updates their fields/first example, adds missing lessons, and saves during API startup.
- **Output/errors/permission:** DTO list/detail; standard validation and 404 behavior; no permission.

### 5.3 Sentence patterns

- **Purpose:** Search and display conversational sentence patterns.
- **Entry points:** `PatternsController`; `patterns.js`.
- **Core files:** `PatternService`, `PatternRepository`, `SentencePattern`, pattern DTO/query contracts.
- **Data flow:** `PatternQueryRequest.Purpose` (or legacy `Type`) becomes `PatternQuery.Purpose`. The repository filters `Meaning` for exact equality and searches sentence, type, meaning, and explanation with PostgreSQL `ILIKE`.
- **Validation/rules:** No DataAnnotation limits exist for search/purpose. The current `Purpose` name does not match the actual `Meaning` equality filter.
- **Persistence:** `sentence_patterns`, including PostgreSQL `text[]` examples and an index on `type`.
- **Startup behavior:** `PatternDataSeeder` expects exactly 500 generated patterns. If row count, sentence sequence, or type sequence differs, it deletes all existing rows and recreates the seed set.
- **Output/errors/permission:** DTO list/detail; 404 for a missing ID; no permission.

### 5.4 Review and progress

- **Purpose:** Return up to 20 due vocabulary words, record correct/incorrect reviews, and summarize learned counts by level.
- **Entry points:** `ReviewController`; `review.js`, `progress.js`, `flashcard.js`, and `quiz.js`.
- **Core files:** `ReviewService`, review contracts, `WordProgress`, and `IVocabularyRepository`.
- **Data flow:** `ReviewService` loads vocabulary from the database but stores progress in a static process-local `Dictionary<int, WordProgress>`.
- **Rules:** The next interval is 7 days for at least 90% correct, 3 days for at least 70%, 1 day for at least 50%, and immediate otherwise. A new word is immediately due. A word is considered learned after any correct answer.
- **Validation:** `SubmitReviewRequest` has no DataAnnotations, and the service does not confirm that `wordId` exists before creating progress.
- **Persistence:** Vocabulary is persisted; review progress is not. `WordProgress` is not a `DbSet` in `AppDbContext`.
- **Output/errors:** Submit always returns success with progress. Process restart loses all review history, and concurrent access to the static dictionary is not synchronized.
- **Permission:** None; progress is global to the API process rather than scoped to a learner.

### 5.5 Browser-only practice activities

The following pages use vocabulary API data or constants, then keep their active session in `appState.js` or page-module variables:

| Page/module | Source and result |
| --- | --- |
| `flashcard.js` | Ten random vocabulary words; rating adds XP and submits review success for ratings 3+ |
| `quiz.js` | Up to 200 vocabulary rows; creates local distractors, scores locally, submits a review result |
| `matching.js` | Six random words; matches word/meaning pairs locally |
| `listening.js` | Up to 200 words; uses speech synthesis and checks typed spelling locally |
| `builder.js` | Hard-coded sentence/token exercises; local scoring and XP |
| `fillblank.js` | Hard-coded questions; local scoring and XP |
| `dashboard.js` | One random word plus local display statistics/shortcuts |

`appState.js` persists only XP under `ww_xp`. It reads `ww_streak` and `ww_level`, but no code in that module writes them. Practice session arrays and indices disappear on refresh. There is no server-side attempt or score persistence for these activities.

### 5.6 TOEIC learner API and scoring

- **Purpose:** List public TOEIC tests, return test/question content without correct flags, and score answer IDs.
- **Entry point:** `ToeicController`; methods exist in `apiClient.js`, but no learner TOEIC page/route is registered in `router.js`.
- **Core files:** `ToeicService`, `ToeicRepository`, TOEIC DTOs, and the `ToeicTest -> ToeicPart -> ToeicQuestion -> ToeicAnswer` entity graph.
- **Data flow:** Repository eager-loads the graph. Service maps answer options without `IsCorrect`. On submit, duplicate question answers use the last submitted answer; omitted answers are incorrect; score is correct/total as a percentage rounded to two decimals.
- **Validation/rules:** Optional part must be 1-7. The submit answer collection is required, but individual question/answer ID membership is evaluated only by scoring comparisons.
- **Persistence:** Test content is persisted; learner submissions/scores are not.
- **External service:** None for learner reads/scoring.
- **Errors/permission:** A test with no questions is treated as not found during submit. No permission.

See `../TOEIC_DATA_IMPORT.md` for the administration/import flow and schema.

### 5.7 TOEIC import administration

- **Purpose:** Upload JSON/CSV packages, crawl an HTTP JSON source, save valid test content, and view import logs.
- **Entry points:** `AdminToeicController`; `toeicAdmin.js`; `/admin/toeic` and `/toeic-admin` frontend routes.
- **Core files:** `ToeicImportService`, `ToeicImportPackageWriter`, importer/crawler interfaces, adapters under `WordWave.Infrastructure/Toeic`, and `ToeicRepository`.
- **Data flow:** Upload selects an importer by `SourceType`; crawl downloads and normalizes JSON; both delegate package validation/persistence to the writer. The writer may skip invalid questions and save a partial test, then writes an import log.
- **Validation/rules:** Upload request limit is 20 MB; package metadata and question/answer rules are documented in `TOEIC_DATA_IMPORT.md`. Imported tests are always public.
- **Persistence:** All TOEIC content tables plus `toeic_import_logs`.
- **External service:** The website crawler uses `HttpClient` and a simplified robots.txt check.
- **Errors:** Empty files return a non-Problem-Details `{ error }` 400. Parse/network/database exceptions reach the global 500 handler and may not be logged as import records.
- **Permission:** None despite the admin route and navigation label.

### 5.8 IELTS practice

- **Purpose:** List seeded IELTS tests, resume/save one attempt per test/learner ID, submit, and return objective plus heuristic productive-skill scores.
- **Entry points:** `IeltsController`; `ielts.js` at `/ielts` and `/ielts-test`.
- **Core files:** `IeltsService`, `IeltsRepository`, `IeltsTest`, `IeltsAttempt`, IELTS DTOs, and `IeltsDataSeeder`.
- **Data flow:** Test definitions are stored as JSONB. The service deserializes them, removes correct answers/key terms before returning the test, and combines the public test with an optional attempt. The frontend autosaves state and submits it using the constant learner ID `default`.
- **Validation/rules:** Request learner ID is at most 120 characters and defaults to `default`; state data is required. Repository uniqueness is `(ielts_test_id, learner_id)`. Objective answers use normalized exact matching. Listening/reading use fixed raw-to-band maps. Writing/speaking use word count, vocabulary variety, connectors, paragraphs, sentence length, and seed key terms; this is a local heuristic, not an official IELTS assessment or an AI evaluation.
- **Persistence:** `ielts_tests` and `ielts_attempts`; state/result JSONB, submission status, timestamps, and overall band are stored.
- **Startup behavior:** `IeltsDataSeeder` counts existing tests and adds generated tests until the total reaches ten. It assumes existing count/order can be used to select the next seed index.
- **Browser integration:** Speech synthesis reads listening transcript content; optional browser speech recognition can populate speaking responses.
- **Errors:** Missing test/attempt uses 404 Problem Details. Invalid stored test JSON can cause the global 500 response. Frontend failures display toasts.
- **Permission:** None. A caller can read or overwrite an attempt by knowing its learner ID; the shipped frontend makes all users share `default`.

## 6. End-to-end execution flow

### 6.1 Startup

1. `Program.cs` builds configuration and registers `AppDbContext` with the `Supabase` connection string.
2. Controllers, the custom model-validation response, Problem Details services, CORS, application services, repositories, importers, and crawler `HttpClient` are registered.
3. Exception, CORS, status-code, controller, and health-check middleware/endpoints are configured.
4. Grammar, pattern, and IELTS seeders run sequentially before the server starts listening. Seeder exceptions are logged and startup continues.
5. The API binds `http://0.0.0.0:<PORT>`, defaulting to port `10000`.

No call to `Database.Migrate`, `EnsureCreated`, or equivalent exists. The database schema must already support the seeders.

### 6.2 Browser request

```text
index.html inline module
  -> initRouter()
  -> URL normalized by router.js
  -> page container activated + initPage()
  -> page calls apiClient.js
  -> fetch http://localhost:10000/api/... (local hostnames)
  -> ASP.NET controller
  -> Application service
  -> Infrastructure repository
  -> AppDbContext / PostgreSQL
  -> JSON response
  -> page renders DOM and updates module/app state
```

`router.js` uses `history.pushState`/`replaceState`; direct refreshes require the static host to fall back to `index.html`.

### 6.3 Frontend routes

| Canonical path | Page module | Notes |
| --- | --- | --- |
| `/` | `dashboard.js` | Aliases `/home`, `/dashboard` |
| `/vocab` | `vocabulary.js` | Alias `/vocabulary` |
| `/grammar` | `grammar.js` | |
| `/speaking` | `patterns.js` | Alias `/patterns`; not the IELTS speaking module |
| `/practice` | `flashcard.js` | Alias `/flashcard` |
| `/quiz` | `quiz.js` | |
| `/matching` | `matching.js` | |
| `/writing` | `builder.js` | Alias `/builder` |
| `/reading` | `fillblank.js` | Alias `/fillblank` |
| `/listening` | `listening.js` | |
| `/ielts` | `ielts.js` | Alias `/ielts-test` |
| `/review` | `review.js` | |
| `/roadmap` | `progress.js` | Alias `/progress` |
| `/admin/toeic` | `toeicAdmin.js` | Alias `/toeic-admin`; unprotected |

Unknown paths render the dashboard. Canonical paths are defined separately in `PAGE_PATHS`.

### 6.4 Error flow

- `[ApiController]` model validation uses the configured 400 `ValidationProblemDetails` with a trace ID.
- Controllers inheriting `ApiControllerBase` use a consistent 404 `ProblemDetails` helper.
- Unhandled exceptions return `application/problem+json`; development responses expose the exception message, while other environments return a generic message.
- Empty status-code responses are converted to Problem Details by `UseStatusCodePages`.
- Default ASP.NET logging and explicit seeder-error logging are present. No structured logging provider, request tracing system, or persistent application log sink is configured in the repository.
- Frontend handling varies by page: some show a toast/error element, while several `.catch(() => {})` paths silently ignore failed progress submissions/searches.

### 6.5 Backend endpoint catalog

| Method | Route | Controller/action |
| --- | --- | --- |
| `GET` | `/api/health` | Minimal endpoint in `Program.cs` |
| `GET` | `/api/vocabulary` | `VocabularyController.GetAll` |
| `GET` | `/api/vocabulary/random` | `VocabularyController.GetRandom` |
| `GET` | `/api/vocabulary/topics` | `VocabularyController.GetTopics` |
| `GET` | `/api/vocabulary/{id}` | `VocabularyController.GetById` |
| `GET` | `/api/grammar` | `GrammarController.GetAll` |
| `GET` | `/api/grammar/{id}` | `GrammarController.GetById` |
| `GET` | `/api/patterns` | `PatternsController.GetAll` |
| `GET` | `/api/patterns/{id}` | `PatternsController.GetById` |
| `GET` | `/api/review/daily` | `ReviewController.GetDaily` |
| `POST` | `/api/review/submit` | `ReviewController.Submit` |
| `GET` | `/api/review/progress` | `ReviewController.GetProgress` |
| `GET` | `/api/toeic/tests` | `ToeicController.GetTests` |
| `GET` | `/api/toeic/tests/{id}` | `ToeicController.GetTestById` |
| `GET` | `/api/toeic/questions` | `ToeicController.GetQuestions` |
| `POST` | `/api/toeic/tests/{id}/submit` | `ToeicController.Submit` |
| `POST` | `/api/admin/toeic/import/json` | `AdminToeicController.ImportJson` |
| `POST` | `/api/admin/toeic/import/csv` | `AdminToeicController.ImportCsv` |
| `POST` | `/api/admin/toeic/crawl` | `AdminToeicController.Crawl` |
| `GET` | `/api/admin/toeic/import-logs` | `AdminToeicController.GetImportLogs` |
| `GET` | `/api/ielts/tests` | `IeltsController.GetTests` |
| `GET` | `/api/ielts/tests/{id}` | `IeltsController.GetTestById` |
| `GET` | `/api/ielts/tests/{id}/attempt` | `IeltsController.GetAttempt` |
| `PUT` | `/api/ielts/tests/{id}/attempt` | `IeltsController.SaveAttempt` |
| `POST` | `/api/ielts/tests/{id}/submit` | `IeltsController.Submit` |

There is no `/api/vocabulary/levels` action even though the unused `apiClient.getLevels` method constructs that URL.

## 7. Authentication and authorization flow

There is no implemented authentication or authorization flow.

Evidence:

- `Program.cs` does not register or invoke authentication/authorization middleware.
- No controller or action has `[Authorize]`, `[AllowAnonymous]`, role, claim, or policy attributes.
- There are no user, account, role, token, session, or permission models.
- `AdminToeicController` and the TOEIC admin frontend page are public.
- IELTS identity is a caller-supplied string, not a verified principal.
- Review progress is global process state rather than per-user state.

Required roles, identity provider, token/session format, and intended permission matrix: **Not confirmed from the current codebase**.

Any feature that depends on ownership or administration must first introduce or integrate a real identity model and enforce it server-side. Hiding a frontend navigation item is not authorization.

## 8. Database and persistence flow

### 8.1 Context and mapping

`backend/WordWave.Infrastructure/Persistence/AppDbContext.cs` is the persistence mapping source. It uses explicit snake_case table names and selected snake_case column mappings. Unmapped property names rely on Npgsql/EF conventions, so developers must inspect generated migrations rather than assume every column is snake_case.

| Aggregate/table(s) | Relationships and important storage |
| --- | --- |
| Vocabulary: `vocabulary` | Independent `VocabWord` rows |
| Grammar: `grammar_lessons`, `grammar_examples` | One-to-many; cascade delete by `lesson_id` |
| Patterns: `sentence_patterns` | `Examples` stored as PostgreSQL `text[]`; index on `Type` |
| TOEIC test: `toeic_tests`, `toeic_parts` | Test -> parts cascade |
| TOEIC content: `toeic_questions`, `toeic_answers`, `toeic_passages`, `toeic_audios` | Part owns content; answers cascade; passage/audio question references set null when target is deleted |
| TOEIC audit: `toeic_import_logs` | Independent import outcome rows |
| IELTS: `ielts_tests`, `ielts_attempts` | Test -> attempts cascade; unique test/learner; test/state/result use JSONB; band precision `(3,1)` |

`WordProgress` is a Domain model but is not configured in `AppDbContext` and is not persisted.

### 8.2 Migrations

The checked-in migration files are:

1. `20260515090619_AddToeicDataImport.cs`: creates and seeds TOEIC tables.
2. `20260518050000_AddPatternType.cs`: adds/indexes `type` on an already-existing `sentence_patterns` table and rewrites seed values.
3. `20260518100000_AddIeltsTests.cs`: creates IELTS test/attempt tables.

There is no checked-in initial migration that creates `vocabulary`, `grammar_lessons`, `grammar_examples`, or `sentence_patterns`. The current `AppDbContextModelSnapshot` lists the pre-existing content tables and TOEIC tables but does not list IELTS tables even though the IELTS migration and runtime context do. Therefore, creating a complete fresh database from this migration set is **not confirmed from the current codebase** and is likely to fail without an external baseline.

Do not run these migrations during ordinary documentation or feature discovery. For an authorized schema change, use the Infrastructure project with the Api startup project, inspect generated SQL/operations, and validate against a disposable database that begins from the real baseline.

### 8.3 Seed data

- TOEIC sample data is EF model seed data in `AppDbContext.SeedToeicData` and the TOEIC migration.
- Grammar, sentence patterns, and IELTS tests are application startup seeders in `WordWave.Api/Data`.
- The source/provisioning process for baseline vocabulary records is **Not confirmed from the current codebase**.

Startup seeders mutate the configured database on every API boot attempt. The pattern seeder is especially high risk because it performs a delete-and-recreate when its exact generated set is not present.

### 8.4 Configuration

`Program.cs` resolves `ConnectionStrings:Supabase`. Both checked-in appsettings files currently contain complete connection strings. This is a credential-exposure finding; values are intentionally not reproduced here. Use `ConnectionStrings__Supabase`, .NET user secrets, or the deployment secret store to override configuration.

## 9. Background processing flow

No background worker, hosted service, scheduler, queue, event bus, or message broker is implemented.

Potentially long operations are executed synchronously within their initiating lifecycle:

- Startup seeders run before `app.Run` starts listening.
- TOEIC upload parsing, website download, validation, database writes, and import-log writes run inside the HTTP request.
- IELTS autosave is a frontend timer that calls the API; it is not a server-side scheduled job.

Retry policy, idempotency strategy, dead-letter handling, distributed locks, and out-of-process job monitoring are **Not confirmed from the current codebase** because no asynchronous processing subsystem exists.

## 10. External integration flow

| Integration | Flow | Configuration/constraints |
| --- | --- | --- |
| PostgreSQL/Supabase | EF Core repositories -> Npgsql -> configured database | `ConnectionStrings:Supabase`; plaintext secret risk |
| TOEIC website source | Admin POST -> `WebsiteToeicCrawler` -> robots fetch -> source fetch -> JSON normalization -> writer -> PostgreSQL | Absolute HTTP/HTTPS only; simplified robots parser; no host/private-network allowlist |
| Render API deployment | Non-local frontend `apiClient.js` -> hard-coded Render API base | No environment-based frontend configuration |
| Netlify frontend | Allowed CORS origin and repository URL evidence | Deployment manifest is absent |
| Google Fonts | Browser loads Sora and DM Mono from Google hosts | Declared in `frontend/index.html` |
| Browser speech | Page -> Speech Synthesis; IELTS optionally uses Speech Recognition | Browser support/permission dependent |
| AI-generated TOEIC | `AiGeneratedToeicImporter` labels JSON as `ai-generated` | No AI service call, prompt, credentials, or endpoint selects this adapter directly |

Object storage and email integrations: **Not confirmed from the current codebase**. TOEIC media models store URL/local-path strings only; the crawler does not download media to managed storage.

## 11. Local development

### 11.1 Prerequisites

- .NET 8 SDK.
- PostgreSQL database with the required baseline schema and migrations.
- A static server capable of SPA fallback.
- Optional Node.js/npm for `npx serve`.
- Optional `dotnet-ef` tool only for authorized migration work.

### 11.2 Start the API

Prefer a configuration override rather than editing appsettings:

```powershell
$env:ConnectionStrings__Supabase = '<PostgreSQL connection string>'
dotnet run --project backend/WordWave.Api/WordWave.Api.csproj
```

`Program.cs` uses `PORT` or defaults to `10000`, so the expected health endpoint is:

```text
http://localhost:10000/api/health
```

The console message mentions port 5000, and `launchSettings.json` contains ports 52669/52670, but `app.Run("http://0.0.0.0:<PORT>")` is the binding implemented by the application. There is no Swagger UI registration despite the console text saying “Swagger/test.”

### 11.3 Start the frontend

```powershell
cd frontend
npx serve . -s -l 5500
```

Open `http://localhost:5500`. The static server must serve `index.html` for paths such as `/vocab` and `/ielts`. Opening the file directly with `file://` is not a supported equivalent because ES module and routing behavior differ.

Local frontend hostnames `localhost` and `127.0.0.1` target `http://localhost:10000/api`; every other hostname targets the hard-coded Render URL.

## 12. Build and deployment

### 12.1 Backend build

```powershell
dotnet restore backend/WordWave.sln
dotnet build backend/WordWave.sln
dotnet test backend/WordWave.sln
```

The solution contains all four runtime projects and the application test project.

### 12.2 Frontend build

There is no compilation step. Deploy `frontend` as static files and configure SPA fallback to `index.html`. Ensure the deployment origin is accepted by API CORS and that the non-local API base in `apiClient.js` is correct.

### 12.3 Container/deployment evidence

`backend/WordWave.Api/Dockerfile` publishes the API into an ASP.NET 8 runtime image and exposes port 10000. Its `COPY` statements assume a specific build context, but the repository does not document that context or test the image build. Because `WordWave.Api.csproj` references sibling projects, validate the Docker build before relying on it.

No GitHub Actions workflow, Render manifest, Netlify manifest, Infrastructure-as-Code, or release script is present. Exact production build commands, migration procedure, secret injection, rollback process, and health-monitoring configuration are **Not confirmed from the current codebase**.

## 13. Testing strategy

### 13.1 Existing tests

`WordWave.Application.Tests` contains two xUnit service tests:

- `GrammarServiceTests.GetByIdAsync_Should_Map_Examples`
- `PatternServiceTests.GetAllAsync_Should_Map_Domain_To_Dto`

Both use hand-written fake repositories and verify Domain-to-DTO mapping. They do not start ASP.NET or a database.

### 13.2 Missing coverage

No automated tests are present for:

- Vocabulary queries and validation.
- Review scheduling, invalid word IDs, concurrency, or restart behavior.
- TOEIC parsing, partial validation, crawling, repository graph loading, or scoring.
- IELTS sanitization, state upsert, scoring boundaries, or malformed JSON.
- Controllers, Problem Details, CORS, startup seeders, EF mappings, or migrations.
- Frontend routes, API client behavior, rendering, accessibility, or browser interactions.
- Docker image or deployment configuration.

### 13.3 Expected verification for changes

- Add service unit tests for new business rules and mappings.
- Add repository/integration tests when changing queries, mappings, migrations, or transaction behavior.
- Add API tests when changing routing, validation, status codes, or authorization.
- For frontend changes, serve the SPA, refresh each changed canonical path, test loading/empty/error/success states, and inspect browser console/network output.
- For TOEIC/IELTS changes, include boundary cases and verify that correct answers or scoring keys are not returned before submission.

## 14. Common development workflows and conventions

### 14.1 Naming and organization

- C#: PascalCase types/public members, `I` interface prefix, `_camelCase` private fields, `Async` suffix for asynchronous methods.
- Backend features: contracts under `Application/Contracts/<Feature>`, ports under `Application/Interfaces`, implementation under `Application/Services` or `Infrastructure/Repositories`.
- API request models: feature-specific files under `WordWave.Api/Models/Requests`, with DataAnnotations for transport constraints.
- JavaScript: camelCase variables/functions, exported `initFeature` page entry points, feature files in `presentation/pages`.
- Frontend containers/routes: page key `toeicAdmin` maps to element ID `page-toeicAdmin`, router metadata, aliases, and a canonical path.
- CSS: reuse `:root` variables and existing component classes in `style.css`; current pages also contain substantial inline styles.

### 14.2 Add a backend endpoint

1. Read the nearest controller, request model, Application service/interface, repository interface/implementation, and tests.
2. Add a request model only for HTTP binding/validation; put reusable contracts in Application.
3. Put business rules/mapping in the service and persistence queries in the repository.
4. Return DTOs instead of EF entities when the response can expose internal fields.
5. Register new implementations in `Program.cs`.
6. Preserve configured validation, 404 Problem Details, and global exception behavior.
7. Add tests, build the solution, and exercise the route.

### 14.3 Add a frontend page

1. Add `frontend/js/presentation/pages/<feature>.js` with `initFeature`.
2. Add transport methods to `apiClient.js`.
3. Add a `page-<feature>` container and optional navigation item to `index.html`.
4. Import/register the page, aliases, and canonical path in `router.js`.
5. Expose handlers on `window` only when inline `onclick` markup requires them.
6. Add/reuse CSS and verify direct route refresh with SPA fallback.
7. Render external/database content safely; many current pages use `innerHTML`, so escaping is a required design consideration.

### 14.4 Change the database

1. Read the entity, `AppDbContext`, all affected repository queries, existing migrations/snapshot, and startup seeders.
2. Update entity/mapping.
3. Generate a migration using Infrastructure as the migration project and Api as startup project.
4. Review generated operations and SQL; do not hand-edit the snapshot to hide model drift.
5. Validate against a disposable database built from the real baseline.
6. Run a migration on a shared/production database only with explicit target approval, backup/rollback planning, and deployment coordination.

### 14.5 Add a TOEIC integration

Follow `../TOEIC_DATA_IMPORT.md`. Implement `IToeicImporter` for file formats or `IToeicDataSourceCrawler` for remote sources, normalize to `ToeicImportPackage`, delegate persistence to `IToeicImportPackageWriter`, register DI, and test license metadata plus failure/partial behavior.

### 14.6 Files to read first

| Change | Read first |
| --- | --- |
| Runtime/DI/API errors/CORS | `Program.cs`, affected controller, `ApiControllerBase.cs` |
| Backend feature | Feature controller, request, service/interface, repository interface/implementation, entity/contracts |
| Database | Entity, `AppDbContext.cs`, repository, all migrations, affected seeder |
| Frontend page/route | `index.html`, `router.js`, `apiClient.js`, `appState.js`, analogous page, `style.css` |
| TOEIC | `TOEIC_DATA_IMPORT.md`, TOEIC controller/service/repository, adapters, entities/mapping |
| IELTS | `ielts.js`, `IeltsController`, request/DTOs, `IeltsService`, `IeltsRepository`, entities, seeder |
| Deployment | `Program.cs`, appsettings files without copying secrets, `apiClient.js`, Dockerfile, README |

### 14.7 Areas not to change casually

- Checked-in connection settings and deployment URLs.
- CORS origins and port/binding behavior.
- Generated migration designers or the EF model snapshot.
- Seed data, especially the destructive pattern-seeder behavior.
- TOEIC licensing, publication, URL-fetching, and correct-answer exposure rules.
- IELTS scoring formulas and client-visible sanitization.
- Public API shapes consumed by the framework-free frontend.

## 15. Findings, risks, and unclear areas

### 15.1 Confirmed findings and technical risks

1. **Committed database credentials:** Both API appsettings files contain plaintext credentials. Rotate exposed credentials and move secrets to environment/user-secret/deployment-secret configuration in a separately authorized security task.
2. **No access control:** All routes, including `/api/admin/toeic`, are public. IELTS learner IDs are untrusted client strings.
3. **Non-durable review state:** `ReviewService` uses unsynchronized static memory. Progress is global, lost on restart, and not safe for multi-instance deployment.
4. **Migration baseline/model drift:** Baseline content tables lack a creation migration, and the snapshot does not reflect IELTS. Fresh provisioning and future migration generation are unreliable until reconciled.
5. **Destructive startup seeding:** Pattern startup can delete/recreate the whole pattern table. All startup seeders run against the configured database before listening.
6. **Arbitrary outbound URL risk:** The TOEIC crawler accepts caller-supplied HTTP/HTTPS URLs without private-network blocking or a host allowlist. The robots parser is incomplete.
7. **Hard-coded environment coupling:** Frontend API URLs and backend CORS origins are source constants, complicating previews and additional environments.
8. **Duplicate frontend bootstrap:** Active inline startup logic in `index.html` overlaps the unused `presentation/app.js`, creating drift risk.
9. **Inconsistent runtime guidance:** Console output, launch settings, and actual `PORT` binding disagree; Swagger is mentioned but not configured.
10. **Limited error visibility:** Several frontend calls silently swallow failures, especially review submissions and global search.
11. **Potential unsafe HTML rendering:** Page modules frequently build `innerHTML` from API or imported content without a central escaping/sanitization layer.
12. **No persisted TOEIC attempts:** TOEIC scoring results are response-only; there is no learner ownership/history.
13. **Heuristic IELTS scoring:** Writing/speaking bands are local formulas and must not be represented as official examiner or AI scores.
14. **Dependency version skew:** EF Core packages use inconsistent patch versions across projects.
15. **Docker build uncertainty:** The Dockerfile's sibling-project/build-context behavior is undocumented and untested.

### 15.2 Not confirmed from the current codebase

- Intended user identity provider, account model, roles, and permission matrix.
- The authoritative baseline schema/provisioning process for vocabulary, grammar, and sentence patterns.
- The source and license/provenance of baseline vocabulary content.
- Production deployment commands, CI/CD, secret injection, migration/rollback process, monitoring, and backups.
- Whether `index.html` or `presentation/app.js` is intended to become the single long-term frontend bootstrap.
- A learner-facing TOEIC UI; only API client methods and the admin UI exist.
- Any email, managed file storage, background processing, queue, event bus, or actual AI-service integration.
- Required retention/privacy rules for IELTS attempt content, especially speaking/writing responses.
- Whether the exposed database credentials have already been rotated outside the repository.

### 15.3 Recommended next documentation and engineering tasks

Do not implement these without explicit follow-up authorization:

1. Create an approved security remediation plan for credential rotation, secret storage, authentication, admin authorization, and learner ownership.
2. Reconstruct and test an authoritative initial database migration; reconcile the model snapshot and document environment-safe migration/rollback procedures.
3. Decide whether startup seeders are development fixtures or production data management, then document and constrain them accordingly.
4. Define environment-based frontend/API configuration and document verified Render/Netlify/Docker deployment commands.
5. Consolidate the frontend bootstrap and add frontend lint/test tooling.
6. Add targeted tests for review scheduling, TOEIC import/scoring/crawl security, IELTS scoring/sanitization/upsert, controllers, and migrations.
7. Define official product wording and privacy/retention rules for IELTS heuristic feedback and stored free-text/speaking responses.
