import { TOPIC_LABELS } from '../../domain/vocabulary.js';

const topicOrder = Object.keys(TOPIC_LABELS);
const coverColors = ['#efc477', '#e9a58e', '#9dcad6', '#bccb9d', '#a6bbdc', '#d8c29a'];
const iconList = '<svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><path d="M6 5h11M6 10h11M6 15h11M2 5h1M2 10h1M2 15h1"/></svg>';
const iconBooks = '<svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true"><path d="M2 3h5v14H2zM9 3h5v14H9zM14 4l2-1 3 13-2 1z"/></svg>';

export function escapeHtml(value) {
  return String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[c]));
}

export function searchKey(value) {
  return String(value || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase().trim();
}

export function readView(key) {
  try { return localStorage.getItem(key) === 'list' ? 'list' : 'books'; } catch { return 'books'; }
}

export function rememberView(key, value) {
  try { localStorage.setItem(key, value); } catch { /* The view still works when browser storage is unavailable. */ }
}

export function bindViewSwitcher(containerId, selected, booksLabel, onChange) {
  const container = document.getElementById(containerId);
  if (!container) return;
  container.innerHTML = `<button type="button" data-view="books" aria-pressed="${selected === 'books'}">${iconBooks}<span>${escapeHtml(booksLabel)}</span></button><button type="button" data-view="list" aria-pressed="${selected === 'list'}">${iconList}<span>Danh sách</span></button>`;
  container.querySelectorAll('[data-view]').forEach(button => { button.onclick = () => onChange(button.dataset.view); });
}

export function renderStudyBook({ key, title, count, levels = [], kind = 'vocabulary', index = 0 }) {
  const topicIndex = topicOrder.indexOf(key);
  const imageIndex = kind === 'grammar' ? 6 : topicIndex >= 0 ? topicIndex : 6;
  const x = (imageIndex % 8) / 7 * 100;
  const y = Math.floor(imageIndex / 8) / 3 * 100;
  const color = coverColors[(kind === 'grammar' ? index + 2 : imageIndex) % coverColors.length];
  const total = Math.max(0, Number(count) || 0);
  const availableLevels = levels.filter(level => level && level !== 'N/A');
  const range = kind === 'grammar' ? key : availableLevels.length > 1 ? `${availableLevels[0]}–${availableLevels.at(-1)}` : availableLevels[0] || (levels.includes('N/A') ? 'Chưa phân loại' : 'Từ vựng');
  const unit = kind === 'grammar' ? 'bài học' : 'từ vựng';
  return `<button type="button" class="study-book${total ? '' : ' study-book--empty'}" data-book-key="${escapeHtml(key)}" ${total ? '' : 'disabled'} aria-label="${escapeHtml(title)}, ${total} ${unit}${total ? ', mở sách' : ', chưa có nội dung'}">
    <span class="book-cover" style="--book-color:${color}">
      <span class="book-cover-top"><span class="book-publisher">WordWave</span><span class="book-level">${escapeHtml(range)}</span></span>
      <span class="book-illustration" style="background-position:${x}% ${y}%" aria-hidden="true"></span>
      <span class="book-title-plate"><span class="book-kind">${kind === 'grammar' ? 'Ngữ pháp' : 'Từ vựng'}</span><span class="book-title">${escapeHtml(title)}</span></span>
      <span class="book-cover-bottom"><span>${total ? total.toLocaleString('vi-VN') + ' ' + unit : 'Chưa có bài học'}</span><svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 10h12m-5-5 5 5-5 5"/></svg></span>
    </span>
  </button>`;
}

export function renderCollectionMessage(containerId, message, retry) {
  const container = document.getElementById(containerId);
  if (!container) return;
  container.replaceChildren();
  const status = document.createElement('div');
  status.className = 'collection-message';
  status.setAttribute('role', 'status');
  const text = document.createElement('p');
  text.textContent = message;
  status.append(text);
  if (retry) {
    const button = document.createElement('button');
    button.className = 'btn btn-secondary';
    button.type = 'button';
    button.textContent = 'Thử lại';
    button.onclick = retry;
    status.append(button);
  }
  container.append(status);
}
