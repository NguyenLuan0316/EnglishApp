// wordwave/frontend/js/api.js
const BASE = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
  ? `http://${window.location.hostname}:10000/api`
  : 'https://englishapp-er2b.onrender.com/api';
  
let csrfToken = null;
async function fetchJson(url, options = {}) {
  const method = options.method || 'GET';
  const headers = new Headers(options.headers);
  if (!['GET', 'HEAD'].includes(method)) {
    if (!csrfToken) {
      const csrf = await fetch(`${BASE}/auth/csrf`, { credentials: 'include', cache: 'no-store' });
      if (!csrf.ok) throw new Error('Không thể xác thực yêu cầu. Vui lòng tải lại trang.');
      csrfToken = (await csrf.json()).token;
    }
    headers.set('X-CSRF-TOKEN', csrfToken);
  }
  let res;
  try {
    res = await fetch(url, { ...options, headers, credentials: 'include', cache: 'no-store' });
  } catch {
    throw new Error('Không kết nối được máy chủ. Kiểm tra kết nối và thử lại.');
  }
  if (!res.ok) {
    const problem = await res.json().catch(() => ({}));
    const validation = problem.errors ? Object.values(problem.errors).flat().join(' ') : '';
    const error = new Error(problem.detail || validation || (res.status === 429 ? 'Bạn thao tác quá nhanh. Vui lòng thử lại sau một phút.' : res.status === 403 ? 'Bạn không có quyền thực hiện thao tác này.' : `Yêu cầu thất bại (${res.status}).`));
    error.status = res.status;
    if (res.status === 403 && problem.title === 'Invalid request verification token') csrfToken = null;
    if (res.status === 401 && !['/auth/me', '/auth/login', '/auth/register'].some(path => url === `${BASE}${path}`)) window.dispatchEvent(new Event('wordwave:session-expired'));
    throw error;
  }
  return res.status === 204 ? null : res.json();
}

async function postJson(url, body) {
  return fetchJson(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
}

async function postForm(url, formData) {
  return fetchJson(url, {
    method: 'POST',
    body: formData,
  });
}

async function recordLearning(payload) {
  const result = await postJson(`${BASE}/learning/submit`, payload);
  window.dispatchEvent(new CustomEvent('wordwave:learning-updated', { detail: result.summary }));
  return result;
}

export const api = {
  me: () => fetchJson(`${BASE}/auth/me`),
  login: async payload => {
    const user = await postJson(`${BASE}/auth/login`, payload);
    csrfToken = null;
    return user;
  },
  register: payload => postJson(`${BASE}/auth/register`, payload),
  logout: async () => {
    await postJson(`${BASE}/auth/logout`, {});
    csrfToken = null;
  },
  changePassword: async payload => {
    await fetchJson(`${BASE}/auth/password`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
    csrfToken = null;
  },
  getUsers: (params = {}) => fetchJson(`${BASE}/admin/users?${new URLSearchParams(params)}`),
  createUser: payload => postJson(`${BASE}/admin/users`, payload),
  updateUser: (id, payload) => fetchJson(`${BASE}/admin/users/${id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }),
  deleteUser: id => fetchJson(`${BASE}/admin/users/${id}`, { method: 'DELETE' }),
  resetUserPassword: (id, newPassword) => fetchJson(`${BASE}/admin/users/${id}/password`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ newPassword }) }),
  updateUserAccess: (id, payload) => fetchJson(`${BASE}/admin/users/${id}/access`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }),
  // Vocabulary
  getVocab:    (params = {}) => fetchJson(`${BASE}/vocabulary?${new URLSearchParams(params)}`),
  getTopics:   ()            => fetchJson(`${BASE}/vocabulary/topics`),
  getTopicSummaries: (params = {}) => fetchJson(`${BASE}/vocabulary/topics/summary?${new URLSearchParams(params)}`),
  getLevels:   ()            => fetchJson(`${BASE}/vocabulary/levels`),
  getRandom:   (params = {}) => fetchJson(`${BASE}/vocabulary/random?${new URLSearchParams(params)}`),
  getWordById: (id)          => fetchJson(`${BASE}/vocabulary/${id}`),

  // Grammar
  getGrammar:    (level = '') => fetchJson(`${BASE}/grammar${level ? '?level=' + level : ''}`),
  getGrammarById:(id)         => fetchJson(`${BASE}/grammar/${id}`),

  // Patterns
  getPatterns: (params = {}) => fetchJson(`${BASE}/patterns?${new URLSearchParams(params)}`),

  // Review
  getDaily:    ()       => fetchJson(`${BASE}/review/daily`),
  getProgress: ()       => fetchJson(`${BASE}/review/progress`),
  submitReview: (wordId, correct) => recordLearning({ contentType: 'vocabulary', contentId: wordId, correct }),
  getLearningDaily: () => fetchJson(`${BASE}/learning/daily`),
  getLearningProgress: () => fetchJson(`${BASE}/learning/progress`),
  submitLearning: payload => recordLearning(payload),

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

  // IELTS
  getIeltsTests: () => fetchJson(`${BASE}/ielts/tests`),
  getIeltsTestById: id => fetchJson(`${BASE}/ielts/tests/${id}`),
  saveIeltsAttempt: (id, learnerId, stateData) => fetchJson(`${BASE}/ielts/tests/${id}/attempt`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ stateData }),
  }),
  submitIeltsTest: (id, learnerId, stateData) => postJson(`${BASE}/ielts/tests/${id}/submit`, { stateData }),
};
