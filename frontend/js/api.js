// wordwave/frontend/js/api.js
const BASE = 'http://localhost:5000/api';

async function fetchJson(url) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`API error ${res.status}`);
  return res.json();
}

async function postJson(url, body) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`API error ${res.status}`);
  return res.json();
}

export const api = {
  // Vocabulary
  getVocab:    (params = {}) => fetchJson(`${BASE}/vocabulary?${new URLSearchParams(params)}`),
  getTopics:   ()            => fetchJson(`${BASE}/vocabulary/topics`),
  getLevels:   ()            => fetchJson(`${BASE}/vocabulary/levels`),
  getRandom:   (params = {}) => fetchJson(`${BASE}/vocabulary/random?${new URLSearchParams(params)}`),
  getWordById: (id)          => fetchJson(`${BASE}/vocabulary/${id}`),

  // Grammar
  getGrammar:    (level = '') => fetchJson(`${BASE}/grammar${level ? '?level=' + level : ''}`),
  getGrammarById:(id)         => fetchJson(`${BASE}/grammar/${id}`),

  // Patterns
  getPatterns: () => fetchJson(`${BASE}/patterns`),

  // Review
  getDaily:    ()       => fetchJson(`${BASE}/review/daily`),
  getProgress: ()       => fetchJson(`${BASE}/review/progress`),
  submitReview:(wordId, correct) => postJson(`${BASE}/review/submit`, { wordId, correct }),
};
