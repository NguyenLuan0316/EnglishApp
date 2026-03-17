// wordwave/frontend/js/pages/flashcard.js
import { api }                      from '../api.js';
import { state }                    from '../state.js';
import { shuffle, speak, toast, confetti } from '../utils.js';

export async function initFlashcard() {
  const level = document.getElementById('fc-level')?.value || '';
  try {
    const words = await api.getRandom({ count: 10, ...(level && { level }) });
    state.fcWords   = words;
    state.fcIndex   = 0;
    state.fcFlipped = false;
    showCard();
  } catch(e) {
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

window.rateCard = function(rating) {
  state.addXP(rating * 2);
  document.getElementById('fc-actions').style.display  = 'none';
  document.getElementById('fc-next-btn').style.display = 'block';
  if (rating >= 3) toast(`✓ Tốt! +${rating * 2} XP`, 'success');
  // Ghi nhận spaced repetition
  const w = state.fcWords[state.fcIndex];
  if (w) api.submitReview(w.id, rating >= 3).catch(() => {});
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
