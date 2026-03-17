# WordWave – Học Tiếng Anh

## Cấu trúc project

```
wordwave/
├── backend/
│   └── WordWave.Api/          # .NET 8 Web API (C#)
│       ├── Controllers/
│       ├── Data/
│       ├── Models/
│       ├── Program.cs
│       └── WordWave.Api.csproj
└── frontend/                  # Vanilla HTML/CSS/JS (không framework)
    ├── css/
    │   └── style.css
    ├── js/
    │   ├── api.js             # Gọi backend API
    │   ├── state.js           # Quản lý state
    │   ├── router.js          # SPA router
    │   ├── utils.js           # Helper functions
    │   └── pages/
    │       ├── dashboard.js
    │       ├── vocabulary.js
    │       ├── grammar.js
    │       ├── flashcard.js
    │       ├── quiz.js
    │       ├── matching.js
    │       ├── builder.js
    │       ├── fillblank.js
    │       ├── listening.js
    │       ├── review.js
    │       └── progress.js
    └── index.html
```

## Chạy project

### 1. Backend (.NET 8)
```bash
cd backend/WordWave.Api
dotnet run
# API chạy tại: http://localhost:5000
```

### 2. Frontend
```bash
cd frontend
# Mở index.html trong trình duyệt, HOẶC dùng Live Server (VSCode extension)
# Cài: Extensions → tìm "Live Server" → Install → Click "Go Live"
```

## API Endpoints
| Method | URL | Mô tả |
|--------|-----|-------|
| GET | /api/vocabulary | Lấy từ vựng (filter: level, topic, search) |
| GET | /api/vocabulary/random | Lấy từ ngẫu nhiên |
| GET | /api/grammar | Lấy bài ngữ pháp |
| GET | /api/patterns | Lấy mẫu câu |
| GET | /api/review/daily | Từ cần ôn hôm nay |
| POST | /api/review/submit | Ghi nhận kết quả |
| GET | /api/review/progress | Tiến độ học |
