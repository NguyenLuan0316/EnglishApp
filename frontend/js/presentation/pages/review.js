import { state } from '../../application/state/appState.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { escapePracticeText as escape } from '../../domain/practice.js';
import { speak } from '../../shared/utils.js';

const labels = { vocabulary: 'Từ vựng', grammar: 'Ngữ pháp', patterns: 'Mẫu câu' };
let plan = null;
let requestId = 0;
let saving = false;
let revealed = false;

export async function initReview() {
  const request = ++requestId;
  const userId = state.userId;
  plan = null;
  saving = false;
  document.getElementById('review-status').textContent = 'Đang tải bài học dành cho bạn…';
  document.getElementById('review-remaining').textContent = '—';
  document.getElementById('review-breakdown').textContent = '';
  document.getElementById('review-lessons').replaceChildren();
  document.getElementById('review-area').style.display = 'none';
  document.getElementById('review-start-card').style.display = 'block';
  document.getElementById('review-start').disabled = true;
  const retry = document.getElementById('review-retry');
  retry.hidden = true;
  retry.onclick = initReview;
  try {
    const loaded = await api.getLearningDaily();
    if (request !== requestId || state.userId !== userId) return;
    plan = loaded;
    renderPlan();
  } catch (error) {
    if (request !== requestId || state.userId !== userId) return;
    document.getElementById('review-status').textContent = error.message || 'Không thể tải bài học. Vui lòng thử lại.';
    retry.hidden = false;
  }
}

function renderPlan() {
  const completed = plan.lessons.filter(lesson => lesson.completed).length;
  const remaining = plan.lessons.length - completed;
  document.getElementById('review-status').textContent = `Ngày ${plan.date.split('-').reverse().join('/')} · Lịch học theo giờ Việt Nam (UTC+7).`;
  document.getElementById('review-remaining').textContent = plan.total ? `${completed}/${plan.total} bài đã hoàn thành` : 'Chưa có bài cần học hôm nay';
  document.getElementById('review-breakdown').textContent = Object.entries(labels).map(([type, label]) => `${label}: ${plan.lessons.filter(lesson => lesson.contentType === type).length}`).join(' · ');
  document.getElementById('review-start').disabled = remaining === 0;
  document.getElementById('review-start').textContent = completed ? 'Tiếp tục học' : 'Bắt đầu học';
  document.getElementById('review-lessons').innerHTML = plan.lessons.map(lesson => `
    <div class="daily-lesson-row">
      <div><strong>${escape(lesson.title)}</strong><p>${labels[lesson.contentType]} · ${lesson.isNew ? 'Bài mới' : 'Đến hạn ôn'}${lesson.level ? ` · ${escape(lesson.level)}` : ''}</p></div>
      <span class="daily-lesson-state">${lesson.completed ? (lesson.correct ? 'Đã nhớ' : 'Sẽ ôn lại') : 'Chưa học'}</span>
    </div>`).join('');
}

window.startReview = function() {
  if (!plan || saving) return;
  document.getElementById('review-start-card').style.display = 'none';
  document.getElementById('review-area').style.display = 'block';
  renderReviewCard();
};

function renderReviewCard() {
  revealed = false;
  const lesson = plan.lessons.find(item => !item.completed);
  const area = document.getElementById('review-area');
  if (!lesson) {
    area.innerHTML = '<div class="card daily-study-card"><h2>Đã hoàn thành bài học hôm nay</h2><p>Tiến độ đã được lưu vào tài khoản. Bạn có thể tiếp tục luyện tập hoặc quay lại xem lịch học vào ngày mai.</p></div>';
    return;
  }
  area.innerHTML = `
    <section class="card daily-study-card" aria-labelledby="daily-lesson-title">
      <p>${labels[lesson.contentType]} · ${lesson.isNew ? 'Học mới' : 'Ôn tập'}</p>
      <h2 id="daily-lesson-title" tabindex="-1">${escape(lesson.title)}</h2>
      <button class="btn btn-ghost" id="daily-speak" type="button">Nghe phát âm</button>
      <p>${lesson.isNew ? 'Đọc và nghe nội dung, sau đó tự kiểm tra khả năng ghi nhớ.' : 'Thử nhớ lại ý nghĩa hoặc cấu trúc trước khi xem nội dung.'}</p>
      <button class="btn btn-secondary" id="daily-reveal" type="button">${lesson.isNew ? 'Xem bài học' : 'Xem lại nội dung'}</button>
      <div id="daily-lesson-body" hidden>
        <div class="lesson-detail">${escape(lesson.meaning)}</div>
        <div class="lesson-detail">${escape(lesson.detail)}</div>
        ${lesson.examples.map(example => `<p class="lesson-example">${escape(example)}</p>`).join('')}
        <p>Tự đánh giá sau khi học:</p>
        <div class="lesson-actions">
          <button class="btn btn-secondary" type="button" data-daily-answer="false">Cần ôn lại</button>
          <button class="btn btn-primary" type="button" data-daily-answer="true">Đã nhớ</button>
        </div>
      </div>
      <p class="lesson-feedback" id="daily-save-feedback" role="status"></p>
    </section>`;
  document.getElementById('daily-speak').hidden = lesson.contentType === 'grammar';
  document.getElementById('daily-speak').onclick = () => speak(lesson.title);
  document.getElementById('daily-reveal').onclick = event => {
    revealed = true;
    document.getElementById('daily-lesson-body').hidden = false;
    event.currentTarget.hidden = true;
  };
  area.querySelectorAll('[data-daily-answer]').forEach(button => {
    button.onclick = () => submitLesson(lesson, button.dataset.dailyAnswer === 'true');
  });
  document.getElementById('daily-lesson-title').focus();
}

async function submitLesson(lesson, correct) {
  if (saving || !revealed || lesson.completed) return;
  const request = requestId;
  const userId = state.userId;
  saving = true;
  const feedback = document.getElementById('daily-save-feedback');
  const buttons = [...document.querySelectorAll('[data-daily-answer]')];
  buttons.forEach(button => { button.disabled = true; });
  feedback.textContent = 'Đang lưu tiến độ…';
  try {
    await api.submitLearning({ contentType: lesson.contentType, contentId: lesson.contentId, correct, daily: true });
    if (request !== requestId || state.userId !== userId) return;
    lesson.completed = true;
    lesson.correct = correct;
    renderPlan();
    renderReviewCard();
  } catch (error) {
    if (request !== requestId || state.userId !== userId) return;
    feedback.textContent = `Chưa lưu được: ${error.message}. Bạn có thể bấm lại để thử.`;
    document.getElementById('review-retry').hidden = false;
    buttons.forEach(button => { button.disabled = false; });
  } finally {
    if (request === requestId) saving = false;
  }
}
