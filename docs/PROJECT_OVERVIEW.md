# WordWave Project Overview

## 1. Tổng Quan

WordWave là app học tiếng Anh gồm frontend SPA viết bằng HTML/CSS/JavaScript thuần và backend .NET Web API. Frontend tập trung vào học từ vựng, ngữ pháp, mẫu câu và các bài luyện tập như flashcard, quiz, nối từ, ghép câu, điền từ, luyện nghe và ôn tập theo spaced repetition. Backend cung cấp dữ liệu từ vựng, ngữ pháp, mẫu câu và API ghi nhận tiến độ ôn tập.

## 2. Cấu Trúc Thư Mục

```text
wordwave/
├── README.md
├── docs/
│   └── PROJECT_OVERVIEW.md
├── frontend/
│   ├── index.html
│   ├── assets/
│   │   ├── favicon.svg
│   │   └── logo.svg
│   ├── css/
│   │   └── style.css
│   └── js/
│       ├── application/state/appState.js
│       ├── domain/vocabulary.js
│       ├── infrastructure/api/apiClient.js
│       ├── presentation/app.js
│       ├── presentation/routing/router.js
│       └── presentation/pages/
│           ├── dashboard.js
│           ├── vocabulary.js
│           ├── grammar.js
│           ├── patterns.js
│           ├── flashcard.js
│           ├── quiz.js
│           ├── matching.js
│           ├── builder.js
│           ├── fillblank.js
│           ├── listening.js
│           ├── review.js
│           └── progress.js
└── backend/
    ├── WordWave.sln
    ├── WordWave.Api/
    ├── WordWave.Application/
    ├── WordWave.Domain/
    ├── WordWave.Infrastructure/
    └── WordWave.Application.Tests/
```

## 3. Frontend Pages, Routes Và File Phụ Trách

| URL | Page nội bộ | Chức năng | File chính |
| --- | --- | --- | --- |
| `/`, `/home`, `/dashboard` | `dashboard` | Tổng quan tiến độ, word of the day, shortcut học nhanh, chủ đề gợi ý | `frontend/js/presentation/pages/dashboard.js` |
| `/vocab`, `/vocabulary` | `vocabulary` | Danh sách từ vựng, lọc theo level/topic, tìm kiếm, phát âm | `frontend/js/presentation/pages/vocabulary.js` |
| `/grammar` | `grammar` | Danh sách bài ngữ pháp, lọc level, mở chi tiết công thức và ví dụ | `frontend/js/presentation/pages/grammar.js` |
| `/speaking`, `/patterns` | `patterns` | Mẫu câu giao tiếp, nghĩa, giải thích, ví dụ và phát âm | `frontend/js/presentation/pages/patterns.js` |
| `/practice`, `/flashcard` | `flashcard` | Luyện flashcard, lật thẻ, tự đánh giá, cộng XP | `frontend/js/presentation/pages/flashcard.js` |
| `/quiz` | `quiz` | Trắc nghiệm nghĩa của từ, tính điểm và ghi nhận review | `frontend/js/presentation/pages/quiz.js` |
| `/matching` | `matching` | Nối từ tiếng Anh với nghĩa tiếng Việt | `frontend/js/presentation/pages/matching.js` |
| `/writing`, `/builder` | `builder` | Ghép câu tiếng Anh từ các token cho sẵn | `frontend/js/presentation/pages/builder.js` |
| `/reading`, `/fillblank` | `fillblank` | Điền từ vào câu, chọn đáp án và kiểm tra | `frontend/js/presentation/pages/fillblank.js` |
| `/listening` | `listening` | Nghe phát âm, gõ lại từ nghe được, thống kê đúng/sai | `frontend/js/presentation/pages/listening.js` |
| `/review` | `review` | Ôn tập từ đến hạn, ghi nhận nhớ/không nhớ | `frontend/js/presentation/pages/review.js` |
| `/roadmap`, `/progress` | `progress` | Thống kê tiến độ theo level/topic và heatmap hoạt động | `frontend/js/presentation/pages/progress.js` |

Ghi chú: source hiện tại chưa có page độc lập tên `speaking`, `reading`, `writing` hoặc `roadmap`. Các URL này được map vào module hiện có gần nhất để đáp ứng routing rõ ràng mà không thêm UI mới.

## 4. Luồng Hoạt Động Chính

1. `frontend/index.html` chứa layout sidebar, topbar, container của từng page và import module JavaScript.
2. `frontend/js/presentation/routing/router.js` đọc URL hiện tại, render page tương ứng, cập nhật active state của sidebar và dùng History API để đổi URL không reload trang.
3. Mỗi page module có hàm `init...()` để tải dữ liệu, render nội dung và đăng ký handler cần thiết trên `window` cho các `onclick` trong HTML.
4. `frontend/js/infrastructure/api/apiClient.js` gọi backend. Khi chạy local, API base là `http://localhost:10000/api`; khi deploy dùng Render URL.
5. `frontend/js/application/state/appState.js` giữ state runtime như XP, flashcard, quiz, matching, listening và review. XP được lưu vào `localStorage`.
6. Backend nhận request qua controller, gọi service trong Application, service gọi repository trong Infrastructure, repository đọc database qua EF Core/Npgsql.

## 5. Cách Chạy Local

### Backend

```powershell
cd backend
dotnet run --project WordWave.Api/WordWave.Api.csproj
```

API mặc định chạy tại:

```text
http://localhost:10000
```

Health check:

```text
http://localhost:10000/api/health
```

### Frontend

Khuyến nghị serve thư mục `frontend` làm web root để các route như `/vocab` hoạt động đúng:

```powershell
cd frontend
npx serve . -s -l 5500
```

Sau đó mở:

```text
http://localhost:5500/
http://localhost:5500/vocab
```

Nếu dùng VS Code Live Server và mở từ root repo theo URL dạng `http://127.0.0.1:5500/frontend/index.html`, click menu vẫn đổi URL không reload. Tuy nhiên refresh trực tiếp `/vocab` có thể 404 nếu server không fallback về `frontend/index.html`.

## 6. Static Server Fallback Cho SPA

Vì app dùng History API, server static cần trả về `index.html` cho các route frontend như `/vocab`, `/grammar`, `/practice`.

Ví dụ với `serve`:

```powershell
cd frontend
npx serve . -s -l 5500
```

Ví dụ cấu hình Netlify:

```text
/* /index.html 200
```

Ví dụ cấu hình Nginx khi `frontend` là web root:

```nginx
location / {
  try_files $uri $uri/ /index.html;
}
```

## 7. Backend API Endpoints

| Method | Endpoint | Mô tả |
| --- | --- | --- |
| `GET` | `/api/health` | Health check |
| `GET` | `/api/vocabulary?level=&topic=&search=&page=&limit=` | Lấy danh sách từ vựng có phân trang/lọc |
| `GET` | `/api/vocabulary/random?level=&topic=&count=` | Lấy từ vựng ngẫu nhiên |
| `GET` | `/api/vocabulary/topics` | Lấy danh sách topic |
| `GET` | `/api/vocabulary/{id}` | Lấy một từ theo id |
| `GET` | `/api/grammar?level=` | Lấy bài ngữ pháp, có thể lọc level |
| `GET` | `/api/grammar/{id}` | Lấy một bài ngữ pháp |
| `GET` | `/api/patterns` | Lấy mẫu câu |
| `GET` | `/api/patterns/{id}` | Lấy một mẫu câu |
| `GET` | `/api/review/daily` | Lấy từ cần ôn hôm nay |
| `POST` | `/api/review/submit` | Ghi nhận kết quả ôn tập `{ wordId, correct }` |
| `GET` | `/api/review/progress` | Lấy tiến độ học |

## 8. Model, Entity Và Database

Backend dùng Entity Framework Core với PostgreSQL/Npgsql, connection string tên `Supabase`.

Các entity chính:

- `VocabWord`: map bảng `vocabulary`, gồm `id`, `word`, `phonetic`, `meaning`, `example`, `example_meaning`, `level`, `topic`, `created_at`.
- `SentencePattern`: map bảng `sentence_patterns`, gồm `id`, `sentence`, `meaning`, `explanation`, `examples`, `created_at`.
- `GrammarLesson`: map bảng `grammar_lessons`, gồm `id`, `title`, `level`, `description`, `formula`, `tips`, `created_at` và collection `GrammarExamples`.
- `GrammarExample`: map bảng `grammar_examples`, gồm `id`, `lesson_id`, `en`, `vi`.
- `WordProgress`: map bảng `word_progress`, nhưng tiến độ review hiện tại đang lưu bằng in-memory dictionary trong `ReviewService`, chưa persist qua `DbContext`.

`AppDbContext` khai báo `Vocabulary`, `SentencePatterns`, `GrammarLessons`, `GrammarExamples` và cấu hình một số column snake_case.

## 9. Quy Ước Style/CSS

- CSS tập trung trong `frontend/css/style.css`.
- Theme dùng CSS variables trong `:root`: màu nền, text, accent, border radius, font, shadow.
- Component pattern hiện có: `.sidebar`, `.nav-item`, `.card`, `.btn`, `.badge`, `.tabs`, `.topic-pill`, `.word-card`, `.quiz-option`, `.match-item`, `.blank-input`, `.grammar-card`, `.modal`, `.toast`.
- Các page đang dùng nhiều inline style để tinh chỉnh layout trong markup/template string.
- Khi thêm chức năng mới, ưu tiên tái sử dụng class hiện có thay vì thêm style mới. Nếu bắt buộc thêm CSS, đặt tên theo pattern hiện tại và dùng lại biến `var(--...)`.
- Không thêm UI framework mới nếu không cần.

## 10. Ghi Chú Phát Triển Chức Năng Mới

- Thêm page mới bằng cách tạo module trong `frontend/js/presentation/pages/`, thêm `<div class="page" id="page-...">` trong `index.html`, đăng ký page/route trong `router.js`.
- Menu/sidebar đang dùng `onclick="navigate('pageId')"`, nên route URL được router tự push bằng History API.
- Nếu route mới cần hỗ trợ refresh trực tiếp, static server phải fallback về `index.html`.
- Giữ nguyên thiết kế hiện tại: không đổi màu, spacing, form, layout, animation khi chỉ thêm logic.
- Dữ liệu frontend nên đi qua `apiClient.js`; logic dùng chung đặt trong `shared/utils.js` hoặc `application/state/appState.js` nếu là state UI.

## 11. Điểm Có Thể Refactor Sau

- `index.html` chứa nhiều markup page và inline handler; có thể tách template page ra module nếu app lớn hơn.
- `presentation/app.js` gần như trùng bootstrap inline trong `index.html`; nên chọn một entrypoint duy nhất về lâu dài.
- Một số handler trong page module phụ thuộc `window.event` hoặc global `window.*`; có thể chuyển sang `addEventListener` khi refactor.
- `ReviewService` lưu tiến độ bằng static in-memory dictionary, nên mất dữ liệu khi restart backend. Có thể persist bằng bảng `word_progress`.
- Cần thêm migration/seed hoặc tài liệu schema database để setup môi trường mới nhanh hơn.
