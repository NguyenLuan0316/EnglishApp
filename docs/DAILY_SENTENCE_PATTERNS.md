# Daily sentence patterns

The existing 500-pattern seed catalog is supplemented by 1,000 English/Vietnamese sentences in `backend/WordWave.Api/Data/daily-patterns.json`.

The additional catalog contains 20 everyday topics with 50 sentences each: cafes, restaurants, groceries, clothing, directions, transport, hotels, housework, cooking, family, work, study, devices, appointments, leisure, wellbeing, deliveries, bills, lost belongings, and household repairs. Each topic practices ten common sentence structures with five relevant substitutions.

`meaning` remains the Vietnamese topic label used by the existing purpose filter. `explanation` contains the sentence-specific Vietnamese translation. `examples` contains the English sentence and its translation. The existing search, pagination, and pronunciation controls work without frontend changes.

The JSON file is embedded in the API assembly, including published builds. Startup validates the additional catalog and inserts missing sentences, comparing trimmed English sentences without case sensitivity. Existing rows, IDs, edits, and custom content are preserved. Repeated sequential startup does not insert duplicates. Simultaneous startup across multiple API instances is not protected by a database uniqueness constraint.

No schema migration is required. Start or redeploy the updated API to load missing content through the existing startup seeder. Preparing this change does not modify a running database.

Checks:

```powershell
dotnet build backend/WordWave.sln --no-restore
dotnet test backend/WordWave.Api.Tests/WordWave.Api.Tests.csproj --filter FullyQualifiedName~PatternCatalogTests
dotnet test backend/WordWave.Application.Tests/WordWave.Application.Tests.csproj
```

The catalog tests verify the packaged resource, topic sizes, complete translations, uniqueness, compatibility with the seed loader, and no overlap with the existing 500 sentences. They do not connect to a database.
