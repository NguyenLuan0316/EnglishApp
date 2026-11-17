// wordwave/frontend/js/pages/quiz.js
import { state } from '../../application/state/appState.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { escapePracticeText } from '../../domain/practice.js';
import { shuffle, toast, confetti } from '../../shared/utils.js';

let allPool = [];
let quizRequest = 0;
let questionTimer = null;

export async function initQuiz() {
  const request = ++quizRequest;
  clearTimeout(questionTimer);
  state.quizAnswered = true;
  state.quizWords = [];
  document.getElementById('quiz-question-area').textContent = 'Đang tải câu hỏi từ Từ vựng…';
  try {
    const words = await api.getRandom({ count: 100 });
    if (request !== quizRequest) return;
    const seen = new Set();
    allPool = words.filter(word => {
      const key = word.meaning?.trim().toLowerCase();
      if (!word.word || !key || seen.has(key)) return false;
      seen.add(key);
      return true;
    });
    if (allPool.length < 2) {
      document.getElementById('quiz-question-area').textContent = 'Chưa đủ từ có nghĩa khác nhau để tạo câu hỏi. Chọn làm lại để thử lại.';
      return;
    }
    const pool       = shuffle([...allPool]);
    state.quizWords  = pool.slice(0, 10);
    state.quizIndex  = 0;
    state.quizScore  = 0;
    state.quizAnswered = false;
    document.getElementById('quiz-score').textContent = '0 ✓';
    renderQuestion();
  } catch(e) {
    if (request !== quizRequest) return;
    document.getElementById('quiz-question-area').textContent = 'Không thể tải câu hỏi. Hãy thử lại.';
    toast('Không thể tải câu hỏi.', 'error');
  }
}

function renderQuestion() {
  const w     = state.quizWords[state.quizIndex];
  if (!w) return;
  const total = state.quizWords.length;
  const pct   = Math.round((state.quizIndex + 1) / total * 100);

  document.getElementById('quiz-progress-text').textContent = `Câu ${state.quizIndex + 1}/${total}`;
  document.getElementById('quiz-progress-bar').style.width  = pct + '%';

  // 4 lựa chọn: 1 đúng + 3 sai
  const others = shuffle(allPool.filter(v => v.id !== w.id)).slice(0, 3);
  const opts   = shuffle([w, ...others]);
  state.quizAnswered = false;

  const letters = ['A', 'B', 'C', 'D'];
  document.getElementById('quiz-question-area').innerHTML = `
    <div class="card" style="margin-bottom:20px;text-align:center;padding:28px;">
      <div style="font-size:11px;color:var(--text3);margin-bottom:8px;text-transform:uppercase;letter-spacing:1px;">Từ này có nghĩa là gì?</div>
      <div style="font-size:36px;font-weight:700;margin-bottom:8px;">${escapePracticeText(w.word)}</div>
      <div style="font-size:14px;color:var(--accent2);font-family:var(--mono);">${escapePracticeText(w.phonetic || '')}</div>
    </div>
    ${opts.map((o, i) => `
      <button class="quiz-option" id="qopt-${o.id}" onclick="answerQuiz(${w.id}, ${o.id}, this)">
        <span class="option-letter">${letters[i]}</span>${escapePracticeText(o.meaning)}
      </button>`).join('')}
  `;
}

window.answerQuiz = function(correctId, selectedId, btn) {
  if (state.quizAnswered) return;
  state.quizAnswered = true;
  document.querySelectorAll('.quiz-option').forEach(b => b.disabled = true);

  if (selectedId === correctId) {
    btn.classList.add('correct');
    state.quizScore++;
    document.getElementById('quiz-score').textContent = state.quizScore + ' ✓';
    toast('✓ Đúng rồi! +10 XP', 'success');
    api.submitReview(correctId, true).catch(() => toast('Chưa lưu được tiến độ. Vui lòng thử lại khi có kết nối.', 'error'));
  } else {
    btn.classList.add('wrong');
    document.getElementById('qopt-' + correctId)?.classList.add('correct');
    const correctWord = allPool.find(v => v.id === correctId);
    toast(`✗ Sai! Đáp án: ${correctWord?.meaning || ''}`, 'error');
    api.submitReview(correctId, false).catch(() => toast('Chưa lưu được tiến độ. Vui lòng thử lại khi có kết nối.', 'error'));
  }

  questionTimer = setTimeout(() => {
    state.quizIndex++;
    if (state.quizIndex >= state.quizWords.length) {
      const pct = Math.round(state.quizScore / state.quizWords.length * 100);
      document.getElementById('quiz-question-area').innerHTML = `
        <div class="card" style="text-align:center;padding:40px;">
          <div style="font-size:48px;margin-bottom:8px;">${pct >= 80 ? '🏆' : pct >= 60 ? '👍' : '📚'}</div>
          <div style="font-size:24px;font-weight:700;margin-bottom:4px;">${state.quizScore}/${state.quizWords.length} đúng</div>
          <div style="font-size:16px;color:var(--text2);margin-bottom:20px;">Tỷ lệ: ${pct}%</div>
          <button class="btn btn-primary btn-lg" onclick="initQuiz()">↺ Làm lại</button>
        </div>`;
      if (pct >= 80) confetti();
    } else {
      renderQuestion();
    }
  }, 1200);
};

window.initQuiz = initQuiz;
