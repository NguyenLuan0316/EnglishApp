// wordwave/frontend/js/pages/grammar.js
import { api }   from '../api.js';
import { toast, badgeClass } from '../utils.js';

let lessons = [];

export async function initGrammar() {
  if (lessons.length === 0) {
    try { lessons = await api.getGrammar(); }
    catch(e) { toast('Không thể tải ngữ pháp. Kiểm tra kết nối API.', 'error'); return; }
  }
  renderGrammar(lessons);
}

export function filterGrammarByLevel(level) {
  document.querySelectorAll('#page-grammar .tab').forEach(t => t.classList.remove('active'));
  event.target.classList.add('active');
  const filtered = level === 'all' ? lessons : lessons.filter(g => g.level === level);
  renderGrammar(filtered);
}

function renderGrammar(data) {
  const container = document.getElementById('grammar-list');
  if (!container) return;
  container.innerHTML = data.map(g => `
    <div class="grammar-card" id="gc-${g.id}">
      <div class="grammar-header" onclick="toggleGrammar(${g.id})">
        <div style="display:flex;align-items:center;gap:10px;margin-bottom:6px;">
          <span class="badge ${badgeClass(g.level)}">${g.level}</span>
          <div class="grammar-title">${g.title}</div>
        </div>
        <div class="grammar-desc">${g.description}</div>
      </div>
      <div class="grammar-content" id="gContent-${g.id}">
        <div style="background:rgba(99,179,237,0.08);border:1px solid var(--border);border-radius:var(--r2);padding:12px;margin-bottom:12px;font-family:var(--mono);font-size:13px;color:var(--accent2);">${g.formula}</div>
        ${g.examples.map(e => `
          <div class="example-block">
            <div class="example-en">${e.en}</div>
            <div class="example-vi">${e.vi}</div>
          </div>`).join('')}
        <div style="margin-top:12px;padding:10px 12px;background:rgba(246,173,85,0.08);border:1px solid rgba(246,173,85,0.2);border-radius:var(--r2);font-size:13px;color:var(--accent3);">
          💡 ${g.tips}
        </div>
      </div>
      <div class="grammar-footer" onclick="toggleGrammar(${g.id})">
        <span style="font-size:12px;color:var(--text3);">Nhấn để xem chi tiết</span>
        <span style="color:var(--accent);" id="gArrow-${g.id}">▼</span>
      </div>
    </div>`).join('');
}

window.toggleGrammar = function(id) {
  const el    = document.getElementById('gContent-' + id);
  const arrow = document.getElementById('gArrow-' + id);
  if (!el) return;
  el.classList.toggle('show');
  arrow.textContent = el.classList.contains('show') ? '▲' : '▼';
};
