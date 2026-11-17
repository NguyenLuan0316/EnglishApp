# Illustrated vocabulary and grammar library views

The vocabulary and grammar pages now offer two views using the existing WordWave design: illustrated books and the original list. The initial view is books. Explicit view-switch choices are remembered per library in browser storage; opening a book temporarily shows the list without replacing that preference. Storage restrictions do not prevent switching views.

## Behavior

- **Vocabulary:** one illustrated cover per actual topic. Count and available levels come from the authenticated `GET /api/vocabulary/topics/summary?level=...` endpoint, not the checked-in import manifest. The existing topic-keys endpoint and paged-word API stay compatible. Level filtering updates book counts. Topic search accepts Vietnamese with or without accents. Opening a book selects that topic in the existing paged list; a back control returns to the shelf. Word search, pronunciation and pagination remain available in list view.
- **Grammar:** A1, A2, B1, B2 and C1 books, as requested. Counts are grouped from actual lessons returned by the grammar API. The current local data has 25 A1, 25 A2, 25 B1 and 45 B2 lessons. C1 has no lessons and is explicitly disabled with “Chưa có bài học”; no launch date or artificial progress is shown. Search can match lesson titles, Vietnamese names or a level. The original expandable lesson cards, examples and paging remain available inside each book or through the list switch.
- API errors offer retry controls. Vocabulary request sequencing prevents older level/search responses from replacing the latest selection. Buttons have visible keyboard focus and selected states; the book count/title/level remain text, independent of the decorative illustrations.

## Design direction

This extends the existing dark application, rather than replacing its identity. The user-supplied screenshot establishes the physical-book shape and level-based grammar arrangement. The existing typography, navigation and learning flows stay in place. Colored covers, a binding strip, a page edge and genuine topic illustrations distinguish collections from ordinary data cards. Counts are real, covers are navigation controls, and no percentage-complete bars or locked-content claims are invented.

The grid adapts to the available content width and uses two columns on phones. Long Vietnamese topic names wrap inside the paper title plate. Empty grammar books retain readable text. The selected default book view lets users discover the feature immediately; both existing list views are one button away.

## Illustration provenance

The project asset is `frontend/assets/books/topic-atlas.webp`. It was generated using the built-in image generation tool as one transparent 8-column by 4-row illustration atlas, then converted from PNG to WebP at quality 86 using FFmpeg with transparency preserved. The original generated PNG remains in the local generated-images library. The screenshot was used to understand book-cover structure, not copied into the application.

Atlas cells follow the `TOPIC_LABELS` order: daily, food, travel, work, technology, health, education, science, society, shopping, family, weather, animals, environment, clothing, personality, emotions, sports, arts, home, finance, transport, communication, numbers, time, people, actions, colors, geography, culture, properties, manner. Grammar books reuse the education illustration. Unknown future topic keys get the education illustration as a safe fallback.

Generation prompt:

> Create ONE production web asset: a perfectly regular sprite atlas of 32 small editorial spot illustrations, arranged EXACTLY 8 columns by 4 rows, all 32 equal-size cells, no gutters, no borders, NO TEXT, NO letters, NO numbers, NO book covers or UI. Wide 2:1 canvas preferably 2048 by 1024. Transparent background throughout each cell. Each illustration occupies central 70% of its cell with ample completely transparent padding; absolutely no subject crosses cell boundaries. Beautiful hand-painted gouache / softly shaded colored pencil, rich cobalt blue, warm tomato orange, forest green and honey accents, cream highlights. Carefully drawn real objects with soft organic shapes, tasteful educational textbook cover art, consistent light and scale, suitable for dark and colored book covers. Subject order EXACT left-to-right, top-to-bottom: Row1: 1 daily life steaming teacup beside house keys; 2 food bowl of noodles and vegetables; 3 travel small suitcase and globe; 4 work briefcase and notebook; 5 technology laptop with mouse; 6 health stethoscope and small first-aid kit; 7 education open schoolbook and pencil; 8 science laboratory flask and small atom model. Row2: 9 society a few small houses around a public square; 10 shopping paper shopping bag and basket; 11 family three friendly small human figures parent parent child; 12 weather sun rain cloud; 13 animals a friendly fox and little bird; 14 environment leafy tree and sprout; 15 clothing folded sweater and scarf; 16 personality two different theatrical character masks. Row3: 17 emotions expressive smiling and thoughtful faces; 18 sports football and tennis racket; 19 arts paintbrush palette and theater ticket; 20 home cozy armchair and small lamp; 21 finance stacked coins and small bank building; 22 transport little bus and bicycle; 23 communication two overlapping speech bubbles and a fountain pen, no text inside; 24 numbers wooden counting beads and a ruler with tick marks but NO numerals. Row4: 25 time alarm clock and small hourglass; 26 people two friendly portrait busts; 27 actions hand placing a small block on another block; 28 colors arrangement of paint tubes and colored swatches; 29 geography folded map and compass without letters; 30 culture lantern and festive bunting; 31 properties contrasting smooth round stone and rough angular rock; 32 manner a feather beside a simple balance scale. Each cell must contain exactly its specified subject, don't add captions, don't merge cells, don't use a grid of icon glyphs. This is one transparent illustrated atlas to be sampled as CSS sprites, so exact uniform eight-by-four composition is essential.

## Verification

- .NET integration coverage validates aggregate counts, level-specific counts and an empty level against the imported 3,000-word dataset in disposable PostgreSQL.
- `scripts/tests/study_books.test.mjs` exercises both view switches, unique atlas cells, real counts, unaccented search, drill-down/back behavior, retained list paging/search, empty/error/retry states, stale request rejection, grammar level navigation and escaping of untrusted topic names.
- For DOM tests, install jsdom outside the production frontend and point `WORDWAVE_DOM_TEST_MODULES` at that dependency directory:

```powershell
npm install --prefix "$env:TEMP/wordwave-books-dom-tests" --no-audit --no-fund jsdom
$env:WORDWAVE_DOM_TEST_MODULES = "$env:TEMP/wordwave-books-dom-tests"
node --test scripts/tests/study_books.test.mjs
```

The application remains vanilla JavaScript; jsdom is a test-only dependency, not shipped to the browser. The change adds no database entities, migrations or modifications to learning content.
