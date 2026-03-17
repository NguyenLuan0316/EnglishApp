// wordwave/frontend/js/pages/quiz.js
import { api }                 from '../api.js';
import { state }               from '../state.js';
import { shuffle, toast, confetti } from '../utils.js';

let allPool = [];

export async function initQuiz() {
  try {
    if (allPool.length === 0) {
      const res = await api.getVocab({ limit: 200 });
      allPool = res.data || [];
    }
    const pool       = shuffle([...allPool]);
    state.quizWords  = pool.slice(0, 10);
    state.quizIndex  = 0;
    state.quizScore  = 0;
    state.quizAnswered = false;
    document.getElementById('quiz-score').textContent = '0 ✓';
    renderQuestion();
  } catch(e) {
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
      <div style="font-size:36px;font-weight:700;margin-bottom:8px;">${w.word}</div>
      <div style="font-size:14px;color:var(--accent2);font-family:var(--mono);">${w.phonetic}</div>
    </div>
    ${opts.map((o, i) => `
      <button class="quiz-option" id="qopt-${o.id}" onclick="answerQuiz(${w.id}, ${o.id}, this)">
        <span class="option-letter">${letters[i]}</span>${o.meaning}
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
    state.addXP(10);
    toast('✓ Đúng rồi! +10 XP', 'success');
    api.submitReview(correctId, true).catch(() => {});
  } else {
    btn.classList.add('wrong');
    document.getElementById('qopt-' + correctId)?.classList.add('correct');
    const correctWord = allPool.find(v => v.id === correctId);
    toast(`✗ Sai! Đáp án: ${correctWord?.meaning || ''}`, 'error');
    api.submitReview(correctId, false).catch(() => {});
  }

  setTimeout(() => {
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
