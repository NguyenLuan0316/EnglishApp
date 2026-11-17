# Account-specific daily learning

## Behavior

`/api/learning/daily` supplies a persistent plan for the authenticated account and study date. A study day follows UTC+7 (Vietnam/Bangkok), including server deployments using UTC. Each plan contains up to ten vocabulary items, two grammar lessons, and three sentence patterns. Due reviews come first, followed by unseen material. New vocabulary and grammar favor earlier CEFR levels. A deterministic account/date/content hash varies selection within a level. Plans keep their item IDs and ordering through reloads and API restarts; completing a plan does not create another plan on the same day. Deleted catalog items are omitted when rendering an existing plan.

The learner opens each lesson, reads its translation, explanation/formula and examples, and self-assesses as remembered or needing review. This is self-reported study, not a proctored assessment. Both choices complete the daily task; only a remembered answer contributes to the learned count and XP. A wrong answer is due immediately for practice and can appear in the next day's plan. Correct answers schedule the next review after 0, 1, 3, or 7 days according to cumulative recall ratio.

`/api/learning/submit` accepts `contentType` (`vocabulary`, `grammar`, or `patterns`), `contentId`, `correct`, and optional `daily`. Account identity comes only from the authenticated session. A daily submission must belong to that account's current plan. Completed daily tasks are idempotent: retries do not add attempts or XP again. An open plan from yesterday must be reloaded after midnight. Ordinary practice submissions can repeat because they represent separate attempts.

## Shared account progress

Vocabulary progress remains in `user_word_progress`, preserving all existing learned counts. New grammar/pattern progress lives in `user_content_progress`. `user_daily_lessons` stores daily tasks and completion; `user_study_activity` stores daily attempts, remembered counts, and XP. All new rows reference the account with cascade deletion. Per-account PostgreSQL transaction locks serialize plan creation and recording. Daily completion, progress and activity updates commit in one transaction.

All six practice pages submit their source catalog IDs to the same account progress system. Vocabulary review through the legacy `/api/review/submit` endpoint also records daily activity. Every newly saved remembered attempt earns 10 XP. Previous XP stored only in a browser is not imported into server totals, and historical activity dates are not invented from aggregate vocabulary counts.

`/api/learning/progress` returns learned totals for all three content types, vocabulary counts by level/topic, current daily completion, lifetime recorded attempts/recall, XP, streak, and the past 364 days of recorded activity. A content item is learned after at least one remembered attempt. A streak includes consecutive study days ending today, or yesterday if today's study has not begun. The first unanswered day breaks the streak after that grace day.

Dashboard, Statistics and the sidebar use this server summary. No sample totals, random activity heatmap or hardcoded proficiency badge remain on these surfaces. Successful submissions publish the returned server summary; re-entering a page and signing in on another device reload account data. Failed requests show recovery controls rather than sample results. Switching accounts in the normal sign-in flow reloads the document; page loaders also ignore responses associated with a previous account or superseded load.

## Database deployment

The generated AuthDbContext migration is `20261116163243_AddDailyLearning`. It adds only the three new tables and their constraints; it does not modify or delete existing account, vocabulary or content rows. It uses the separate `__AuthMigrationsHistory` chain and does not depend on the incomplete legacy content migration baseline.

The reviewable incremental script is `docs/migrations/daily-learning.sql`. It expects `20261116093642_AddUserAuthentication` and its Auth history table to exist. It is wrapped in a transaction and guarded by migration history. A target's authentication baseline must be verified before applying the script. Alternatively, apply the AuthDbContext migration chain using the standard EF tooling and an explicitly confirmed connection target. Apply the database update before deploying the updated API/frontend. Schema changes are not applied automatically at API startup.

After explicit user approval, `20261116163243_AddDailyLearning` was applied to the local `wordwave_test` database at `127.0.0.1` on 2026-11-16. EF completed creation of all three tables and recorded the migration. No remote/deployed database was updated. PostgreSQL integration tests create uniquely named disposable databases on loopback, generate their test schemas, and remove those databases afterward; they never migrate an existing database.

## Verification

```powershell
dotnet build backend/WordWave.sln --no-restore
dotnet test backend/WordWave.Application.Tests/WordWave.Application.Tests.csproj --no-restore
dotnet test backend/WordWave.Api.Tests/WordWave.Api.Tests.csproj --no-restore --filter 'FullyQualifiedName~LearningApiTests|FullyQualifiedName~LearningStoreTests|FullyQualifiedName~PatternCatalogTests'
```

Database tests require a localhost PostgreSQL connection with permission to create/drop uniquely named test databases. They use `WORDWAVE_TEST_CONNECTION`, or the existing local development connection. Remote hosts are rejected.

```powershell
npm install --prefix "$env:TEMP/wordwave-practice-checks" jsdom@26 --no-audit --no-fund
$env:WORDWAVE_DOM_TEST_MODULES = "$env:TEMP/wordwave-practice-checks"
node --test scripts/tests/practice_data.test.mjs scripts/tests/daily_learning.test.mjs scripts/tests/study_books.test.mjs
```

Service tests cover quotas, due-first ordering, stable plans, account isolation, local midnight, streaks, missing/deleted content, and review scheduling. PostgreSQL/API tests cover actual persistence across contexts, concurrent requests, daily idempotency, account/type isolation, cascade deletion, legacy review compatibility, authentication, CSRF, validation and error responses. DOM tests cover learning/reveal/resume, failed saves/retries, shared Dashboard/Statistics summaries, account switching, late responses, and transport events.

Desktop/mobile browser checks exercised daily learning, save failure/retry, reload/resume, Dashboard/Statistics synchronization, account switching, all six practice routes, and both localhost/deployed API-base branches with mocked API responses. Live deployed availability/content was not tested. The migration was generated and inspected and EF reports no pending Auth model changes; the incremental migration was subsequently applied to the explicitly approved local target described above.
