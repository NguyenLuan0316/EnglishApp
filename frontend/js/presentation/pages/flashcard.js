// wordwave/frontend/js/pages/flashcard.js
import { state } from '../../application/state/appState.js';
import { api } from '../../infrastructure/api/apiClient.js';
import { shuffle, speak, toast, confetti } from '../../shared/utils.js';

let flashcardRequest = 0;

export async function initFlashcard() {
  const request = ++flashcardRequest;
  cardSaving = false;
  cardRated = false;
  state.fcWords = [];
  document.getElementById('fc-word').textContent = 'Đang tải…';
  document.getElementById('fc-meaning').textContent = '';
  document.getElementById('fc-example').textContent = '';
  document.getElementById('fc-phonetic').textContent = '';
  document.getElementById('fc-actions').style.display = 'none';
  document.getElementById('fc-next-btn').style.display = 'none';
  const level = document.getElementById('fc-level')?.value || '';
  try {
    const words = await api.getRandom({ count: 10, ...(level && { level }) });
    if (request !== flashcardRequest) return;
    if (!words.length) {
      document.getElementById('fc-word').textContent = 'Chưa có từ ở độ khó này';
      return;
    }
    state.fcWords   = words;
    state.fcIndex   = 0;
    state.fcFlipped = false;
    showCard();
  } catch(e) {
    if (request !== flashcardRequest) return;
    document.getElementById('fc-word').textContent = 'Không thể tải từ. Chọn độ khó để thử lại.';
    toast('Không thể tải từ vựng.', 'error');
  }
}

function showCard() {
  const w = state.fcWords[state.fcIndex];
  if (!w) return;
  document.getElementById('fc-word').textContent     = w.word;
  document.getElementById('fc-phonetic').textContent = w.phonetic;
  document.getElementById('fc-meaning').textContent  = w.meaning;
  document.getElementById('fc-example').textContent  = `"${w.example}"`;
  document.getElementById('flashcard')?.classList.remove('flipped');
  document.getElementById('fc-actions').style.display  = 'none';
  document.getElementById('fc-next-btn').style.display = 'none';
  state.fcFlipped = false;
  cardRated = false;

  const total = state.fcWords.length;
  const pct   = Math.round((state.fcIndex + 1) / total * 100);
  document.getElementById('fc-count').textContent        = `Thẻ ${state.fcIndex + 1}/${total}`;
  document.getElementById('fc-progress').style.width     = pct + '%';
}

window.flipCard = function() {
  document.getElementById('flashcard')?.classList.toggle('flipped');
  state.fcFlipped = !state.fcFlipped;
  document.getElementById('fc-actions').style.display = state.fcFlipped ? 'block' : 'none';
};

let cardSaving = false;
let cardRated = false;

window.rateCard = async function(rating) {
  if (cardSaving || cardRated) return;
  const w = state.fcWords[state.fcIndex];
  if (!w) return;
  const request = flashcardRequest;
  cardSaving = true;
  try { await api.submitReview(w.id, rating >= 3); } catch {
    if (request !== flashcardRequest) return;
    toast('Chưa lưu được tiến độ. Bấm đánh giá để thử lại.', 'error');
    cardSaving = false;
    return;
  }
  if (request !== flashcardRequest) return;
  cardSaving = false;
  cardRated = true;
  document.getElementById('fc-actions').style.display  = 'none';
  document.getElementById('fc-next-btn').style.display = 'block';
  if (rating >= 3) toast('✓ Tốt! +10 XP', 'success');
};

window.nextFlashCard = function() {
  state.fcIndex++;
  if (state.fcIndex >= state.fcWords.length) {
    toast('🎉 Hoàn thành! Bạn đã học xong 10 thẻ.', 'success');
    confetti();
    setTimeout(initFlashcard, 2000);
    return;
  }
  showCard();
};

window.speakFC = function() {
  speak(document.getElementById('fc-word')?.textContent || '');
};
