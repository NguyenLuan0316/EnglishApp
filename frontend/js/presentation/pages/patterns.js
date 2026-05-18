// wordwave/frontend/js/pages/patterns.js
import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { speak, toast } from '../../shared/utils.js';

let allPatterns = [];
let patterns = [];
let patternPurposes = [];
let currentPage = 1;
let pageSize = 6;
let filtersReady = false;
let searchTimer = null;

export async function initPatterns() {
  if (!filtersReady) {
    const loaded = await loadPatterns({ updateTypes: true });
    if (!loaded) return;
    setupPatternFilters();
  }

  renderPatternsPage(currentPage);
}

async function loadPatterns(options = {}) {
  try {
    allPatterns = await api.getPatterns();
    patterns = filterPatterns(allPatterns);
    if (options.updateTypes) {
      patternPurposes = [...new Set(allPatterns.map(p => p.meaning).filter(Boolean))];
    }
    return true;
  } catch(e) {
    toast('Không thể tải mẫu câu.', 'error');
    return false;
  }
}

function setupPatternFilters() {
  const searchInput = document.getElementById('patterns-search');
  const typeSelect = document.getElementById('patterns-type-filter');
  if (!searchInput || !typeSelect) return;

  typeSelect.innerHTML = `
    <option value="">Tất cả mục đích</option>
    ${patternPurposes.map(purpose => `<option value="${escapeHtml(purpose)}">${escapeHtml(purpose)}</option>`).join('')}
  `;

  searchInput.addEventListener('input', () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(applyPatternFilters, 250);
  });
  searchInput.addEventListener('keydown', event => {
    if (event.key === 'Enter') {
      event.preventDefault();
      clearTimeout(searchTimer);
      applyPatternFilters();
    }
  });

  typeSelect.addEventListener('change', applyPatternFilters);
  filtersReady = true;
}

function applyPatternFilters() {
  currentPage = 1;
  patterns = filterPatterns(allPatterns);
  renderPatternsPage(1);
}

function filterPatterns(items) {
  const filters = getPatternFilters();
  const search = normalizeText(filters.search);

  return items.filter(pattern => {
    if (filters.purpose && pattern.meaning !== filters.purpose) {
      return false;
    }

    if (!search) {
      return true;
    }

    return [
      pattern.sentence,
      pattern.meaning,
      pattern.explanation,
      ...(pattern.examples || []),
    ].some(value => normalizeText(value).includes(search));
  });
}

function renderPatternsPage(page) {
  const totalPages = Math.max(1, Math.ceil(patterns.length / pageSize));
  currentPage = Math.min(Math.max(page, 1), totalPages);
  const start = (currentPage - 1) * pageSize;
  const pageItems = patterns.slice(start, start + pageSize);

  const container = document.getElementById('patterns-list');
  if (!container) return;

  if (pageItems.length === 0) {
    container.innerHTML = '<div class="card" style="color:var(--text3);text-align:center;padding:24px;">Không tìm thấy mẫu câu phù hợp.</div>';
    renderPatternsPagination(0);
    return;
  }

  container.innerHTML = pageItems.map(p => `
    <div class="card">
      <div style="display:flex;align-items:flex-start;gap:12px;">
        <div style="width:36px;height:36px;border-radius:50%;background:rgba(99,179,237,0.1);border:1px solid var(--border);display:flex;align-items:center;justify-content:center;flex-shrink:0;font-size:16px;">&#128172;</div>
        <div style="flex:1;">
          <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:10px;margin-bottom:2px;flex-wrap:wrap;">
            <div style="font-size:17px;font-weight:600;">${escapeHtml(p.sentence)}</div>
            <span class="badge" style="border-color:rgba(99,179,237,.35);color:var(--accent);background:rgba(99,179,237,.08);">${escapeHtml(p.meaning)}</span>
          </div>
          <div style="font-size:14px;color:var(--accent2);margin-bottom:6px;">${escapeHtml(p.meaning)}</div>
          <div style="font-size:13px;color:var(--text2);margin-bottom:10px;">${escapeHtml(p.explanation)}</div>
          <div style="display:flex;flex-direction:column;gap:4px;">
            ${(p.examples || []).map(e => `<div class="example-block" style="padding:8px 12px;"><div class="example-en" style="font-size:13px;">${escapeHtml(e)}</div></div>`).join('')}
          </div>
          <button class="btn btn-ghost btn-sm pattern-speak" style="margin-top:10px;" data-sentence="${escapeHtml(p.sentence)}">&#128266; Nghe</button>
        </div>
      </div>
    </div>`).join('');

  container.querySelectorAll('.pattern-speak').forEach(button => {
    button.addEventListener('click', () => speak(button.dataset.sentence || ''));
  });

  renderPatternsPagination(patterns.length);
}

function renderPatternsPagination(total) {
  renderPagination('patterns-pagination', {
    page: currentPage,
    total,
    limit: pageSize,
    onPageChange: renderPatternsPage,
    onPageSizeChange: nextSize => {
      pageSize = nextSize;
      renderPatternsPage(1);
    },
  });
}

function getPatternFilters() {
  return {
    search: document.getElementById('patterns-search')?.value.trim() || '',
    purpose: document.getElementById('patterns-type-filter')?.value || '',
  };
}

function normalizeText(value) {
  return String(value ?? '').trim().toLocaleLowerCase('vi-VN');
}

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
