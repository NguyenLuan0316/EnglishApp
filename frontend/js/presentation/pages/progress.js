import { topicLabel } from '../../domain/vocabulary.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { state } from '../../application/state/appState.js';
import { escapePracticeText as escape } from '../../domain/practice.js';
import { levelColor } from '../../shared/utils.js';

let requestId = 0;
function renderBars(container, groups, label, color) {
  document.getElementById(container).innerHTML = Object.entries(groups).map(([key, data]) => {
    const percent = data.total ? Math.round(data.learned / data.total * 100) : 0;
    return `<div><div style="display:flex;justify-content:space-between;margin-bottom:6px;"><span>${escape(label(key))}</span><span>${data.learned}/${data.total} (${percent}%)</span></div><div class="progress-track" style="height:8px;"><div class="progress-fill" style="width:${percent}%;background:${color(key)};"></div></div></div>`;
  }).join('') || 'Chưa có dữ liệu.';
}

function renderProgress(summary) {
  document.getElementById('progress-learned').textContent = summary.totalLearned;
  document.getElementById('progress-accuracy').textContent = summary.attempts ? `${Math.round(summary.correctCount / summary.attempts * 100)}%` : '—';
  document.getElementById('progress-streak').textContent = summary.streak;
  document.getElementById('xp-total').textContent = summary.totalXp;
  document.getElementById('progress-status').textContent = 'Tiến độ được lưu theo tài khoản. “Đã học” là nội dung có ít nhất một lượt nhớ đúng.';
  const labels = { vocabulary: 'Từ vựng', grammar: 'Ngữ pháp', patterns: 'Mẫu câu' };
  document.getElementById('progress-content-summary').textContent = Object.entries(labels).map(([type, label]) => `${label}: ${summary.byContent[type]?.learned ?? 0}/${summary.byContent[type]?.total ?? 0}`).join(' · ');
  renderBars('progress-level-bars', summary.byLevel, key => key, levelColor);
  renderBars('progress-topic-bars', summary.byTopic, topicLabel, () => 'var(--accent)');
  const activity = new Map(summary.activity.map(day => [day.date, day.attempts]));
  const end = new Date(`${summary.date}T00:00:00Z`);
  const cells = Array.from({ length: 364 }, (_, index) => {
    const date = new Date(end.getTime() - (363 - index) * 86400000).toISOString().slice(0, 10);
    const count = activity.get(date) || 0;
    const opacity = count ? Math.min(1, 0.25 + count / 20) : 1;
    return `<div class="heat-cell" style="background:${count ? 'var(--accent)' : 'var(--bg3)'};opacity:${opacity};" title="${date}: ${count} lượt học"></div>`;
  });
  document.getElementById('activity-heatmap').innerHTML = `<div class="heatmap">${cells.join('')}</div>`;
}

export async function initProgress() {
  const request = ++requestId;
  const userId = state.userId;
  document.getElementById('progress-status').textContent = 'Đang tải tiến độ…';
  for (const id of ['progress-learned', 'progress-accuracy', 'progress-streak', 'xp-total']) document.getElementById(id).textContent = '—';
  for (const id of ['progress-content-summary', 'progress-level-bars', 'progress-topic-bars', 'activity-heatmap']) document.getElementById(id).replaceChildren();
  const retry = document.getElementById('progress-retry');
  retry.hidden = true;
  retry.onclick = initProgress;
  try {
    const summary = await api.getLearningProgress();
    if (request !== requestId || state.userId !== userId) return;
    state.applyLearningSummary(summary);
    renderProgress(summary);
  } catch (error) {
    if (request !== requestId || state.userId !== userId) return;
    document.getElementById('progress-status').textContent = error.message || 'Không thể tải tiến độ. Vui lòng thử lại.';
    retry.hidden = false;
  }
}

window.addEventListener('wordwave:learning-updated', event => {
  if (document.getElementById('page-progress')?.classList.contains('active')) renderProgress(event.detail);
});
