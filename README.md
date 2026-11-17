# WordWave

WordWave is an English-learning application with a framework-free browser SPA and a .NET 8 Web API. It currently supports vocabulary study, grammar lessons, sentence patterns, practice activities, spaced review, TOEIC data import and scoring, and IELTS practice attempts with local heuristic scoring.

## Repository layout

```text
.
|-- backend/
|   |-- WordWave.Api/                 # HTTP entry point, controllers, startup seeders
|   |-- WordWave.Application/         # Use cases, contracts, and repository interfaces
|   |-- WordWave.Domain/              # Persistence entities
|   |-- WordWave.Infrastructure/      # EF Core, PostgreSQL repositories, TOEIC adapters
|   `-- WordWave.Application.Tests/   # xUnit application-service tests
|-- frontend/                         # Static HTML, CSS, and ES modules
|-- docs/PROJECT_OVERVIEW.md           # Architecture and development guide
|-- TOEIC_DATA_IMPORT.md               # TOEIC import schema and adapter guide
`-- .codex/skills/wordwave-development/ # Repository-specific coding-agent skill
```

## Prerequisites

- .NET 8 SDK
- A PostgreSQL database compatible with the current EF Core model
- A static HTTP server with SPA fallback support for the frontend
- Node.js only if using the `npx serve` example below

## Run locally

Configure the `Supabase` connection string through an environment variable or .NET user secrets. The environment-variable form is:

```powershell
$env:ConnectionStrings__Supabase = '<PostgreSQL connection string>'
```

Start the API:

```powershell
dotnet run --project backend/WordWave.Api/WordWave.Api.csproj
```

Unless `PORT` is set, `Program.cs` binds the API to `http://localhost:10000`. Verify it at `http://localhost:10000/api/health`.

Serve the frontend with SPA fallback:

```powershell
cd frontend
npx serve . -s -l 5500
```

Open `http://localhost:5500`. The frontend uses `http://localhost:10000/api` when its hostname is `localhost` or `127.0.0.1`.

The API does not apply migrations automatically. Do not run database migrations against a shared or production database without explicit approval. See [Project overview](docs/PROJECT_OVERVIEW.md#8-database-and-persistence-flow) for the current migration limitations.

## Build and test

```powershell
dotnet restore backend/WordWave.sln
dotnet build backend/WordWave.sln
dotnet test backend/WordWave.sln
```

The frontend has no build step or automated test suite in the current repository. It is loaded as native ES modules from `frontend/index.html`.

## Documentation

- [Project overview and architecture](docs/PROJECT_OVERVIEW.md)
- [TOEIC data import](TOEIC_DATA_IMPORT.md)

The detailed architecture guide is the source of truth for module flows, conventions, known risks, and feature-development workflows.
