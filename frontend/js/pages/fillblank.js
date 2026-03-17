// wordwave/frontend/js/pages/fillblank.js
import { state } from '../state.js';
import { shuffle, toast } from '../utils.js';

const QUESTIONS = [
  { sentence:'I ___ to school yesterday.',     answer:'went',       options:['go','went','goes','going'],        hint:"Past tense của 'go'" },
  { sentence:'She ___ not like coffee.',        answer:'does',       options:['do','does','did','is'],            hint:'Present simple negative' },
  { sentence:'They are ___ a movie now.',       answer:'watching',   options:['watch','watched','watching','watches'], hint:'Present continuous (-ing)' },
  { sentence:'I have ___ my homework.',         answer:'finished',   options:['finish','finishes','finished','finishing'], hint:'Present perfect (V3)' },
  { sentence:'He ___ speak three languages.',   answer:'can',        options:['could','can','will','should'],     hint:'Khả năng ở hiện tại' },
  { sentence:'The book was ___ by Hemingway.',  answer:'written',    options:['write','wrote','written','writing'], hint:'Câu bị động (V3)' },
  { sentence:'If it rains, I ___ stay home.',   answer:'will',       options:['would','will','am','should'],      hint:'Conditional type 1' },
  { sentence:'I ___ here since 2020.',          answer:'have lived', options:['live','lived','have lived','am living'], hint:"Present perfect + 'since'" },
];

export function initFillBlank() {
  state.fbQuestions = shuffle([...QUESTIONS]).slice(0, 6);
  state.fbIndex     = 0;
  renderFB();
}

function renderFB() {
  const q = state.fbQuestions[state.fbIndex];
  if (!q) return;
  document.getElementById('fb-counter').textContent = `Câu ${state.fbIndex + 1}/${state.fbQuestions.length}`;

  document.getElementById('fb-question-area').innerHTML = `
    <div class="card" style="margin-bottom:20px;">
      <div style="font-size:18px;line-height:1.8;margin-bottom:16px;">
        ${q.sentence.replace('___', `<input type="text" class="blank-input" id="fb-input" placeholder="..." autocomplete="off" style="width:130px;">`)}
      </div>
      <div style="font-size:12px;color:var(--text3);">💡 Gợi ý: ${q.hint}</div>
    </div>
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:16px;">
      ${q.options.map(o => `<button class="quiz-option" onclick="fillOpt('${o}')">${o}</button>`).join('')}
    </div>
    <div style="display:flex;gap:10px;">
      <button class="btn btn-primary" onclick="checkFB()">✓ Kiểm tra</button>
      <button class="btn btn-ghost"   onclick="nextFB()">→ Bỏ qua</button>
    </div>
    <div id="fb-feedback" style="margin-top:12px;"></div>
  `;
}

window.fillOpt = function(word) {
  const inp = document.getElementById('fb-input');
  if (inp) inp.value = word;
};

window.checkFB = function() {
  const q   = state.fbQuestions[state.fbIndex];
  const val = document.getElementById('fb-input')?.value.trim().toLowerCase();
  const inp = document.getElementById('fb-input');
  const fb  = document.getElementById('fb-feedback');
  if (!q || !inp || !fb) return;

  if (val === q.answer.toLowerCase()) {
    inp.classList.add('correct');
    fb.innerHTML = `<div style="color:var(--green);font-size:14px;">✓ Đúng! +10 XP</div>`;
    state.addXP(10);
    setTimeout(nextFB, 1200);
  } else {
    inp.classList.add('wrong');
    fb.innerHTML = `<div style="color:var(--red);font-size:14px;">✗ Sai! Đáp án: <strong style="color:var(--accent);">${q.answer}</strong></div>`;
    setTimeout(nextFB, 2000);
  }
};

function nextFB() {
  state.fbIndex++;
  if (state.fbIndex >= state.fbQuestions.length) {
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
