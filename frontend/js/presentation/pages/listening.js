// wordwave/frontend/js/pages/listening.js
import { state } from '../../application/state/appState.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { escapePracticeText } from '../../domain/practice.js';
import { speak, toast } from '../../shared/utils.js';

let listenRequest = 0;
let listenTimer = null;
let listenAnswered = true;

export async function initListening() {
  buildBars();
  await nextListenWord();
}

function buildBars() {
  const c = document.getElementById('audio-bars');
  if (!c) return;
  c.innerHTML = Array.from({length:20}, (_, i) =>
    `<div class="audio-bar" style="animation-delay:${i*0.05}s;animation-duration:${0.5+Math.random()*0.5}s;animation-play-state:paused;height:8px;"></div>`
  ).join('');
}

async function nextListenWord() {
  const request = ++listenRequest;
  clearTimeout(listenTimer);
  listenAnswered = true;
  state.listenWord = null;
  const input = document.getElementById('listen-input');
  input.value = '';
  input.className = 'blank-input';
  input.disabled = true;
  const feedback = document.getElementById('listen-feedback');
  feedback.style.display = 'none';
  const button = document.getElementById('listen-play-btn');
  button.textContent = 'Đang tải…';
  button.disabled = true;
  try {
    const words = await api.getRandom({ count: 1, level: state.listenLevel });
    if (request !== listenRequest) return;
    state.listenWord = words[0] || null;
    if (!state.listenWord) {
      feedback.style.display = 'block';
      feedback.textContent = 'Chưa có từ ở độ khó này. Chọn độ khó khác hoặc bấm Bỏ qua để thử lại.';
      return;
    }
    listenAnswered = false;
    input.disabled = false;
    button.disabled = false;
  } catch {
    if (request !== listenRequest) return;
    feedback.style.display = 'block';
    feedback.textContent = 'Không thể tải từ. Bấm Bỏ qua để thử lại.';
    toast('Không thể tải từ luyện nghe.', 'error');
  } finally {
    if (request === listenRequest) button.textContent = '🔊 Phát âm';
  }
}

window.playListenWord = function() {
  if (!state.listenWord) return;
  speak(state.listenWord.word);
  document.querySelectorAll('.audio-bar').forEach(b => {
    b.style.animationPlayState = 'running';
  });
  setTimeout(() => document.querySelectorAll('.audio-bar').forEach(b => {
    b.style.animationPlayState = 'paused';
  }), 2200);
};

window.checkListening = function() {
  if (!state.listenWord || listenAnswered) return;
  const val = document.getElementById('listen-input')?.value.trim().toLowerCase();
  if (!val) return;
  listenAnswered = true;
  const fb  = document.getElementById('listen-feedback');
  fb.style.display = 'block';

  if (val === state.listenWord.word.toLowerCase()) {
    document.getElementById('listen-input').classList.add('correct');
    fb.innerHTML = `<div style="color:var(--green);">✓ Đúng! "${escapePracticeText(state.listenWord.word)}" — ${escapePracticeText(state.listenWord.meaning)} (+10 XP)</div>`;
    state.listenCorrect++;
  } else {
    document.getElementById('listen-input').classList.add('wrong');
    fb.innerHTML = `<div style="color:var(--red);">✗ Sai. Từ cần nghe là: <strong style="color:var(--accent);">${escapePracticeText(state.listenWord.word)}</strong></div>`;
    state.listenWrong++;
  }
  api.submitReview(state.listenWord.id, val === state.listenWord.word.toLowerCase()).catch(() => toast('Chưa lưu được tiến độ luyện nghe.', 'error'));
  updateListenStats();
  listenTimer = setTimeout(nextListenWord, 2000);
};

window.skipListening = function() {
  if (state.listenWord && !listenAnswered) {
    state.listenWrong++;
    api.submitReview(state.listenWord.id, false).catch(() => toast('Chưa lưu được tiến độ luyện nghe.', 'error'));
  }
  updateListenStats();
  nextListenWord();
};

window.setListenLevel = function(el, level) {
  document.querySelectorAll('#page-listening .tab').forEach(t => t.classList.remove('active'));
  el.classList.add('active');
  state.listenLevel = level;
  nextListenWord();
};

function updateListenStats() {
  const el = document.getElementById('listen-stats');
  if (el) el.textContent = `${state.listenCorrect} đúng / ${state.listenWrong} sai`;
}
