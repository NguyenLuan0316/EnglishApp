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
const dom = new JSDOM(html, { url: 'http://localhost:5500/writing' });
globalThis.window = dom.window;
globalThis.document = dom.window.document;
globalThis.localStorage = dom.window.localStorage;
const timers = new Map();
let timerId = 0;
globalThis.setTimeout = callback => { timers.set(++timerId, callback); return timerId; };
globalThis.clearTimeout = id => timers.delete(id);
const from = relative => import(pathToFileURL(path.join(root, 'frontend/js', relative)).href);
const { api } = await from('infrastructure/api/apiClient.js');
const { state } = await from('application/state/appState.js');
const { collectPracticeSentences, tokenizeSentence, createBlankQuestion } = await from('domain/practice.js');
const { loadPracticeSentences } = await from('application/practice/practiceData.js');
const { initBuilder } = await from('presentation/pages/builder.js');
const { initFillBlank } = await from('presentation/pages/fillblank.js');
const { initQuiz } = await from('presentation/pages/quiz.js');
const { initListening } = await from('presentation/pages/listening.js');
const { initFlashcard } = await from('presentation/pages/flashcard.js');
const { initMatching } = await from('presentation/pages/matching.js');
const words = [
  { id: 812, word: 'home', meaning: 'nhà', level: 'A1', example: 'We are at home.', exampleMeaning: 'Chúng tôi đang ở nhà.' },
  { id: 930, word: 'friend', meaning: 'bạn', level: 'A2', example: 'She is my friend.', exampleMeaning: 'Cô ấy là bạn tôi.' },
  { id: 999, word: 'read', meaning: 'đọc', level: 'B1', example: 'I read every day.', exampleMeaning: 'Tôi đọc mỗi ngày.' },
  { id: 1001, word: 'homeward', meaning: 'nhà', level: 'B1' },
];
const lessons = [{ id: 14, title: 'Present perfect', examples: [{ en: "I've already finished it.", vi: 'Tôi đã hoàn thành việc đó rồi.' }] }];
const patterns = [
  { id: 15, sentence: 'Please close the door.', meaning: 'Ở nhà', explanation: 'Làm ơn đóng cửa lại.' },
  { id: 16, sentence: 'How are you doing?', meaning: 'Chào hỏi', explanation: 'Dùng khi gặp bạn bè.' },
];
const resetApi = () => {
  api.getRandom = async () => structuredClone(words);
  api.getGrammar = async () => structuredClone(lessons);
  api.getPatterns = async () => structuredClone(patterns);
  api.submitReview = async () => {};
  api.submitLearning = async () => {
    state.applyLearningSummary({ totalXp: state.xp + 10, streak: 1, dailyTotal: 15, dailyCompleted: 0 });
    return { recorded: true };
  };
};
resetApi();

test('sentence pool combines three sources, excludes usage notes and deduplicates', () => {
  const entries = collectPracticeSentences(words, lessons, patterns);
  assert.deepEqual(new Set(entries.map(x => x.source)), new Set(['vocabulary', 'grammar', 'patterns']));
  assert.equal(entries.length, 5);
  assert.ok(!entries.some(x => x.sentence === 'How are you doing?'));
  assert.equal(collectPracticeSentences(words, lessons, [...patterns, patterns[0]]).length, 5);
  assert.deepEqual(tokenizeSentence("I've got a pen, a pen."), ["I've", 'got', 'a', 'pen', ',', 'a', 'pen', '.']);
  const question = createBlankQuestion(entries[0], () => 0);
  assert.equal(question.before + question.answer + question.after, entries[0].sentence);
  assert.equal(collectPracticeSentences([{ example: 'A <script> attack.', exampleMeaning: 'Không an toàn.' }]).length, 0);
});

test('sessions include every available source and survive a failed source', async () => {
  resetApi();
  const full = await loadPracticeSentences();
  assert.equal(new Set(full.sentences.slice(0, 3).map(x => x.source)).size, 3);
  api.getGrammar = async () => { throw new Error('offline'); };
  const partial = await loadPracticeSentences();
  assert.deepEqual(partial.failedSources, ['Ngữ pháp']);
  assert.ok(partial.sentences.length > 0);
  resetApi();
});

test('builder uses live library data and awards XP once', async () => {
  await initBuilder();
  assert.ok(state.builderTarget.length >= 3);
  assert.ok(!document.getElementById('builder-prompt').textContent.includes('Đang tải'));
  state.builderSentence = [...state.builderTarget];
  const before = state.xp;
  await Promise.all([window.checkBuilder(), window.checkBuilder()]);
  assert.equal(state.xp, before + 10);
  api.getRandom = api.getGrammar = api.getPatterns = async () => [];
  await initBuilder();
  assert.equal(state.builderTarget.length, 0);
  assert.match(document.getElementById('builder-prompt').textContent, /Chưa có/);
  await window.checkBuilder();
  assert.equal(state.xp, before + 10);
  resetApi();
});

test('fill blank uses translations and prevents repeat grading and stale timers', async () => {
  await initFillBlank();
  assert.equal(state.fbQuestions.length, 5);
  assert.equal(new Set(state.fbQuestions.slice(0, 3).map(x => x.source)).size, 3);
  const input = document.getElementById('fb-input');
  input.value = state.fbQuestions[0].answer;
  const before = state.xp;
  await Promise.all([window.checkFB(), window.checkFB()]);
  assert.equal(state.xp, before + 10);
  const timer = timerId;
  window.nextFB();
  assert.ok(!timers.has(timer));
  assert.equal(state.fbIndex, 1);
  api.getRandom = api.getGrammar = api.getPatterns = async () => { throw new Error('offline'); };
  await initFillBlank();
  assert.equal(state.fbQuestions.length, 0);
  assert.match(document.getElementById('fb-question-area').textContent, /Chưa có câu/);
  resetApi();
});

test('quiz samples whole vocabulary, removes identical answer choices and reloads', async () => {
  let calls = 0;
  api.getRandom = async params => { assert.equal(params.count, 100); calls++; return words; };
  await initQuiz();
  assert.equal(state.quizWords.length, 3);
  assert.equal(new Set([...document.querySelectorAll('.quiz-option')].map(x => x.textContent.slice(1))).size, 3);
  await initQuiz();
  assert.equal(calls, 2);
  resetApi();
});

test('listening queries the selected level, ignores stale responses and handles empty results', async () => {
  const requests = [];
  api.getRandom = params => new Promise(resolve => requests.push({ params, resolve }));
  const first = initListening();
  assert.deepEqual(requests[0].params, { count: 1, level: 'A1' });
  window.setListenLevel(document.querySelector('#page-listening .tab'), 'B1');
  assert.deepEqual(requests[1].params, { count: 1, level: 'B1' });
  requests[1].resolve([words[2]]);
  await new Promise(resolve => queueMicrotask(resolve));
  requests[0].resolve([words[0]]);
  await first;
  assert.equal(state.listenWord.id, words[2].id);
  api.getRandom = async () => [];
  await initListening();
  assert.equal(state.listenWord, null);
  assert.match(document.getElementById('listen-feedback').textContent, /Chưa có từ/);
  resetApi();
});

test('flashcards and matching keep using vocabulary API records', async () => {
  resetApi();
  await initFlashcard();
  assert.equal(document.getElementById('fc-word').textContent, words[0].word);
  await initMatching();
  assert.ok(document.getElementById('ml-812'));
});


test('the complete new daily catalog can generate sentence practice', async () => {
  const catalog = JSON.parse(await readFile(path.join(root, 'backend/WordWave.Api/Data/daily-patterns.json'), 'utf8'));
  const entries = collectPracticeSentences([], [], catalog);
  assert.equal(entries.length, 1000);
  assert.ok(entries.every(entry => {
    const question = createBlankQuestion(entry);
    return question && question.before + question.answer + question.after === entry.sentence;
  }));
});
