// wordwave/frontend/js/pages/patterns.js
import { api }   from '../api.js';
import { speak, toast } from '../utils.js';

export async function initPatterns() {
  let patterns = [];
  try { patterns = await api.getPatterns(); }
  catch(e) { toast('Không thể tải mẫu câu.', 'error'); return; }

  const container = document.getElementById('patterns-list');
  if (!container) return;
  container.innerHTML = patterns.map(p => `
    <div class="card">
      <div style="display:flex;align-items:flex-start;gap:12px;">
        <div style="width:36px;height:36px;border-radius:50%;background:rgba(99,179,237,0.1);border:1px solid var(--border);display:flex;align-items:center;justify-content:center;flex-shrink:0;font-size:16px;">💬</div>
        <div style="flex:1;">
          <div style="font-size:17px;font-weight:600;margin-bottom:2px;">${p.sentence}</div>
          <div style="font-size:14px;color:var(--accent2);margin-bottom:6px;">${p.meaning}</div>
          <div style="font-size:13px;color:var(--text2);margin-bottom:10px;">${p.explanation}</div>
          <div style="display:flex;flex-direction:column;gap:4px;">
            ${p.examples.map(e => `<div class="example-block" style="padding:8px 12px;"><div class="example-en" style="font-size:13px;">${e}</div></div>`).join('')}
          </div>
          <button class="btn btn-ghost btn-sm" style="margin-top:10px;" onclick="window._speak('${p.sentence.replace(/'/g,"\\'")}')">🔊 Nghe</button>
        </div>
      </div>
    </div>`).join('');
}
