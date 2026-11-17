import { api } from '../../infrastructure/api/apiClient.js';
// wordwave/frontend/js/pages/fillblank.js
import { state } from '../../application/state/appState.js';
import { loadPracticeSentences } from '../../application/practice/practiceData.js';
import { createBlankQuestion, escapePracticeText, normalizePracticeAnswer } from '../../domain/practice.js';
import { toast } from '../../shared/utils.js';

let requestId = 0;
let advanceTimer = null;
let answered = true;

export async function initFillBlank() {
  const request = ++requestId;
  clearTimeout(advanceTimer);
  answered = true;
  state.fbQuestions = [];
  state.fbIndex = 0;
  document.getElementById('fb-counter').textContent = 'Đang tải…';
  document.getElementById('fb-question-area').textContent = 'Đang lấy câu từ Từ vựng, Ngữ pháp và Mẫu câu…';
  try {
    const { sentences, failedSources } = await loadPracticeSentences();
    if (request !== requestId) return;
    if (failedSources.length) toast(`Chưa tải được: ${failedSources.join(', ')}. Bạn có thể thử lại.`, 'error');
    state.fbQuestions = sentences.map(entry => createBlankQuestion(entry)).filter(Boolean).slice(0, 6);
    if (!state.fbQuestions.length) {
      document.getElementById('fb-counter').textContent = 'Chưa có câu hỏi';
      document.getElementById('fb-question-area').textContent = 'Chưa có câu phù hợp. Chọn “Làm lại” để tải lại thư viện.';
      return;
    }
    renderFB();
  } catch {
    if (request !== requestId) return;
    document.getElementById('fb-counter').textContent = 'Chưa tải được';
    document.getElementById('fb-question-area').textContent = 'Không thể tải câu. Chọn “Làm lại” để thử lại.';
  }
}

function renderFB() {
  const q = state.fbQuestions[state.fbIndex];
  if (!q) return;
  answered = false;
  document.getElementById('fb-counter').textContent = `Câu ${state.fbIndex + 1}/${state.fbQuestions.length}`;

  document.getElementById('fb-question-area').innerHTML = `
    <div class="card" style="margin-bottom:20px;">
      <div style="font-size:18px;line-height:1.8;margin-bottom:16px;">
        ${escapePracticeText(q.before)}<input type="text" class="blank-input" id="fb-input" aria-label="Từ còn thiếu" placeholder="..." autocomplete="off" style="width:130px;">${escapePracticeText(q.after)}
      </div>
      <div style="font-size:12px;color:var(--text3);">${escapePracticeText(q.hint)}</div>
    </div>
    <p style="font-size:14px;">Hoàn thành câu gốc trong thư viện (${escapePracticeText(q.context)}).</p>
    <div style="display:flex;gap:10px;">
      <button class="btn btn-primary" id="fb-check" onclick="checkFB()">✓ Kiểm tra</button>
      <button class="btn btn-ghost"   onclick="nextFB()">→ Bỏ qua</button>
    </div>
    <div id="fb-feedback" role="status" style="margin-top:12px;"></div>
  `;
  document.getElementById('fb-input').addEventListener('keydown', event => {
    if (event.key === 'Enter') window.checkFB();
  });
}

window.checkFB = async function() {
  const q   = state.fbQuestions[state.fbIndex];
  const val = document.getElementById('fb-input')?.value.trim().toLowerCase();
  const inp = document.getElementById('fb-input');
  const fb  = document.getElementById('fb-feedback');
  if (!q || !inp || !fb || answered) return;
  if (!val) { inp.focus(); return; }
  const request = requestId;
  answered = true;
  inp.disabled = true;
  document.getElementById('fb-check').disabled = true;

  const correct = normalizePracticeAnswer(val) === normalizePracticeAnswer(q.answer);
  fb.textContent = 'Đang lưu kết quả…';
  try {
    await api.submitLearning({ contentType: q.source, contentId: q.contentId, correct });
  } catch (error) {
    if (request !== requestId) return;
    answered = false;
    inp.disabled = false;
    document.getElementById('fb-check').disabled = false;
    fb.textContent = `Chưa lưu được: ${error.message}. Bấm Kiểm tra để thử lại.`;
    return;
  }
  if (request !== requestId) return;
  if (correct) {
    inp.classList.add('correct');
    fb.innerHTML = `<div style="color:var(--green);font-size:14px;">✓ Đúng! +10 XP</div>`;
    advanceTimer = setTimeout(nextFB, 1200);
  } else {
    inp.classList.add('wrong');
    fb.innerHTML = `<div style="color:var(--red);font-size:14px;">Từ trong câu gốc: <strong style="color:var(--accent);">${escapePracticeText(q.answer)}</strong></div>`;
    advanceTimer = setTimeout(nextFB, 2000);
  }
};

function nextFB() {
  if (!state.fbQuestions.length) return;
  clearTimeout(advanceTimer);
  state.fbIndex++;
  if (state.fbIndex >= state.fbQuestions.length) {
    answered = true;
    document.getElementById('fb-counter').textContent = `Đã xong ${state.fbQuestions.length} câu`;
    document.getElementById('fb-question-area').innerHTML = `
      <div class="card" style="text-align:center;padding:40px;">
        <div style="font-size:40px;margin-bottom:8px;">📝</div>
        <div style="font-size:20px;font-weight:700;">Hoàn thành!</div>
        <button class="btn btn-primary btn-lg" style="margin-top:20px;" onclick="initFillBlank()">Làm lại</button>
      </div>`;
    return;
  }
  renderFB();
}

window.nextFB      = nextFB;
window.initFillBlank = initFillBlank;
