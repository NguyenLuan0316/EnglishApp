// wordwave/frontend/js/pages/vocabulary.js
import { api }          from '../api.js';
import { speak, toast, badgeClass } from '../utils.js';

let allWords = [], filtered = [], displayCount = 20;

const TOPIC_LABELS = { daily:'Đời sống', food:'Ẩm thực', travel:'Du lịch', work:'Công việc',
  technology:'Công nghệ', health:'Sức khỏe', education:'Giáo dục', science:'Khoa học',
  society:'Xã hội', shopping:'Mua sắm' };

export async function initVocabulary() {
  // Load tất cả (có thể dùng filter từ API)
  if (allWords.length === 0) {
    try {
      const res = await api.getVocab({ limit: 200 });
      allWords = res.data || [];
    } catch(e) {
      toast('Không thể kết nối API. Đang dùng dữ liệu offline.', 'error');
    }
  }

  // Populate topic select
  const topics = [...new Set(allWords.map(w => w.topic))];
  const sel    = document.getElementById('topic-filter');
  if (sel && sel.options.length <= 1) {
    topics.forEach(t => {
      const o = document.createElement('option');
      o.value = t; o.textContent = TOPIC_LABELS[t] || t;
      sel.appendChild(o);
    });
  }

  // Topic pills
  const pillsEl = document.getElementById('topic-filter-pills');
  if (pillsEl) pillsEl.innerHTML = topics.map(t =>
    `<div class="topic-pill" id="pill-${t}" onclick="setTopicFilter('${t}')">${TOPIC_LABELS[t]||t}</div>`
  ).join('');

  filterVocab();
}

export function filterVocab() {
  const search = document.getElementById('vocab-search')?.value.toLowerCase() || '';
  const level  = document.getElementById('level-filter')?.value || '';
  const topic  = document.getElementById('topic-filter')?.value || '';

  filtered = allWords.filter(w => {
    if (level && w.level !== level) return false;
    if (topic && w.topic !== topic) return false;
    if (search && !w.word.toLowerCase().includes(search) && !w.meaning.toLowerCase().includes(search)) return false;
    return true;
  });
  displayCount = 20;
  renderList();
}

export function setTopicFilter(t) {
  const sel = document.getElementById('topic-filter');
  if (sel) sel.value = t;
  document.querySelectorAll('.topic-pill').forEach(p => p.classList.remove('active'));
  document.getElementById('pill-' + t)?.classList.add('active');
  filterVocab();
}

export function loadMoreVocab() {
  displayCount += 20;
  renderList();
}

function renderList() {
  const container = document.getElementById('vocab-list');
  if (!container) return;
  const slice = filtered.slice(0, displayCount);
  container.innerHTML = slice.map(v => `
    <div class="word-card">
      <div class="word-info">
        <div style="display:flex;align-items:center;gap:8px;">
          <span class="word-english">${v.word}</span>
          <span class="badge ${badgeClass(v.level)}">${v.level}</span>
        </div>
        <div class="word-phonetic">${v.phonetic}</div>
        <div class="word-meaning">${v.meaning}</div>
        <div class="word-example">${v.example}</div>
      </div>
      <button class="speak-btn" onclick="window._speak('${v.word.replace(/'/g,"\\'")}')">🔊</button>
    </div>
  `).join('');
  const loadBtn = document.getElementById('vocab-load-more');
  if (loadBtn) loadBtn.style.display = filtered.length > displayCount ? 'block' : 'none';
}

// Expose để HTML có thể gọi
window._speak = speak;
