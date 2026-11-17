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
const dom = new JSDOM(html, { url: 'http://localhost:5500/vocab' });
globalThis.window = dom.window;
globalThis.document = dom.window.document;
globalThis.localStorage = dom.window.localStorage;
dom.window.HTMLElement.prototype.scrollIntoView = () => {};
const from = relative => import(pathToFileURL(path.join(root, 'frontend', relative)).href);
const { api } = await from('js/infrastructure/api/apiClient.js');
const entries = JSON.parse(await readFile(path.join(root, 'frontend/data/vocabulary/words-3000.json'), 'utf8')).entries;
const summary = level => Object.entries(Object.groupBy(entries.filter(word => !level || word.level === level), word => word.topic)).map(([topic, words]) => ({ topic, count: words.length, levels: [...new Set(words.map(word => word.level))].sort() }));
api.getTopics = async () => [...new Set(entries.map(word => word.topic))];
api.getTopicSummaries = async params => summary(params?.level);
api.getVocab = async ({ topic, level, search = '', page = 1, limit = 20 }) => {
  const data = entries.filter(word => (!topic || word.topic === topic) && (!level || word.level === level) && (!search || (word.word + ' ' + word.meaning).toLowerCase().includes(search.toLowerCase())));
  return { total: data.length, page, limit, data: data.slice((page - 1) * limit, page * limit) };
};
api.getGrammar = async () => [
  { id: 1, title: 'Present Simple', level: 'A1', description: 'Habits', formula: 'S + V', tips: 'Tip', examples: [{ en: 'I learn.', vi: 'Tôi học.' }] },
  { id: 2, title: 'Articles A An The', level: 'A1', description: 'Articles', formula: 'a/an/the + noun', examples: [] },
  { id: 3, title: 'Past Simple', level: 'A2', description: 'Past events', formula: 'S + V2', examples: [] },
  { id: 4, title: 'First Conditional', level: 'B1', description: 'Conditions', formula: 'If...', examples: [] },
  { id: 5, title: 'Mixed Conditionals', level: 'B2', description: 'Mixed conditions', formula: 'If...', examples: [] },
];
const vocabulary = await from('js/presentation/pages/vocabulary.js');
const grammar = await from('js/presentation/pages/grammar.js');
const books = await from('js/presentation/components/studyBooks.js');
const el = id => document.getElementById(id);
const clickView = (prefix, view) => document.querySelector(`#${prefix}-view-switch [data-view="${view}"]`).click();
async function waitFor(predicate) {
  for (let i = 0; i < 100; i++) { if (predicate()) return; await new Promise(resolve => setImmediate(resolve)); }
  assert.ok(predicate(), 'UI did not reach its expected state');
}

test('Illustrated vocabulary and grammar views', async t => {
  await t.test('All real topics have unique illustration cells and live counts', async () => {
    await vocabulary.initVocabulary();
    const cards = [...document.querySelectorAll('#vocab-books .study-book')];
    assert.equal(cards.length, 32);
    assert.equal(new Set(cards.map(card => card.querySelector('.book-illustration').style.backgroundPosition)).size, 32);
    assert.match(el('vocab-books-summary').textContent, /3\.000/);
    assert.equal(el('vocab-list').hidden, true);
    assert.equal(document.querySelector('#vocab-view-switch [data-view="books"]').getAttribute('aria-pressed'), 'true');
  });
  await t.test('Unaccented topic search, book drill-down and back navigation work', async () => {
    el('vocab-search').value = 'gia dinh'; vocabulary.filterVocab();
    assert.equal(document.querySelectorAll('#vocab-books .study-book').length, 1);
    document.querySelector('#vocab-books [data-book-key="family"]').click();
    await waitFor(() => document.querySelectorAll('#vocab-list .word-card').length > 0);
    assert.equal(el('topic-filter').value, 'family');
    assert.equal(el('vocab-book-drill').hidden, false);
    assert.equal(el('vocab-open-book-title').textContent, 'Gia đình');
    assert.equal(localStorage.getItem('ww_vocab_view'), null, 'Drilling into a book must not replace the preferred view');
    el('vocab-back-to-books').click();
    await waitFor(() => !el('vocab-books').hidden);
    assert.equal(document.querySelectorAll('#vocab-books .study-book').length, 1);
  });
  await t.test('Original word list, word search and paging remain available', async () => {
    clickView('vocab', 'list');
    await waitFor(() => el('vocab-list').getAttribute('aria-busy') === null);
    assert.equal(localStorage.getItem('ww_vocab_view'), 'list');
    el('vocab-search').value = ''; vocabulary.filterVocab();
    vocabulary.setTopicFilter('daily');
    await waitFor(() => el('vocab-list').getAttribute('aria-busy') === null);
    assert.equal(document.querySelectorAll('#vocab-list .word-card').length, 20);
    document.querySelector('#vocab-pagination [data-page="2"]').click();
    await waitFor(() => el('vocab-list').getAttribute('aria-busy') === null);
    assert.match(el('vocab-pagination').textContent, /21-40/);
    el('topic-filter').value = '';
    el('vocab-search').value = 'computer'; vocabulary.filterVocab();
    await waitFor(() => el('vocab-list').getAttribute('aria-busy') === null);
    assert.match(el('vocab-list').textContent, /computer/);
  });
  await t.test('Level-specific counts and empty levels are accurate', async () => {
    clickView('vocab', 'books');
    el('vocab-search').value = ''; vocabulary.filterVocab();
    el('level-filter').value = 'A1'; vocabulary.filterVocab();
    await waitFor(() => el('vocab-books').getAttribute('aria-busy') === null);
    assert.match(el('vocab-books-summary').textContent, /722/);
    el('level-filter').value = 'C1'; vocabulary.filterVocab();
    await waitFor(() => el('vocab-books').getAttribute('aria-busy') === null);
    assert.equal(document.querySelectorAll('#vocab-books .study-book').length, 0);
    assert.match(el('vocab-books').textContent, /Chưa có từ vựng/);
  });
  await t.test('Failed summary requests offer a working retry', async () => {
    api.getTopicSummaries = async () => { throw new Error('Máy chủ không khả dụng'); };
    el('level-filter').value = 'A2'; vocabulary.filterVocab();
    await waitFor(() => el('vocab-books').textContent.includes('Thử lại'));
    assert.match(el('vocab-books').textContent, /Máy chủ không khả dụng/);
    api.getTopicSummaries = async params => summary(params?.level);
    el('vocab-books').querySelector('button').click();
    await waitFor(() => document.querySelectorAll('#vocab-books .study-book').length > 0);
    assert.match(el('vocab-books-summary').textContent, /718/);
  });
  await t.test('Late responses cannot overwrite a newer selected level', async () => {
    const pending = {};
    api.getTopicSummaries = ({ level }) => new Promise(resolve => { pending[level] = resolve; });
    el('level-filter').value = 'A1'; vocabulary.filterVocab();
    el('level-filter').value = 'B2'; vocabulary.filterVocab();
    pending.B2(summary('B2'));
    await waitFor(() => el('vocab-books').getAttribute('aria-busy') === null);
    assert.match(el('vocab-books-summary').textContent, /628/);
    pending.A1(summary('A1'));
    await new Promise(resolve => setImmediate(resolve));
    assert.match(el('vocab-books-summary').textContent, /628/);
    api.getTopicSummaries = async params => summary(params?.level);
  });
  await t.test('Grammar books use actual counts and disable the empty C1 book', async () => {
    await grammar.initGrammar();
    assert.equal(document.querySelectorAll('#grammar-books .study-book').length, 5);
    assert.equal(document.querySelector('#grammar-books [data-book-key="C1"]').disabled, true);
    assert.match(el('grammar-books-summary').textContent, /5 bài học/);
    assert.match(document.querySelector('#grammar-books [data-book-key="C1"]').textContent, /Chưa có bài học/);
    document.querySelector('#grammar-books [data-book-key="A1"]').click();
    assert.equal(document.querySelectorAll('#grammar-list .grammar-card').length, 2);
    assert.equal(document.querySelector('#grammar-level-tabs [data-level="A1"]').getAttribute('aria-pressed'), 'true');
    window.toggleGrammar(1);
    assert.equal(el('gContent-1').classList.contains('show'), true);
    el('grammar-back-to-books').click();
    assert.equal(el('grammar-books').hidden, false);
  });
  await t.test('Grammar title/level search and the remembered list view work', async () => {
    el('grammar-search').value = 'B1'; grammar.filterGrammarByName('B1');
    assert.equal(document.querySelectorAll('#grammar-books .study-book').length, 1);
    document.querySelector('#grammar-books [data-book-key="B1"]').click();
    assert.equal(el('grammar-search').value, '');
    assert.equal(document.querySelectorAll('#grammar-list .grammar-card').length, 1);
    clickView('grammar', 'list');
    assert.equal(localStorage.getItem('ww_grammar_view'), 'list');
    grammar.filterGrammarByLevel('all');
    el('grammar-search').value = 'hien tai'; grammar.filterGrammarByName('hien tai');
    assert.equal(document.querySelectorAll('#grammar-list .grammar-card').length, 1);
    assert.match(el('grammar-list').textContent, /Present Simple/);
  });
  await t.test('Unknown topics still render a cover and API text is escaped', () => {
    const host = document.createElement('div');
    host.innerHTML = books.renderStudyBook({ key: '"><img src=x onerror=alert(1)>', title: '<script>bad</script>', count: 2, levels: ['A1'] });
    assert.equal(host.querySelector('img'), null);
    assert.equal(host.querySelector('script'), null);
    assert.match(host.textContent, /<script>bad<\/script>/);
    assert.ok(host.querySelector('.book-illustration'));
  });
  await t.test('Unclassified vocabulary has a readable label and does not create a false CEFR range', async () => {
    const { levelLabel } = await import('../../frontend/js/shared/utils.js');
    assert.equal(levelLabel('N/A'), 'Chưa phân loại');
    assert.ok(document.querySelector('#level-filter option[value="N/A"]'));
    assert.ok(document.querySelector('#level-filter option[value="C2"]'));
    const host = document.createElement('div');
    host.innerHTML = books.renderStudyBook({ key: 'family', title: 'Gia đình', count: 5, levels: ['A1', 'C2', 'N/A'] });
    assert.equal(host.querySelector('.book-level').textContent, 'A1–C2');
    host.innerHTML = books.renderStudyBook({ key: 'family', title: 'Gia đình', count: 5, levels: ['N/A'] });
    assert.equal(host.querySelector('.book-level').textContent, 'Chưa phân loại');
  });
  dom.window.close();
});
