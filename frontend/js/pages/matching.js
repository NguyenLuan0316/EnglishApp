// wordwave/frontend/js/pages/matching.js
import { api }              from '../api.js';
import { state }            from '../state.js';
import { shuffle, toast, confetti } from '../utils.js';

export async function initMatching() {
  try {
    const words = await api.getRandom({ count: 6 });
    state.matchPairs        = words;
    state.matchSelected     = null;
    state.matchSelectedSide = null;
    state.matchCorrect      = 0;
    renderMatching();
  } catch(e) {
    toast('Không thể tải từ vựng.', 'error');
  }
}

function renderMatching() {
  const pairs = state.matchPairs;
  const left  = [...pairs];
  const right = shuffle([...pairs]);

  state.matchLeft  = left;
  state.matchRight = right;

  document.getElementById('match-left').innerHTML = left.map(w =>
    `<div class="match-item" id="ml-${w.id}" onclick="selectMatch('left',${w.id})">${w.word}</div>`
  ).join('');
  document.getElementById('match-right').innerHTML = right.map(w =>
    `<div class="match-item" id="mr-${w.id}" onclick="selectMatch('right',${w.id})">${w.meaning}</div>`
  ).join('');
  document.getElementById('match-score-bar').innerHTML =
    `<span style="font-size:13px;color:var(--text2);">Đã nối đúng: <strong style="color:var(--green);">0 / ${pairs.length}</strong></span>`;
  document.getElementById('match-result').style.display = 'none';
}

window.selectMatch = function(side, id) {
  if (!state.matchSelected) {
    state.matchSelected     = id;
    state.matchSelectedSide = side;
    document.getElementById(`m${side[0]}-${id}`)?.classList.add('selected');
    return;
  }
  if (state.matchSelectedSide === side) {
    document.querySelectorAll('.match-item').forEach(e => e.classList.remove('selected'));
    state.matchSelected     = id;
    state.matchSelectedSide = side;
    document.getElementById(`m${side[0]}-${id}`)?.classList.add('selected');
    return;
  }

  const leftId  = side === 'left'  ? id : state.matchSelected;
  const rightId = side === 'right' ? id : state.matchSelected;
  document.querySelectorAll('.match-item').forEach(e => e.classList.remove('selected'));

  if (leftId === rightId) {
    document.getElementById('ml-' + leftId)?.classList.add('matched');
    document.getElementById('mr-' + rightId)?.classList.add('matched');
    state.matchCorrect++;
    state.addXP(5);
    toast('✓ Khớp! +5 XP', 'success');
    document.getElementById('match-score-bar').innerHTML =
      `<span style="font-size:13px;color:var(--text2);">Đã nối đúng: <strong style="color:var(--green);">${state.matchCorrect} / ${state.matchPairs.length}</strong></span>`;
    if (state.matchCorrect === state.matchPairs.length) {
      document.getElementById('match-result').style.display = 'block';
      document.getElementById('match-result').innerHTML = `
        <div style="padding:20px;background:rgba(104,211,145,0.1);border:1px solid var(--green);border-radius:var(--r);color:var(--green);">
          🎉 Xuất sắc! Bạn đã nối đúng tất cả!
          <button class="btn btn-success" style="margin-left:12px;" onclick="initMatching()">Chơi lại</button>
        </div>`;
      confetti();
    }
  } else {
    document.getElementById('ml-' + leftId)?.classList.add('wrong-match');
    document.getElementById('mr-' + rightId)?.classList.add('wrong-match');
    toast('✗ Không khớp!', 'error');
    setTimeout(() => {
      document.getElementById('ml-' + leftId)?.classList.remove('wrong-match');
      document.getElementById('mr-' + rightId)?.classList.remove('wrong-match');
    }, 600);
  }
  state.matchSelected = null;
  state.matchSelectedSide = null;
};

window.initMatching = initMatching;
