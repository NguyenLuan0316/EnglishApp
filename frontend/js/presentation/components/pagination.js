export function renderPagination(containerId, options) {
  const container = document.getElementById(containerId);
  if (!container) return;

  const total = Number(options.total || 0);
  const limit = Math.max(1, Number(options.limit || 20));
  const totalPages = Math.max(1, Math.ceil(total / limit));
  const page = clamp(Number(options.page || 1), 1, totalPages);
  const pageSizeOptions = normalizePageSizeOptions(options.pageSizeOptions, limit);
  const canChangePageSize = typeof options.onPageSizeChange === 'function';

  const pages = pageWindow(page, totalPages);
  const from = total === 0 ? 0 : ((page - 1) * limit) + 1;
  const to = Math.min(page * limit, total);
  const controls = totalPages > 1
    ? `<div class="pagination-controls">
        <button type="button" class="pagination-btn" data-page="${page - 1}" ${page === 1 ? 'disabled' : ''}>Trước</button>
        ${pages.map(item => item === '...'
          ? '<span class="pagination-ellipsis">...</span>'
          : `<button type="button" class="pagination-btn${item === page ? ' active' : ''}" data-page="${item}">${item}</button>`
        ).join('')}
        <button type="button" class="pagination-btn" data-page="${page + 1}" ${page === totalPages ? 'disabled' : ''}>Sau</button>
      </div>`
    : '';

  container.innerHTML = `
    <div class="pagination">
      <div class="pagination-meta">
        <div class="pagination-summary">Hiển thị ${from}-${to} / ${total}</div>
        ${canChangePageSize ? `
          <label class="pagination-size">
            <span>Số lượng/trang</span>
            <select data-page-size>
              ${pageSizeOptions.map(size => `<option value="${size}" ${size === limit ? 'selected' : ''}>${size}</option>`).join('')}
            </select>
          </label>
        ` : ''}
      </div>
      ${controls}
    </div>
  `;

  container.querySelectorAll('.pagination-btn[data-page]').forEach(button => {
    button.addEventListener('click', () => {
      const nextPage = Number(button.dataset.page);
      if (!button.disabled && nextPage >= 1 && nextPage <= totalPages && nextPage !== page) {
        options.onPageChange?.(nextPage);
      }
    });
  });

  container.querySelector('[data-page-size]')?.addEventListener('change', event => {
    options.onPageSizeChange?.(Number(event.target.value));
  });
}

function normalizePageSizeOptions(values, currentLimit) {
  const defaults = [6, 10, 20, 50, 100];
  return [...new Set([...(values || defaults), currentLimit]
    .map(value => Number(value))
    .filter(value => Number.isFinite(value) && value > 0))]
    .sort((a, b) => a - b);
}

function pageWindow(page, totalPages) {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const pages = [1];
  const start = Math.max(2, page - 1);
  const end = Math.min(totalPages - 1, page + 1);

  if (start > 2) pages.push('...');
  for (let item = start; item <= end; item++) pages.push(item);
  if (end < totalPages - 1) pages.push('...');
  pages.push(totalPages);

  return pages;
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}
