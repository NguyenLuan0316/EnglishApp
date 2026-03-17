// wordwave/frontend/js/pages/listening.js
import { api }           from '../api.js';
import { state }         from '../state.js';
import { speak, toast }  from '../utils.js';

let wordPool = [];

export async function initListening() {
  buildBars();
  try {
    if (wordPool.length === 0) {
      const res = await api.getVocab({ limit: 200 });
      wordPool = res.data || [];
    }
  } catch(e) {}
  nextListenWord();
}

function buildBars() {
  const c = document.getElementById('audio-bars');
  if (!c) return;
  c.innerHTML = Array.from({length:20}, (_, i) =>
    `<div class="audio-bar" style="animation-delay:${i*0.05}s;animation-duration:${0.5+Math.random()*0.5}s;animation-play-state:paused;height:8px;"></div>`
  ).join('');
}

function nextListenWord() {
  const pool = wordPool.filter(v => v.level === state.listenLevel);
  if (pool.length === 0) return;
  const words = [...pool].sort(() => Math.random() - 0.5);
  state.listenWord = words[0];

  const inp = document.getElementById('listen-input');
  if (inp) { inp.value = ''; inp.className = 'blank-input'; inp.style.cssText = 'width:100%;text-align:left;padding:12px 16px;'; }
  document.getElementById('listen-feedback').style.display = 'none';
  document.getElementById('listen-play-btn').textContent   = '🔊 Phát âm';
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
  if (!state.listenWord) return;
  const val = document.getElementById('listen-input')?.value.trim().toLowerCase();
  const fb  = document.getElementById('listen-feedback');
  fb.style.display = 'block';

  if (val === state.listenWord.word.toLowerCase()) {
    document.getElementById('listen-input').classList.add('correct');
    fb.innerHTML = `<div style="color:var(--green);">✓ Đúng! "${state.listenWord.word}" — ${state.listenWord.meaning} (+8 XP)</div>`;
    state.listenCorrect++;
    state.addXP(8);
  } else {
    document.getElementById('listen-input').classList.add('wrong');
    fb.innerHTML = `<div style="color:var(--red);">✗ Sai. Từ cần nghe là: <strong style="color:var(--accent);">${state.listenWord.word}</strong></div>`;
    state.listenWrong++;
  }
  updateListenStats();
  setTimeout(nextListenWord, 2000);
};

window.skipListening = function() {
  state.listenWrong++;
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
