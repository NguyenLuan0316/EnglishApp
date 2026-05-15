# TOEIC Data Import

Tai lieu nay mo ta kien truc du lieu va cach import/crawl du lieu luyen TOEIC vao WordWave.

## Luu y ban quyen

- Khong import de thi TOEIC chinh thuc, sach thuong mai, audio/image co ban quyen, hoac noi dung khong co quyen su dung.
- Chi import tu file JSON/CSV do admin so huu, duoc cap phep, public domain, Creative Commons phu hop, hoac nguon public cho phep tai su dung.
- Moi file import phai co `sourceName`, `license`, va `contentOwner`.
- Website crawler phai kiem tra `robots.txt`. Neu robots.txt khong cho phep truy cap URL can crawl, he thong bo qua nguon do.
- Audio/image duoc luu bang URL hoac `localPath`. Neu can download file, hay them storage adapter rieng va chi download khi license cho phep.

## Cau truc du lieu

Bang TOEIC moi:

- `toeic_tests`: metadata cua test, nguon du lieu, license, owner.
- `toeic_parts`: 7 part TOEIC trong moi test.
- `toeic_questions`: cau hoi, prompt, image URL, passage/audio lien quan.
- `toeic_answers`: dap an va co `is_correct`.
- `toeic_passages`: doan doc cho Part 6/7 hoac ngu canh khac.
- `toeic_audios`: audio URL/local path/transcript cho Part 1-4.
- `toeic_import_logs`: log import/crawl, so cau thanh cong/loi va chi tiet loi.

Part duoc ho tro:

1. Picture Description
2. Question Response
3. Conversations
4. Talks
5. Incomplete Sentences
6. Text Completion
7. Reading Comprehension

## JSON mau

```json
{
  "title": "Admin TOEIC Mini Test",
  "description": "Original practice content created by the admin.",
  "sourceType": "json",
  "sourceName": "Admin uploaded file",
  "sourceUrl": "",
  "license": "Owned by admin and allowed for WordWave use.",
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
          "explanation": "The preposition 'by' is used for method.",
          "answers": [
            { "label": "A", "answerText": "by", "isCorrect": true },
            { "label": "B", "answerText": "on", "isCorrect": false },
            { "label": "C", "answerText": "with", "isCorrect": false },
            { "label": "D", "answerText": "for", "isCorrect": false }
          ]
        }
      ]
    },
    {
      "partNumber": 7,
      "name": "Reading Comprehension",
      "instructions": "Read the text and answer the questions.",
      "passages": [
        {
          "key": "email-1",
          "title": "Office Email",
          "content": "The training session will begin at 10 a.m. in Room 402."
        }
      ],
      "questions": [
        {
          "questionNumber": 2,
          "passageKey": "email-1",
          "questionText": "Where will the training take place?",
          "answers": [
            { "label": "A", "answerText": "Room 402", "isCorrect": true },
            { "label": "B", "answerText": "Lobby", "isCorrect": false }
          ]
        }
      ]
    }
  ]
}
```

## CSV mau

CSV gom moi dong la mot dap an. Nhieu dong co cung `partNumber` + `questionNumber` se duoc gom thanh mot cau hoi.

```csv
title,description,sourceType,sourceName,sourceUrl,license,contentOwner,partNumber,partName,instructions,passageKey,passageTitle,passageContent,audioKey,audioUrl,audioLocalPath,audioTranscript,questionNumber,prompt,questionText,imageUrl,difficulty,explanation,answerLabel,answerText,isCorrect
Admin TOEIC CSV,Original admin content,csv,Admin CSV,,Owned by admin,WordWave Admin,5,Incomplete Sentences,Choose the best answer.,,,,,,,,1,,The invoice was sent _____ email.,,easy,Use by for method.,A,by,true
Admin TOEIC CSV,Original admin content,csv,Admin CSV,,Owned by admin,WordWave Admin,5,Incomplete Sentences,Choose the best answer.,,,,,,,,1,,The invoice was sent _____ email.,,easy,Use by for method.,B,on,false
Admin TOEIC CSV,Original admin content,csv,Admin CSV,,Owned by admin,WordWave Admin,7,Reading Comprehension,Read and answer.,email-1,Office Email,The training starts at 10 a.m.,,,,,2,,When does the training start?,,easy,,A,10 a.m.,true
Admin TOEIC CSV,Original admin content,csv,Admin CSV,,Owned by admin,WordWave Admin,7,Reading Comprehension,Read and answer.,email-1,Office Email,The training starts at 10 a.m.,,,,,2,,When does the training start?,,easy,,B,2 p.m.,false
```

## Cach import

Admin API:

- `POST /api/admin/toeic/import/json`: upload multipart form field `file`.
- `POST /api/admin/toeic/import/csv`: upload multipart form field `file`.
- `POST /api/admin/toeic/crawl`: body `{ "keyword": "toeic", "sourceUrl": "https://example.com/allowed-toeic.json" }`.
- `GET /api/admin/toeic/import-logs?page=1&limit=50`: xem log import.

User API:

- `GET /api/toeic/tests`
- `GET /api/toeic/tests/{id}`
- `GET /api/toeic/questions?part=1`
- `POST /api/toeic/tests/{id}/submit`

Submit body:

```json
{
  "answers": [
    { "questionId": 1, "answerId": 1 },
    { "questionId": 2, "answerId": 5 }
  ]
}
```

## Cach them crawler source moi

Kien truc hien tai dung adapter:

- `IToeicImporter`: parser cho file do admin upload.
- `JsonToeicImporter`: import JSON theo schema WordWave.
- `CsvToeicImporter`: import CSV dang flat row.
- `AiGeneratedToeicImporter`: adapter cho noi dung AI-generated da duoc admin kiem duyet.
- `IToeicDataSourceCrawler`: interface crawler public source.
- `WebsiteToeicCrawler`: crawler generic, kiem tra robots.txt va normalize JSON schema WordWave.

De them nguon moi:

1. Tao class moi implement `IToeicDataSourceCrawler` hoac `IToeicImporter`.
2. Khong hard-code logic vao controller.
3. Kiem tra robots.txt/terms truoc khi fetch.
4. Normalize ve `ToeicImportPackage`.
5. Goi `IToeicImportPackageWriter.SaveAsync(...)` de validate va luu DB.
6. Dang ky adapter trong `Program.cs`.

Khuyen nghi: moi adapter source rieng nen co validation license/metadata rieng va test rieng.
