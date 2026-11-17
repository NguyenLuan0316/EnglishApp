export function escapePracticeText(value) {
  return String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
}

// Keep contractions and hyphenated words intact, while retaining punctuation tokens.
export function tokenizeSentence(sentence) {
  return String(sentence).match(/[A-Za-z]+(?:['’\-][A-Za-z]+)*|\d+(?:[.,]\d+)*|[^\sA-Za-z\d]/g) || [];
}

export function collectPracticeSentences(words = [], lessons = [], patterns = []) {
  const entries = [];
  const add = (sentence, translation, source, hint = '', contentId = null) => {
    if (typeof sentence !== 'string' || typeof translation !== 'string') return;
    sentence = sentence.trim();
    translation = translation.trim();
    const tokens = tokenizeSentence(sentence);
    const wordCount = tokens.filter(token => /[A-Za-z]/.test(token)).length;
    if (!translation || wordCount < 3 || wordCount > 24 || /___|\.{3}|…|[<>]/.test(sentence)) return;
    entries.push({ contentId, sentence, translation, source, hint, words: tokens });
  };
  words.forEach(word => add(word.example, word.exampleMeaning, 'vocabulary', word.meaning, word.id));
  lessons.forEach(lesson => (lesson.examples || []).forEach(example => add(example.en, example.vi, 'grammar', lesson.title, lesson.id)));
  patterns.forEach(pattern => {
    // Older catalog explanations describe usage rather than translate the sentence.
    const translation = pattern.explanation;
    if (typeof translation === 'string' && !/^Dùng\b/u.test(translation) && translation !== pattern.meaning) {
      add(pattern.sentence, translation, 'patterns', pattern.meaning, pattern.id);
    }
  });
  const seen = new Set();
  return entries.filter(entry => {
    const key = entry.sentence.toLowerCase().replace(/’/g, "'").replace(/\s+/g, ' ');
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

export function createBlankQuestion(entry, random = Math.random) {
  // Mask one whole word by its original character offsets; never replace a substring.
  const matches = [...entry.sentence.matchAll(/[A-Za-z]+(?:['’\-][A-Za-z]+)*/g)].filter(match => match[0].length > 2);
  if (!matches.length) return null;
  const chosen = matches[Math.floor(random() * matches.length)];
  return {
    before: entry.sentence.slice(0, chosen.index),
    after: entry.sentence.slice(chosen.index + chosen[0].length),
    answer: chosen[0],
    hint: entry.translation,
    source: entry.source,
    contentId: entry.contentId,
    context: entry.hint,
  };
}

export function normalizePracticeAnswer(value) {
  return String(value ?? '').trim().toLowerCase().replace(/’/g, "'").replace(/\s+/g, ' ');
}
