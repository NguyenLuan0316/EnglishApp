# TOEIC Data Import

This document describes the implemented TOEIC import and crawl pipeline in WordWave. It is grounded in `AdminToeicController`, `ToeicImportService`, the importer/crawler adapters under `backend/WordWave.Infrastructure/Toeic/`, and the TOEIC entity graph in `AppDbContext`.

## Important access and content warning

The routes under `/api/admin/toeic` are named as admin routes but have no authentication or authorization in the current codebase. Do not expose them to untrusted clients until access control is implemented.

Only import content that WordWave is permitted to store and redistribute. `ToeicImportPackageWriter` requires `sourceName`, `license`, and `contentOwner`, but it only checks that these values are non-empty; it does not verify the legal claims.

## Implemented components

| Responsibility | Code |
| --- | --- |
| HTTP upload/crawl endpoints | `backend/WordWave.Api/Controllers/AdminToeicController.cs` |
| Import orchestration and log queries | `backend/WordWave.Application/Services/ToeicImportService.cs` |
| Shared package validation and persistence | `ToeicImportPackageWriter` in the same service file |
| JSON parser | `backend/WordWave.Infrastructure/Toeic/JsonToeicImporter.cs` |
| Flat-row CSV parser | `backend/WordWave.Infrastructure/Toeic/CsvToeicImporter.cs` |
| AI-generated JSON discriminator | `backend/WordWave.Infrastructure/Toeic/AiGeneratedToeicImporter.cs` |
| HTTP JSON crawler and robots check | `backend/WordWave.Infrastructure/Toeic/WebsiteToeicCrawler.cs` |
| Database access | `backend/WordWave.Infrastructure/Repositories/ToeicRepository.cs` |
| Import contracts | `backend/WordWave.Application/Contracts/Toeic/ToeicImportModels.cs` |
| Entity relationships and seed data | `backend/WordWave.Infrastructure/Persistence/AppDbContext.cs` |

The `ai-generated` adapter inherits the JSON parser, but no dedicated HTTP endpoint currently selects it. The upload service selects only `json` or `csv`.

## Data model

The migration `backend/WordWave.Infrastructure/Migrations/20260515090619_AddToeicDataImport.cs` creates:

- `toeic_tests`: test metadata, source, license, owner, publication flag, and creation time.
- `toeic_parts`: parts 1 through 7 within a test.
- `toeic_questions`: prompts, question text, image URL, difficulty, explanation, and optional passage/audio links.
- `toeic_answers`: answer options with one option marked correct by import validation.
- `toeic_passages`: reading/context passages owned by a part.
- `toeic_audios`: audio URL/local path/transcript owned by a part.
- `toeic_import_logs`: success, partial, or failure details for validated save attempts.

`AppDbContext` configures cascade delete from tests to parts and from parts to their questions, passages, and audios. Deleting a passage or audio sets the corresponding optional question foreign key to null.

## JSON package

`JsonToeicImporter` deserializes case-insensitively, permits comments and trailing commas, and fills a missing `sourceType` with `json` and a missing `sourceName` with the uploaded filename.

```json
{
  "title": "Admin TOEIC Mini Test",
  "description": "Original practice content created by the administrator.",
  "sourceType": "json",
  "sourceName": "Admin upload",
  "sourceUrl": "",
  "license": "Owned by the content owner and permitted for WordWave use.",
  "contentOwner": "WordWave Admin",
  "parts": [
    {
      "partNumber": 5,
      "name": "Incomplete Sentences",
      "instructions": "Choose the best answer.",
      "questions": [
        {
          "questionNumber": 1,
          "questionText": "The invoice was sent _____ email.",
          "difficulty": "easy",
          "explanation": "Use 'by' for the method.",
          "answers": [
            { "label": "A", "answerText": "by", "isCorrect": true },
            { "label": "B", "answerText": "on", "isCorrect": false }
          ]
        }
      ]
    },
    {
      "partNumber": 7,
      "name": "Reading Comprehension",
      "instructions": "Read the passage and answer the question.",
      "passages": [
        {
          "key": "email-1",
          "title": "Office Email",
          "content": "The training session begins at 10 a.m. in Room 402."
        }
      ],
      "questions": [
        {
          "questionNumber": 2,
          "passageKey": "email-1",
          "questionText": "Where will the training take place?",
          "answers": [
            { "label": "A", "answerText": "Room 402", "isCorrect": true },
            { "label": "B", "answerText": "The lobby", "isCorrect": false }
          ]
        }
      ]
    }
  ]
}
```

Passage and audio associations are resolved by case-insensitive keys within each part. A missing referenced key does not currently produce a validation error; the question is saved without that relationship.

## CSV package

`CsvToeicImporter` treats each non-empty physical line as one row. Rows with the same `partNumber` and `questionNumber` become one question, and each row contributes an answer. Headers are normalized by removing non-alphanumeric characters and lowercasing them.

```csv
title,description,sourceType,sourceName,sourceUrl,license,contentOwner,partNumber,partName,instructions,passageKey,passageTitle,passageContent,audioKey,audioUrl,audioLocalPath,audioTranscript,questionNumber,prompt,questionText,imageUrl,difficulty,explanation,answerLabel,answerText,isCorrect
Admin TOEIC CSV,Original content,csv,Admin CSV,,Owned by administrator,WordWave Admin,5,Incomplete Sentences,Choose the best answer.,,,,,,,,1,,The invoice was sent _____ email.,,easy,Use by for method.,A,by,true
Admin TOEIC CSV,Original content,csv,Admin CSV,,Owned by administrator,WordWave Admin,5,Incomplete Sentences,Choose the best answer.,,,,,,,,1,,The invoice was sent _____ email.,,easy,Use by for method.,B,on,false
```

The parser supports quoted commas and doubled quotes on a single line. It does not implement multiline quoted CSV fields.

## Validation and save behavior

`ToeicImportPackageWriter.SaveAsync` enforces these rules:

- A title, source type, source name, license, and content owner are required.
- At least one part is required, and valid part numbers are 1 through 7.
- A question number must be greater than zero.
- A question needs either `questionText` or `prompt`.
- A question needs at least two answers and exactly one correct answer.
- Every answer needs a non-empty label and text.

Invalid questions are skipped. If at least one question remains, the test is saved with status `partial`; if none remain, only a failure log is written. Valid imported tests are always assigned `IsPublic = true`.

Parsing, network, deserialization, and database exceptions occur before or outside the validation-result path and are handled by the API's global 500 Problem Details response. Those exceptions are not guaranteed to create a TOEIC import log.

## HTTP endpoints

### Import administration

| Method | Route | Input and result |
| --- | --- | --- |
| `POST` | `/api/admin/toeic/import/json` | Multipart field `file`, maximum request size 20 MB |
| `POST` | `/api/admin/toeic/import/csv` | Multipart field `file`, maximum request size 20 MB |
| `POST` | `/api/admin/toeic/crawl` | JSON `{ "keyword": "toeic", "sourceUrl": "https://..." }` |
| `GET` | `/api/admin/toeic/import-logs?page=1&limit=50` | Paged import logs; limit range is 1 to 200 |

The crawl request validates that `keyword` is present and at most 100 characters and that `sourceUrl` is a URL. The crawler itself requires an absolute HTTP or HTTPS URL. `keyword` is passed through the pipeline but is not used to filter the downloaded data.

### Learner-facing TOEIC API

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/api/toeic/tests` | Lists public tests with part/question counts |
| `GET` | `/api/toeic/tests/{id}` | Returns one public test without correct-answer flags |
| `GET` | `/api/toeic/questions?part=1` | Returns questions, optionally filtered to part 1-7 |
| `POST` | `/api/toeic/tests/{id}/submit` | Scores submitted answer IDs and returns correct answers/explanations |

Submit body:

```json
{
  "answers": [
    { "questionId": 1, "answerId": 1 },
    { "questionId": 2, "answerId": 5 }
  ]
}
```

Duplicate submissions for the same question use the last answer. Unanswered questions count toward the denominator. Attempts and scores are not persisted.

## Crawler behavior

`WebsiteToeicCrawler`:

1. Validates an absolute HTTP or HTTPS source URL.
2. Fetches `/robots.txt` from the source host.
3. Refuses the crawl if the robots request fails or if its simple `User-agent: *`/`Disallow` parser rejects the path.
4. Downloads the source URL as text.
5. Deserializes it as the WordWave JSON package.
6. Sends the normalized package to `ToeicImportPackageWriter`.

The current robots parser is intentionally small and does not implement the complete robots exclusion standard. The crawler has no host allowlist or private-network blocking, so arbitrary URL fetching is a security-sensitive area.

## Add an importer or crawler

1. Read `IToeicImporter`, `IToeicDataSourceCrawler`, and `IToeicImportPackageWriter` under `backend/WordWave.Application/Interfaces/`.
2. Normalize source-specific input into `ToeicImportPackage`; do not put parsing logic in a controller.
3. Preserve source, license, and content-owner metadata.
4. Delegate validation/persistence to `IToeicImportPackageWriter.SaveAsync`.
5. Register the adapter in `backend/WordWave.Api/Program.cs`.
6. Add application/infrastructure tests for parsing, validation, partial imports, and failure behavior.
7. Coordinate any schema changes through an EF Core migration; never edit a generated migration or model snapshot by hand.

Do not download or persist media files until a storage adapter, size/content validation, access policy, and licensing policy are explicitly designed. The current model only stores a URL or local-path string.
