import { api } from '../../infrastructure/api/apiClient.js';
import { authState } from '../../application/state/authState.js';
import { state } from '../../application/state/appState.js';
import { LEVELS, TOPIC_LABELS } from '../../domain/vocabulary.js';
import { escapePracticeText as escape } from '../../domain/practice.js';
import { levelColor, speak } from '../../shared/utils.js';

let requestId = 0;

function renderSummary(summary) {
  for (const type of ['vocabulary', 'grammar', 'patterns']) {
    document.getElementById(`dashboard-${type}`).textContent = summary.byContent[type]?.learned ?? 0;
  }
  document.getElementById('dashboard-daily').textContent = `${summary.dailyCompleted}/${summary.dailyTotal}`;
  const accuracy = summary.attempts ? `${Math.round(summary.correctCount / summary.attempts * 100)}%` : 'Chưa có';
  document.getElementById('dashboard-study-summary').textContent = `${summary.streak} ngày học liên tiếp · ${summary.totalXp} XP · Ghi nhớ: ${accuracy} · ${summary.todayAttempts} lượt học hôm nay`;
  document.getElementById('level-progress-bars').innerHTML = Object.entries(summary.byLevel).sort(([a], [b]) => LEVELS.indexOf(a) - LEVELS.indexOf(b)).map(([level, data]) => {
    const percent = data.total ? Math.round(data.learned / data.total * 100) : 0;
    return `<div style="margin-bottom:14px;"><div style="display:flex;justify-content:space-between;margin-bottom:4px;"><span>${escape(level)}</span><span>${data.learned}/${data.total} từ</span></div><div class="progress-track" style="height:8px;"><div class="progress-fill" style="width:${percent}%;background:${levelColor(level)};"></div></div></div>`;
  }).join('') || 'Chưa có dữ liệu từ vựng.';
}

export async function initDashboard() {
  const request = ++requestId;
  const userId = state.userId;
  document.getElementById('dashboard-greeting').textContent = `Chào ${authState.user?.displayName || 'bạn'}!`;
  document.getElementById('dashboard-study-summary').textContent = 'Đang tải tiến độ tài khoản…';
  for (const type of ['vocabulary', 'grammar', 'patterns', 'daily']) document.getElementById(`dashboard-${type}`).textContent = '—';
  document.getElementById('level-progress-bars').replaceChildren();
  document.getElementById('wod-word').textContent = '—';
  document.getElementById('wod-phonetic').textContent = '';
  document.getElementById('wod-meaning').textContent = '';
  document.getElementById('wod-speak').disabled = true;
  const retry = document.getElementById('dashboard-retry');
  retry.hidden = true;
  retry.onclick = initDashboard;
  try {
    const [summary, daily] = await Promise.all([api.getLearningProgress(), api.getLearningDaily()]);
    if (request !== requestId || state.userId !== userId) return;
    state.applyLearningSummary(summary);
    renderSummary(summary);
    const words = daily.lessons.filter(lesson => lesson.contentType === 'vocabulary');
    const word = words.find(lesson => !lesson.completed) || words[0];
    if (word) {
      document.getElementById('wod-word').textContent = word.title;
      document.getElementById('wod-phonetic').textContent = word.detail;
      document.getElementById('wod-meaning').textContent = word.meaning;
      document.getElementById('wod-speak').disabled = false;
      document.getElementById('wod-speak').onclick = () => speak(word.title);
    } else document.getElementById('wod-meaning').textContent = 'Chưa có từ cần học hôm nay.';
  } catch (error) {
    if (request !== requestId || state.userId !== userId) return;
    document.getElementById('dashboard-study-summary').textContent = error.message || 'Không thể tải tiến độ. Vui lòng thử lại.';
    retry.hidden = false;
  }
  document.getElementById('topic-suggestions').innerHTML = Object.values(TOPIC_LABELS).map(label => `<button class="topic-pill" type="button" onclick="navigate('vocabulary')">${escape(label)}</button>`).join('');
}

window.addEventListener('wordwave:learning-updated', event => {
  if (document.getElementById('page-dashboard')?.classList.contains('active')) renderSummary(event.detail);
});
