# Practice content sources

Practice uses the same API catalogs as the learning sections:

| Practice page | Content source |
| --- | --- |
| Flashcards | Random vocabulary, filtered by selected level |
| Multiple-choice quiz | A fresh random vocabulary sample of up to 100 words per session |
| Matching | Random vocabulary |
| Sentence builder | Vocabulary example sentences, grammar examples, and translated sentence patterns |
| Fill in the blank | Vocabulary example sentences, grammar examples, and translated sentence patterns |
| Listening | One random vocabulary word at the selected level per question |

`frontend/js/application/practice/practiceData.js` loads all three catalogs for each sentence-practice session. It uses the existing API client and does not change backend contracts. Available sources are sampled evenly so a large sentence-pattern catalog does not crowd out grammar or vocabulary. If a source fails, the learner sees a warning and can continue with the available sources. Empty or unavailable content clears the previous question and offers recovery through the existing restart controls.

`frontend/js/domain/practice.js` normalizes the bilingual examples, excludes missing translations and unsuitable fragments, removes duplicate sentences, preserves contractions and punctuation, and creates single-word blanks using original character positions. The fill-in task asks learners to restore the original library sentence; it does not claim to accept all grammatically valid paraphrases.

Legacy sentence-pattern explanations starting with `Dùng` describe usage rather than translate the sentence and are excluded from translation-based exercises. The additional 1,000 daily patterns contain per-sentence translations and are eligible once the updated API has loaded them. No standalone hardcoded sentence bank remains in the builder or fill-in pages.

Repeated grading is guarded, restart cancels pending question timers, and outdated loading responses are ignored. Practice entries retain their catalog IDs. Vocabulary exercises submit vocabulary progress; sentence building and fill-in exercises submit progress for the source vocabulary item, grammar lesson, or sentence pattern. Saved results update the account learning summary through `/api/learning/submit`; see `DAILY_LEARNING.md`.

## Verification

The frontend tests use Node.js and JSDOM. Dependencies can be installed outside the repository:

```powershell
npm install --prefix "$env:TEMP/wordwave-practice-checks" jsdom@26 --no-audit --no-fund
$env:WORDWAVE_DOM_TEST_MODULES = "$env:TEMP/wordwave-practice-checks"
node --test scripts/tests/practice_data.test.mjs
```

The tests cover all three sources, duplicate/invalid entries, all 1,000 additional patterns, partial failures, empty states, repeat grading, pending timers, vocabulary quiz refresh, listening level changes, outdated responses, and existing flashcard/matching data loading.

Browser verification covered the six practice routes at desktop and mobile sizes with mocked API responses, including the localhost and deployed API-base branches. It verified correct fill-in grading, empty-state recovery, no horizontal page overflow in the mobile fill-in view, and no uncaught JavaScript errors. Live API availability and deployed content were not verified.
