export function renderPagination(containerId, options) {
  const container = document.getElementById(containerId);
  if (!container) return;

  const total = Number(options.total || 0);
  const limit = Math.max(1, Number(options.limit || 20));
  const totalPages = Math.max(1, Math.ceil(total / limit));
  const page = clamp(Number(options.page || 1), 1, totalPages);

  if (total <= limit) {
    container.innerHTML = '';
    return;
  }

  const pages = pageWindow(page, totalPages);
  const from = total === 0 ? 0 : ((page - 1) * limit) + 1;
  const to = Math.min(page * limit, total);

  container.innerHTML = `
    <div class="pagination">
      <div class="pagination-summary">Hiển thị ${from}-${to} / ${total}</div>
      <div class="pagination-controls">
        <button type="button" class="pagination-btn" data-page="${page - 1}" ${page === 1 ? 'disabled' : ''}>Trước</button>
        ${pages.map(item => item === '...'
          ? '<span class="pagination-ellipsis">...</span>'
          : `<button type="button" class="pagination-btn${item === page ? ' active' : ''}" data-page="${item}">${item}</button>`
        ).join('')}
        <button type="button" class="pagination-btn" data-page="${page + 1}" ${page === totalPages ? 'disabled' : ''}>Sau</button>
      </div>
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
