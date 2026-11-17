// wordwave/frontend/js/pages/vocabulary.js
import { topicLabel, TOPIC_LABELS } from '../../domain/vocabulary.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { speak, toast, badgeClass, levelLabel } from '../../shared/utils.js';
import { bindViewSwitcher, readView, rememberView, renderStudyBook, renderCollectionMessage, searchKey } from '../components/studyBooks.js';

let allWords = [], filtered = [], topics = [], currentPage = 1, totalWords = 0;
let pageSize = 20;
let view = readView('ww_vocab_view');
let summaries = [], summaryLevel = null, summaryRequest = 0, listRequest = 0;
let bookSearch = '', listSearch = '', openedTopic = '';

export async function initVocabulary() {
  if (topics.length === 0) {
    await loadTopics();
  }

  renderTopicFilters();
  document.getElementById('vocab-back-to-books').onclick = async () => {
    const previousTopic = openedTopic;
    await setView('books', false);
    [...document.querySelectorAll('#vocab-books [data-book-key]')].find(button => button.dataset.bookKey === previousTopic)?.focus();
  };
  await setView(view, false);
}

function syncView() {
  const books = view === 'books';
  bindViewSwitcher('vocab-view-switch', view, 'Theo chủ đề', next => {
    setView(next);
    document.querySelector(`#vocab-view-switch [data-view="${next}"]`)?.focus();
  });
  document.getElementById('vocab-books').hidden = !books;
  document.getElementById('vocab-books-summary').hidden = !books;
  document.getElementById('topic-filter').hidden = books;
  document.getElementById('topic-filter-pills').hidden = books;
  document.getElementById('vocab-list').hidden = books;
  document.getElementById('vocab-pagination').hidden = books;
  document.getElementById('vocab-book-drill').hidden = books || !openedTopic;
  const search = document.getElementById('vocab-search');
  search.placeholder = books ? 'Tìm chủ đề...' : 'Tìm từ hoặc nghĩa tiếng Việt...';
  search.setAttribute('aria-label', books ? 'Tìm chủ đề từ vựng' : 'Tìm từ hoặc nghĩa tiếng Việt');
}

async function setView(next, persist = true) {
  view = next === 'list' ? 'list' : 'books';
  if (persist) {
    rememberView('ww_vocab_view', view);
    openedTopic = '';
  }
  document.getElementById('vocab-search').value = view === 'books' ? bookSearch : listSearch;
  syncView();
  if (view === 'books') await loadBooks();
  else await loadVocabularyPage(1);
}

async function loadBooks(force = false) {
  const level = document.getElementById('level-filter').value;
  if (summaryLevel === level && !force) { renderBooks(); return; }
  const request = ++summaryRequest;
  const container = document.getElementById('vocab-books');
  container.setAttribute('aria-busy', 'true');
  document.getElementById('vocab-books-summary').textContent = 'Đang tải các chủ đề…';
  renderCollectionMessage('vocab-books', 'Đang xếp sách lên kệ…');
  try {
    const data = await api.getTopicSummaries(level ? { level } : {});
    if (request !== summaryRequest) return;
    summaries = data;
    summaryLevel = level;
    if (view === 'books') renderBooks();
  } catch (error) {
    if (request !== summaryRequest) return;
    document.getElementById('vocab-books-summary').textContent = 'Chưa tải được thư viện.';
    renderCollectionMessage('vocab-books', error.message || 'Không thể tải chủ đề. Kiểm tra kết nối và thử lại.', () => loadBooks(true));
  } finally {
    if (request === summaryRequest) container.removeAttribute('aria-busy');
  }
}

function renderBooks() {
  const query = searchKey(bookSearch);
  const order = Object.keys(TOPIC_LABELS);
  const rank = topic => order.indexOf(topic) < 0 ? order.length : order.indexOf(topic);
  const data = summaries.filter(book => !query || searchKey(topicLabel(book.topic) + ' ' + book.topic).includes(query)).sort((a, b) => rank(a.topic) - rank(b.topic) || topicLabel(a.topic).localeCompare(topicLabel(b.topic), 'vi'));
  const total = data.reduce((count, book) => count + book.count, 0);
  document.getElementById('vocab-books-summary').textContent = `${data.length} chủ đề · ${total.toLocaleString('vi-VN')} từ vựng`;
  if (!data.length) {
    renderCollectionMessage('vocab-books', summaries.length ? 'Không tìm thấy chủ đề phù hợp. Thử tên khác hoặc xóa từ khóa.' : 'Chưa có từ vựng ở trình độ này. Hãy chọn trình độ khác.');
    return;
  }
  const container = document.getElementById('vocab-books');
  container.innerHTML = data.map(book => renderStudyBook({ key: book.topic, title: topicLabel(book.topic), count: book.count, levels: book.levels })).join('');
  container.querySelectorAll('[data-book-key]').forEach(button => {
    button.onclick = async () => {
      openedTopic = button.dataset.bookKey;
      document.getElementById('topic-filter').value = openedTopic;
      document.getElementById('vocab-open-book-title').textContent = topicLabel(openedTopic);
      listSearch = '';
      await setView('list', false);
      document.getElementById('vocab-open-book-title').focus();
    };
  });
}

async function loadTopics() {
  try {
    topics = (await api.getTopics()).filter(Boolean);
  } catch(e) {
    toast('Không thể tải danh sách chủ đề từ API.', 'error');
    topics = [];
  }
}

function getFilters() {
  const search = document.getElementById('vocab-search')?.value.trim() || '';
  const level  = document.getElementById('level-filter')?.value || '';
  const topic  = document.getElementById('topic-filter')?.value || '';

  return {
    ...(search && { search }),
    ...(level && { level }),
    ...(topic && { topic }),
  };
}

async function loadVocabularyPage(page, append = false) {
  const request = ++listRequest;
  const container = document.getElementById('vocab-list');
  container.setAttribute('aria-busy', 'true');
  document.getElementById('vocab-pagination').replaceChildren();
  renderCollectionMessage('vocab-list', 'Đang tải từ vựng…');
  try {
    const res = await api.getVocab({ ...getFilters(), page, limit: pageSize });
    if (request !== listRequest) return;
    currentPage = res.page || page;
    totalWords = res.total || 0;
    allWords = append ? allWords.concat(res.data || []) : (res.data || []);
    filtered = allWords;
  } catch(e) {
    if (request !== listRequest) return;
    renderCollectionMessage('vocab-list', e.message || 'Không thể tải từ vựng.', () => loadVocabularyPage(page, append));
    return;
  } finally {
    if (request === listRequest) container.removeAttribute('aria-busy');
  }

  renderTopicFilters();
  renderList();
}

function renderTopicFilters() {
  const selectedTopic = document.getElementById('topic-filter')?.value || '';
  const sel = document.getElementById('topic-filter');

  if (sel) {
    const currentValue = sel.value;
    sel.innerHTML = '<option value="">Tất cả Chủ đề</option>';
    topics.forEach(t => {
      const o = document.createElement('option');
      o.value = t;
      o.textContent = topicLabel(t);
      sel.appendChild(o);
    });
    sel.value = topics.includes(currentValue) ? currentValue : '';
  }

  const pillsEl = document.getElementById('topic-filter-pills');
  if (!pillsEl) return;

  pillsEl.innerHTML = topics.map((t, index) => `
    <button type="button" class="topic-pill${t === selectedTopic ? ' active' : ''}" data-topic-index="${index}">
      ${escapeHtml(topicLabel(t))}
    </button>
  `).join('');

  pillsEl.querySelectorAll('.topic-pill').forEach(pill => {
    pill.addEventListener('click', () => {
      const topic = topics[Number(pill.dataset.topicIndex)];
      setTopicFilter(topic);
    });
  });
}

export function filterVocab() {
  if (view === 'books') {
    bookSearch = document.getElementById('vocab-search').value;
    loadBooks();
  } else {
    listSearch = document.getElementById('vocab-search').value;
    if (openedTopic && document.getElementById('topic-filter').value !== openedTopic) {
      openedTopic = '';
      syncView();
    }
    loadVocabularyPage(1);
  }
}

export function setTopicFilter(t) {
  const sel = document.getElementById('topic-filter');
  if (sel) sel.value = t;
  filterVocab();
}

export function loadMoreVocab() {
  loadVocabularyPage(currentPage + 1);
}

function renderList() {
  const container = document.getElementById('vocab-list');
  if (!container) return;

  container.innerHTML = filtered.length ? filtered.map(v => `
    <div class="word-card">
      <div class="word-info">
        <div style="display:flex;align-items:center;gap:8px;">
          <span class="word-english">${escapeHtml(v.word)}</span>
          <span class="badge ${badgeClass(v.level)}">${escapeHtml(levelLabel(v.level))}</span>
        </div>
        <div class="word-phonetic">${escapeHtml(v.phonetic || '')}</div>
        <div class="word-meaning">${escapeHtml(v.meaning || '')}</div>
        <div class="word-example">${escapeHtml(v.example || '')}</div>
      </div>
      <button class="speak-btn" data-word="${escapeHtml(v.word)}">&#128266;</button>
    </div>
  `).join('') : '<div style="color:var(--text3);text-align:center;padding:20px;">Không có từ vựng phù hợp</div>';

  container.querySelectorAll('.speak-btn[data-word]').forEach(button => {
    button.addEventListener('click', event => {
      event.stopPropagation();
      speak(button.dataset.word || '');
    });
  });

  const loadBtn = document.getElementById('vocab-load-more');
  if (loadBtn) loadBtn.style.display = 'none';

  renderPagination('vocab-pagination', {
    page: currentPage,
    total: totalWords,
    limit: pageSize,
    onPageChange: page => loadVocabularyPage(page),
    onPageSizeChange: nextSize => {
      pageSize = nextSize;
      loadVocabularyPage(1);
    },
  });
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

// Expose để HTML có thể gọi
window._speak = speak;
