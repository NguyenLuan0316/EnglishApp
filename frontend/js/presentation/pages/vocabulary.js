// wordwave/frontend/js/pages/vocabulary.js
import { topicLabel } from '../../domain/vocabulary.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { speak, toast, badgeClass } from '../../shared/utils.js';

let allWords = [], filtered = [], topics = [], currentPage = 1, totalWords = 0;
const pageSize = 20;

export async function initVocabulary() {
  if (topics.length === 0) {
    await loadTopics();
  }

  renderTopicFilters();
  await loadVocabularyPage(1);
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
  try {
    const res = await api.getVocab({ ...getFilters(), page, limit: pageSize });
    currentPage = res.page || page;
    totalWords = res.total || 0;
    allWords = append ? allWords.concat(res.data || []) : (res.data || []);
    filtered = allWords;
  } catch(e) {
    toast('Không thể tải từ vựng từ API.', 'error');
    if (!append) {
      allWords = [];
      filtered = [];
      totalWords = 0;
    }
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
  loadVocabularyPage(1);
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
          <span class="badge ${badgeClass(v.level)}">${escapeHtml(v.level)}</span>
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
