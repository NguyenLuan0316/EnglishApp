// wordwave/frontend/js/pages/dashboard.js
import { api }   from '../api.js';
import { navigate } from '../router.js';
import { levelColor } from '../utils.js';

const TOPIC_LABELS = { daily:'Đời sống', food:'Ẩm thực', travel:'Du lịch', work:'Công việc',
  technology:'Công nghệ', health:'Sức khỏe', education:'Giáo dục', science:'Khoa học', society:'Xã hội' };

export async function initDashboard() {
  // Word of day
  try {
    const words = await api.getRandom({ count: 1 });
    const w = words[0];
    if (w) {
      document.getElementById('wod-word').textContent     = w.word;
      document.getElementById('wod-phonetic').textContent = w.phonetic;
      document.getElementById('wod-meaning').textContent  = w.meaning;
      document.getElementById('wod-speak').onclick = () => {
        const u = new SpeechSynthesisUtterance(w.word);
        u.lang = 'en-US'; speechSynthesis.cancel(); speechSynthesis.speak(u);
      };
    }
  } catch(e) { console.warn('Dashboard: API offline, using static data'); }

  // Progress bars
  const levels = ['A1','A2','B1','B2','C1'];
  const mock   = { A1:{l:42,t:100}, A2:{l:31,t:100}, B1:{l:28,t:100}, B2:{l:15,t:80}, C1:{l:5,t:70} };
  const barEl  = document.getElementById('level-progress-bars');
  if (barEl) barEl.innerHTML = levels.map(lv => {
    const d = mock[lv];
    const pct = Math.round(d.l / d.t * 100);
    return `<div style="margin-bottom:10px;">
      <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
        <span style="font-size:13px;font-weight:600;">${lv}</span>
        <span style="font-size:12px;color:var(--text3);">${d.l}/${d.t} từ</span>
      </div>
      <div class="progress-track" style="height:8px;">
        <div class="progress-fill" style="width:${pct}%;background:${levelColor(lv)};"></div>
      </div>
    </div>`;
  }).join('');

  // Topic pills
  const tEl = document.getElementById('topic-suggestions');
  if (tEl) tEl.innerHTML = Object.entries(TOPIC_LABELS).map(([k,v]) =>
    `<div class="topic-pill" onclick="navigate('vocabulary')">${v}</div>`
  ).join('');
}
