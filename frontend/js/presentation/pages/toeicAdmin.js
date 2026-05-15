import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { toast } from '../../shared/utils.js';

let initialized = false;
let importLogPage = 1;
const importLogPageSize = 10;

export function initToeicAdmin() {
  if (!initialized) {
    bindToeicAdminEvents();
    initialized = true;
  }

  loadToeicImportLogs(importLogPage);
}

function bindToeicAdminEvents() {
  document.getElementById('toeic-json-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const file = document.getElementById('toeic-json-file')?.files?.[0];
    if (!file) {
      toast('Chon file JSON truoc khi import.', 'error');
      return;
    }

    await runToeicAction(() => api.importToeicJson(file), 'Import JSON TOEIC hoan tat.');
  });

  document.getElementById('toeic-csv-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const file = document.getElementById('toeic-csv-file')?.files?.[0];
    if (!file) {
      toast('Chon file CSV truoc khi import.', 'error');
      return;
    }

    await runToeicAction(() => api.importToeicCsv(file), 'Import CSV TOEIC hoan tat.');
  });

  document.getElementById('toeic-crawl-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const keyword = document.getElementById('toeic-crawl-keyword')?.value.trim() || '';
    const sourceUrl = document.getElementById('toeic-crawl-url')?.value.trim() || '';

    if (!keyword || !sourceUrl) {
      toast('Nhap keyword va source URL truoc khi crawl.', 'error');
      return;
    }

    await runToeicAction(() => api.crawlToeic({ keyword, sourceUrl }), 'Crawl TOEIC hoan tat.');
  });

  document.getElementById('toeic-refresh-logs')?.addEventListener('click', () => loadToeicImportLogs(importLogPage));
}

async function runToeicAction(action, successMessage) {
  setToeicBusy(true);
  setToeicResult('');

  try {
    const result = await action();
    setToeicResult(formatResult(result));
    toast(successMessage, result.status === 'failed' ? 'error' : 'success');
    await loadToeicImportLogs(1);
  } catch (error) {
    setToeicResult(`<div style="color:var(--red);">Loi: ${escapeHtml(error.message || 'Khong the thuc hien thao tac TOEIC.')}</div>`);
    toast('Khong the thuc hien thao tac TOEIC.', 'error');
  } finally {
    setToeicBusy(false);
  }
}

async function loadToeicImportLogs(page = 1) {
  const container = document.getElementById('toeic-import-logs');
  if (!container) return;

  container.innerHTML = '<div style="color:var(--text3);font-size:13px;">Dang tai log...</div>';
  try {
    const result = await api.getToeicImportLogs({ page, limit: importLogPageSize });
    const logs = Array.isArray(result) ? result : (result.data || []);
    importLogPage = Array.isArray(result) ? page : (result.page || page);
    container.innerHTML = logs.length
      ? logs.map(renderLog).join('')
      : '<div style="color:var(--text3);font-size:13px;">Chua co log import TOEIC.</div>';

    renderPagination('toeic-import-pagination', {
      page: importLogPage,
      total: Array.isArray(result) ? logs.length : (result.total || 0),
      limit: importLogPageSize,
      onPageChange: loadToeicImportLogs,
    });
  } catch {
    container.innerHTML = '<div style="color:var(--red);font-size:13px;">Khong tai duoc import logs. Hay kiem tra API/backend.</div>';
    document.getElementById('toeic-import-pagination')?.replaceChildren();
  }
}

function renderLog(log) {
  const statusColor = log.status === 'success'
    ? 'var(--green)'
    : log.status === 'partial'
      ? 'var(--accent3)'
      : 'var(--red)';

  return `
    <div class="word-card" style="cursor:default;">
      <div class="word-info">
        <div style="display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap;">
          <div>
            <div class="word-english" style="font-size:15px;">${escapeHtml(log.sourceName || log.sourceType)}</div>
            <div class="word-phonetic">${escapeHtml(log.sourceType)} ${log.sourceUrl ? '- ' + escapeHtml(log.sourceUrl) : ''}</div>
          </div>
          <span class="badge" style="background:rgba(99,179,237,.1);color:${statusColor};border:1px solid var(--border);">${escapeHtml(log.status)}</span>
        </div>
        <div style="font-size:12px;color:var(--text2);margin-top:8px;">
          Total: ${log.totalItems} | Imported: ${log.importedItems} | Failed: ${log.failedItems}
        </div>
        ${log.errorMessage ? `<div style="font-size:12px;color:var(--red);margin-top:6px;">${escapeHtml(log.errorMessage)}</div>` : ''}
        <div style="font-size:11px;color:var(--text3);margin-top:6px;">${escapeHtml(new Date(log.createdAt).toLocaleString())}</div>
      </div>
    </div>
  `;
}

function formatResult(result) {
  const errors = result.errors?.length
    ? `<div style="margin-top:10px;color:var(--red);font-size:12px;">${result.errors.map(escapeHtml).join('<br>')}</div>`
    : '';

  return `
    <div style="font-size:13px;color:var(--text2);line-height:1.8;">
      <div><b style="color:var(--text);">Status:</b> ${escapeHtml(result.status || '')}</div>
      <div><b style="color:var(--text);">Test ID:</b> ${result.testId ?? '-'}</div>
      <div><b style="color:var(--text);">Total:</b> ${result.totalItems ?? 0}</div>
      <div><b style="color:var(--text);">Imported:</b> ${result.importedItems ?? 0}</div>
      <div><b style="color:var(--text);">Failed:</b> ${result.failedItems ?? 0}</div>
      ${errors}
    </div>
  `;
}

function setToeicBusy(isBusy) {
  document.querySelectorAll('[data-toeic-action]').forEach(button => {
    button.disabled = isBusy;
    button.style.opacity = isBusy ? '.65' : '1';
  });
}

function setToeicResult(html) {
  const result = document.getElementById('toeic-action-result');
  if (result) result.innerHTML = html;
}

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
