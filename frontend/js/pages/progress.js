// wordwave/frontend/js/pages/progress.js
import { api }          from '../api.js';
import { toast, levelColor } from '../utils.js';

export async function initProgress() {
  let prog = null;
  try { prog = await api.getProgress(); } catch(e) {}

  // Fallback mock nếu API chưa có dữ liệu
  const byLevel = prog?.byLevel || {
    A1:{ Total:50, Learned:42 }, A2:{ Total:75, Learned:31 },
    B1:{ Total:88, Learned:28 }, B2:{ Total:50, Learned:15 }, C1:{ Total:37, Learned:5 }
  };

  // Level bars
  const lbEl = document.getElementById('progress-level-bars');
  if (lbEl) lbEl.innerHTML = Object.entries(byLevel).map(([lv, d]) => {
    const pct = Math.round((d.Learned || d.learned || 0) / (d.Total || d.total || 1) * 100);
    return `<div>
      <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
        <span style="font-size:13px;font-weight:600;">${lv}</span>
        <span style="font-size:12px;color:var(--text3);">${d.Learned ?? d.learned}/${d.Total ?? d.total} (${pct}%)</span>
      </div>
      <div class="progress-track" style="height:10px;">
        <div class="progress-fill" style="width:${pct}%;background:${levelColor(lv)};"></div>
      </div>
    </div>`;
  }).join('');

  // Topic bars (mock)
  const topics = { daily:{l:55,t:80}, food:{l:28,t:40}, travel:{l:20,t:40},
    work:{l:35,t:60}, technology:{l:18,t:30}, health:{l:15,t:25} };
  const labels = { daily:'Đời sống', food:'Ẩm thực', travel:'Du lịch',
    work:'Công việc', technology:'Công nghệ', health:'Sức khỏe' };
  const tbEl = document.getElementById('progress-topic-bars');
  if (tbEl) tbEl.innerHTML = Object.entries(topics).map(([t, d]) => {
    const pct = Math.round(d.l / d.t * 100);
    return `<div>
      <div style="display:flex;justify-content:space-between;margin-bottom:3px;">
        <span style="font-size:12px;color:var(--text2);">${labels[t]}</span>
        <span style="font-size:11px;color:var(--text3);">${pct}%</span>
      </div>
      <div class="progress-track" style="height:6px;">
        <div class="progress-fill" style="width:${pct}%;background:var(--accent);"></div>
      </div>
    </div>`;
  }).join('');

  // Heatmap
  const hEl = document.getElementById('activity-heatmap');
  if (hEl) {
    const cells = Array.from({length: 364}, (_, i) => {
      const v = Math.random();
      const c = v < 0.3 ? 'var(--bg3)' : v < 0.5 ? 'rgba(99,179,237,0.2)' : v < 0.7 ? 'rgba(99,179,237,0.45)' : v < 0.9 ? 'rgba(99,179,237,0.7)' : 'var(--accent)';
      return `<div class="heat-cell" style="background:${c};" title="Ngày ${i+1}"></div>`;
    });
    hEl.innerHTML = `<div class="heatmap">${cells.join('')}</div>`;
  }
}
