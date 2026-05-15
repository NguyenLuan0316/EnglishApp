// wordwave/frontend/js/pages/patterns.js
import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { speak, toast } from '../../shared/utils.js';

let patterns = [];
let currentPage = 1;
const pageSize = 6;

export async function initPatterns() {
  if (patterns.length === 0) {
    try {
      patterns = await api.getPatterns();
    } catch(e) {
      toast('Không thể tải mẫu câu.', 'error');
      return;
    }
  }

  renderPatternsPage(currentPage);
}

function renderPatternsPage(page) {
  const totalPages = Math.max(1, Math.ceil(patterns.length / pageSize));
  currentPage = Math.min(Math.max(page, 1), totalPages);
  const start = (currentPage - 1) * pageSize;
  const pageItems = patterns.slice(start, start + pageSize);

  const container = document.getElementById('patterns-list');
  if (!container) return;

  container.innerHTML = pageItems.map(p => `
    <div class="card">
      <div style="display:flex;align-items:flex-start;gap:12px;">
        <div style="width:36px;height:36px;border-radius:50%;background:rgba(99,179,237,0.1);border:1px solid var(--border);display:flex;align-items:center;justify-content:center;flex-shrink:0;font-size:16px;">&#128172;</div>
        <div style="flex:1;">
          <div style="font-size:17px;font-weight:600;margin-bottom:2px;">${escapeHtml(p.sentence)}</div>
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

  renderPagination('patterns-pagination', {
    page: currentPage,
    total: patterns.length,
    limit: pageSize,
    onPageChange: renderPatternsPage,
  });
}

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
