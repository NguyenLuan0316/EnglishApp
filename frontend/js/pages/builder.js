// wordwave/frontend/js/pages/builder.js
import { state }                    from '../state.js';
import { shuffle, toast, confetti } from '../utils.js';

const SENTENCES = [
  { vi:'Tôi đang học tiếng Anh.',   words:['I','am','studying','English','.'],       extra:['She','He','learning','French','!'] },
  { vi:'Cô ấy thích ăn phở.',       words:['She','likes','eating','pho','.'],        extra:['He','loves','drinking','pizza','!'] },
  { vi:'Chúng tôi sẽ đi du lịch.',  words:['We','will','go','traveling','.'],        extra:['They','travel','going','she','!'] },
  { vi:'Bạn có thể giúp tôi không?',words:['Can','you','help','me','?'],             extra:['Could','please','I','them','!'] },
  { vi:'Hôm nay thời tiết đẹp.',    words:['The','weather','is','nice','today','.'], extra:['This','bad','are','cold','!'] },
];

export function initBuilder() {
  const s = SENTENCES[Math.floor(Math.random() * SENTENCES.length)];
  state.builderTarget   = s.words;
  state.builderSentence = [];
  state.builderWords    = shuffle([...s.words, ...s.extra]);
  document.getElementById('builder-prompt').textContent    = s.vi;
  document.getElementById('builder-feedback').style.display = 'none';
  renderBuilder();
}

function renderBuilder() {
  // track placed words by index to handle duplicates
  const usedIndexes = new Set();
  state.builderSentence.forEach(w => {
    const idx = state.builderWords.findIndex((bw, i) => bw === w && !usedIndexes.has(i));
    if (idx !== -1) usedIndexes.add(idx);
  });

  document.getElementById('builder-words').innerHTML = state.builderWords.map((w, i) =>
    usedIndexes.has(i)
      ? `<span class="word-token" style="opacity:0.35;pointer-events:none;">${w}</span>`
      : `<span class="word-token" data-idx="${i}" onclick="addToBuilder(this)">${w}</span>`
  ).join('');

  document.getElementById('builder-slots').innerHTML = state.builderSentence.map((w, i) =>
    `<span class="slot-token" data-pos="${i}" onclick="removeFromBuilder(this)">${w} <span style="font-size:10px;opacity:0.6">✕</span></span>`
  ).join('');
}

window.addToBuilder = function(el) {
  const w = el.textContent;
  state.builderSentence.push(w);
  renderBuilder();
};

window.removeFromBuilder = function(el) {
  const pos = parseInt(el.dataset.pos);
  state.builderSentence.splice(pos, 1);
  renderBuilder();
};

window.clearBuilder = function() {
  state.builderSentence = [];
  renderBuilder();
  document.getElementById('builder-feedback').style.display = 'none';
};

window.checkBuilder = function() {
  const attempt = state.builderSentence.join(' ');
  const correct = state.builderTarget.join(' ');
  const fb      = document.getElementById('builder-feedback');
  fb.style.display = 'block';
  if (attempt === correct) {
    fb.innerHTML = `<div style="padding:14px;background:rgba(104,211,145,0.1);border:1px solid var(--green);border-radius:var(--r2);color:var(--green);">✓ Chính xác! Xuất sắc! +15 XP</div>`;
    state.addXP(15);
    confetti();
  } else {
    fb.innerHTML = `<div style="padding:14px;background:rgba(252,129,129,0.08);border:1px solid var(--red);border-radius:var(--r2);">
      <div style="color:var(--red);margin-bottom:4px;">✗ Chưa đúng</div>
      <div style="font-size:13px;color:var(--text2);">Đáp án: <strong style="color:var(--accent);">${correct}</strong></div>
    </div>`;
  }
};

window.initBuilder = initBuilder;
