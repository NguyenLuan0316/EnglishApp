// wordwave/frontend/js/pages/builder.js
import { api } from '../../infrastructure/api/apiClient.js';
import { state } from '../../application/state/appState.js';
import { loadPracticeSentences } from '../../application/practice/practiceData.js';
import { escapePracticeText } from '../../domain/practice.js';
import { shuffle, toast, confetti } from '../../shared/utils.js';

let builderRequest = 0;
let builderAnswered = false;
let builderEntry = null;

export async function initBuilder() {
  const request = ++builderRequest;
  builderAnswered = false;
  builderEntry = null;
  state.builderTarget = [];
  state.builderSentence = [];
  state.builderWords = [];
  document.getElementById('builder-prompt').textContent = 'Đang tải câu từ thư viện học…';
  document.getElementById('builder-feedback').style.display = 'none';
  renderBuilder();
  try {
    const { sentences, failedSources } = await loadPracticeSentences();
    if (request !== builderRequest) return;
    if (failedSources.length) toast(`Chưa tải được: ${failedSources.join(', ')}. Bạn có thể thử lại.`, 'error');
    if (!sentences.length) {
      document.getElementById('builder-prompt').textContent = 'Chưa có câu phù hợp. Chọn “Câu mới” để thử lại.';
      return;
    }
    const availableSources = [...new Set(sentences.map(entry => entry.source))];
    const source = availableSources[Math.floor(Math.random() * availableSources.length)];
    const pool = sentences.filter(entry => entry.source === source);
    const sentence = pool[Math.floor(Math.random() * pool.length)];
    builderEntry = sentence;
    state.builderTarget = sentence.words;
    state.builderWords = shuffle([...sentence.words]);
    const sourceLabel = { vocabulary: 'Từ vựng', grammar: 'Ngữ pháp', patterns: 'Mẫu câu' }[sentence.source];
    document.getElementById('builder-prompt').textContent = `${sentence.translation} (${sourceLabel})`;
    renderBuilder();
  } catch {
    if (request !== builderRequest) return;
    document.getElementById('builder-prompt').textContent = 'Không thể tải câu. Chọn “Câu mới” để thử lại.';
  }
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
      ? `<span class="word-token" style="opacity:0.35;pointer-events:none;">${escapePracticeText(w)}</span>`
      : `<span class="word-token" data-idx="${i}" onclick="addToBuilder(this)">${escapePracticeText(w)}</span>`
  ).join('');

  document.getElementById('builder-slots').innerHTML = state.builderSentence.map((w, i) =>
    `<span class="slot-token" data-pos="${i}" onclick="removeFromBuilder(this)">${escapePracticeText(w)} <span style="font-size:12px;opacity:0.6">✕</span></span>`
  ).join('');
}

window.addToBuilder = function(el) {
  if (!state.builderTarget.length || builderAnswered) return;
  const w = state.builderWords[Number(el.dataset.idx)];
  state.builderSentence.push(w);
  renderBuilder();
};

window.removeFromBuilder = function(el) {
  if (builderAnswered) return;
  const pos = parseInt(el.dataset.pos);
  state.builderSentence.splice(pos, 1);
  renderBuilder();
};

window.clearBuilder = function() {
  if (builderAnswered) return;
  state.builderSentence = [];
  renderBuilder();
  document.getElementById('builder-feedback').style.display = 'none';
};

window.checkBuilder = async function() {
  if (!state.builderTarget.length || builderAnswered) return;
  const request = builderRequest;
  builderAnswered = true;
  const attempt = state.builderSentence.join(' ');
  const correct = state.builderTarget.join(' ');
  const fb      = document.getElementById('builder-feedback');
  fb.style.display = 'block';
  fb.textContent = 'Đang lưu kết quả…';
  try {
    await api.submitLearning({ contentType: builderEntry.source, contentId: builderEntry.contentId, correct: attempt === correct });
  } catch (error) {
    if (request !== builderRequest) return;
    builderAnswered = false;
    fb.textContent = `Chưa lưu được: ${error.message}. Bấm Kiểm tra để thử lại.`;
    return;
  }
  if (request !== builderRequest) return;
  if (attempt === correct) {
    fb.innerHTML = `<div style="padding:14px;background:rgba(104,211,145,0.1);border:1px solid var(--green);border-radius:var(--r2);color:var(--green);">✓ Chính xác! Xuất sắc! +10 XP</div>`;
    confetti();
  } else {
    fb.innerHTML = `<div style="padding:14px;background:rgba(252,129,129,0.08);border:1px solid var(--red);border-radius:var(--r2);">
      <div style="color:var(--red);margin-bottom:4px;">✗ Chưa đúng</div>
      <div style="font-size:13px;color:var(--text2);">Đáp án: <strong style="color:var(--accent);">${escapePracticeText(correct)}</strong></div>
    </div>`;
  }
};

window.initBuilder = initBuilder;
