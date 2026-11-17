import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { test } from 'node:test';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const require = createRequire(path.join(process.env.WORDWAVE_DOM_TEST_MODULES || root, 'package.json'));
const { JSDOM } = require('jsdom');
const html = (await readFile(path.join(root, 'frontend/index.html'), 'utf8')).replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi, '');
const dom = new JSDOM(html, { url: 'http://localhost:5500/review' });
Object.assign(globalThis, { window: dom.window, document: dom.window.document, localStorage: dom.window.localStorage, CustomEvent: dom.window.CustomEvent });
const from = relative => import(pathToFileURL(path.join(root, 'frontend/js', relative)).href);
const { api } = await from('infrastructure/api/apiClient.js');
const { state } = await from('application/state/appState.js');
const { authState } = await from('application/state/authState.js');
const { initReview } = await from('presentation/pages/review.js');
const { initDashboard } = await from('presentation/pages/dashboard.js');
const { initProgress } = await from('presentation/pages/progress.js');
const realSubmit = api.submitLearning;
const types = ['vocabulary', 'grammar', 'patterns'];
const stores = new Map();
let rejectSave = false;
const store = () => {
  if (!stores.has(state.userId)) stores.set(state.userId, { completed: new Map(), calls: 0 });
  return stores.get(state.userId);
};
const summary = () => {
  const data = store();
  const correct = [...data.completed.values()].filter(Boolean).length;
  return { date: '2026-11-16', totalLearned: correct, totalXp: correct * 10, streak: data.completed.size ? 1 : 0, attempts: data.completed.size, correctCount: correct, todayAttempts: data.completed.size, dailyTotal: 3, dailyCompleted: data.completed.size, byContent: Object.fromEntries(types.map(type => [type, { total: 1, learned: data.completed.get(type) ? 1 : 0 }])), byLevel: { A1: { total: 1, learned: data.completed.get('vocabulary') ? 1 : 0 } }, byTopic: { daily: { total: 1, learned: data.completed.get('vocabulary') ? 1 : 0 } }, activity: data.completed.size ? [{ date: '2026-11-16', attempts: data.completed.size, correctCount: correct, xp: correct * 10 }] : [] };
};
const daily = () => ({ date: '2026-11-16', total: 3, completed: store().completed.size, lessons: types.map((type, index) => ({ contentType: type, contentId: index + 1, title: type === 'vocabulary' ? '<script>home</script>' : `${type} lesson`, meaning: 'Nghĩa tiếng Việt', detail: 'Nội dung bài học', examples: ['We learn every day.', 'Chúng tôi học mỗi ngày.'], level: type === 'patterns' ? '' : 'A1', isNew: true, completed: store().completed.has(type), correct: store().completed.get(type) ?? null })) });
api.getLearningDaily = async () => daily();
api.getLearningProgress = async () => summary();
api.submitLearning = async request => {
  store().calls++;
  if (rejectSave) throw new Error('Mất kết nối');
  await Promise.resolve();
  if (!store().completed.has(request.contentType)) store().completed.set(request.contentType, request.correct);
  const result = { recorded: true, summary: summary() };
  window.dispatchEvent(new CustomEvent('wordwave:learning-updated', { detail: result.summary }));
  return result;
};
const selectUser = id => {
  state.setUser(id);
  authState.user = { id, displayName: id };
};
const flush = async () => { await new Promise(resolve => setImmediate(resolve)); };
selectUser('first');

test('daily study reveals actual lessons, saves once and resumes from persisted plan', async () => {
  await initReview();
  assert.equal(document.querySelectorAll('.daily-lesson-row').length, 3);
  assert.equal(document.querySelectorAll('#review-lessons script').length, 0);
  window.startReview();
  assert.ok(document.getElementById('daily-lesson-body').hidden);
  document.getElementById('daily-reveal').click();
  assert.ok(!document.getElementById('daily-lesson-body').hidden);
  const remember = document.querySelector('[data-daily-answer="true"]');
  remember.click();
  remember.click();
  await flush();
  assert.equal(store().calls, 1);
  assert.equal(state.xp, 10);
  assert.match(document.getElementById('daily-lesson-title').textContent, /grammar/);
  await initReview();
  assert.match(document.getElementById('review-remaining').textContent, /1\/3/);
  window.startReview();
  assert.match(document.getElementById('daily-lesson-title').textContent, /grammar/);
});

test('failed saves keep the lesson pending and allow retry', async () => {
  document.getElementById('daily-reveal').click();
  rejectSave = true;
  document.querySelector('[data-daily-answer="true"]').click();
  await flush();
  assert.match(document.getElementById('daily-save-feedback').textContent, /Chưa lưu được/);
  assert.equal(store().completed.size, 1);
  assert.equal(document.querySelector('[data-daily-answer="true"]').disabled, false);
  rejectSave = false;
  document.querySelector('[data-daily-answer="false"]').click();
  await flush();
  assert.equal(store().completed.get('grammar'), false);
  assert.equal(state.xp, 10);
});

test('dashboard and progress use the saved account summary and display no invented activity', async () => {
  await initDashboard();
  assert.equal(document.getElementById('dashboard-vocabulary').textContent, '1');
  assert.equal(document.getElementById('dashboard-grammar').textContent, '0');
  assert.equal(document.getElementById('dashboard-daily').textContent, '2/3');
  await initProgress();
  assert.equal(document.getElementById('progress-learned').textContent, '1');
  assert.equal(document.getElementById('progress-accuracy').textContent, '50%');
  assert.equal(document.querySelectorAll('.heat-cell').length, 364);
  assert.equal([...document.querySelectorAll('.heat-cell')].filter(cell => !cell.title.endsWith(': 0 lượt học')).length, 1);
  selectUser('second');
  await initDashboard();
  await initReview();
  await initProgress();
  assert.equal(document.getElementById('dashboard-vocabulary').textContent, '0');
  assert.equal(document.getElementById('dashboard-daily').textContent, '0/3');
  assert.equal(document.getElementById('xp-total').textContent, '0');
  assert.equal(document.getElementById('streak-display').textContent, '0');
  assert.equal([...document.querySelectorAll('.heat-cell')].filter(cell => !cell.title.endsWith(': 0 lượt học')).length, 0);
});

test('a late account response cannot overwrite another learner dashboard', async () => {
  let release;
  const original = api.getLearningProgress;
  api.getLearningProgress = () => new Promise(resolve => { release = resolve; });
  selectUser('first');
  const pending = initDashboard();
  const old = summary();
  selectUser('second');
  api.getLearningProgress = original;
  await initDashboard();
  release(old);
  await pending;
  assert.equal(document.getElementById('dashboard-vocabulary').textContent, '0');
});

test('dashboard failure clears previous counts and offers retry without fallback statistics', async () => {
  const original = api.getLearningProgress;
  api.getLearningProgress = async () => { throw new Error('Mất kết nối'); };
  await initDashboard();
  assert.equal(document.getElementById('dashboard-vocabulary').textContent, '—');
  assert.equal(document.getElementById('dashboard-retry').hidden, false);
  assert.match(document.getElementById('dashboard-study-summary').textContent, /Mất kết nối/);
  api.getLearningProgress = original;
});

test('real API transport publishes only the persisted summary and sends no user identity', async () => {
  const requests = [];
  globalThis.fetch = async (url, options = {}) => {
    requests.push({ url, options });
    return new Response(JSON.stringify(url.endsWith('/auth/csrf') ? { token: 'test-token' } : { recorded: true, summary: { ...summary(), totalXp: 470 } }), { status: 200, headers: { 'Content-Type': 'application/json' } });
  };
  const result = await realSubmit({ contentType: 'grammar', contentId: 12, correct: true, daily: true });
  assert.equal(state.xp, 470);
  assert.equal(result.summary.totalXp, 470);
  assert.deepEqual(JSON.parse(requests[1].options.body), { contentType: 'grammar', contentId: 12, correct: true, daily: true });
  assert.equal(requests[1].options.headers.get('X-CSRF-TOKEN'), 'test-token');
});
