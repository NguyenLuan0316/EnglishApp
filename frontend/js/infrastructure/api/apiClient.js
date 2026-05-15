// wordwave/frontend/js/api.js
const BASE = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
  ? 'http://localhost:10000/api'
  : 'https://englishapp-er2b.onrender.com/api';
  
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

async function postForm(url, formData) {
  const res = await fetch(url, {
    method: 'POST',
    body: formData,
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

  // TOEIC admin
  importToeicJson: (file) => {
    const form = new FormData();
    form.append('file', file);
    return postForm(`${BASE}/admin/toeic/import/json`, form);
  },
  importToeicCsv: (file) => {
    const form = new FormData();
    form.append('file', file);
    return postForm(`${BASE}/admin/toeic/import/csv`, form);
  },
  crawlToeic: (payload) => postJson(`${BASE}/admin/toeic/crawl`, payload),
  getToeicImportLogs: (params = {}) => fetchJson(`${BASE}/admin/toeic/import-logs?${new URLSearchParams(params)}`),

  // TOEIC user
  getToeicTests: () => fetchJson(`${BASE}/toeic/tests`),
  getToeicTestById: (id) => fetchJson(`${BASE}/toeic/tests/${id}`),
  getToeicQuestions: (params = {}) => fetchJson(`${BASE}/toeic/questions?${new URLSearchParams(params)}`),
  submitToeicTest: (id, answers) => postJson(`${BASE}/toeic/tests/${id}/submit`, { answers }),
};
