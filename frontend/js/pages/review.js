// wordwave/frontend/js/pages/review.js
import { api }          from '../api.js';
import { state }        from '../state.js';
import { speak, toast, confetti } from '../utils.js';

export async function initReview() {
  try {
    const words = await api.getDaily();
    state.reviewWords = words;
    document.getElementById('review-remaining').textContent = words.length;
    document.getElementById('review-start-card').style.display = 'block';
    document.getElementById('review-area').style.display        = 'none';
  } catch(e) {
    toast('Không thể tải danh sách ôn tập.', 'error');
  }
}

window.startReview = function() {
  state.reviewIndex = 0;
  document.getElementById('review-start-card').style.display = 'none';
  document.getElementById('review-area').style.display        = 'block';
  renderReviewCard();
};

function renderReviewCard() {
  const w   = state.reviewWords[state.reviewIndex];
  const el  = document.getElementById('review-area');
  if (!w) {
    el.innerHTML = `
      <div class="card" style="text-align:center;padding:40px;">
        <div style="font-size:40px;margin-bottom:8px;">🎉</div>
        <div style="font-size:20px;font-weight:700;">Ôn tập hoàn thành!</div>
        <div style="font-size:14px;color:var(--text2);margin-top:4px;">Bạn đã ôn xong ${state.reviewWords.length} từ hôm nay</div>
      </div>`;
    confetti();
    return;
  }
  const pct = Math.round(state.reviewIndex / Math.max(state.reviewWords.length, 1) * 100);
  el.innerHTML = `
    <div style="margin-bottom:12px;">
      <div style="display:flex;justify-content:space-between;margin-bottom:4px;">
        <span style="font-size:13px;color:var(--text2);">${state.reviewIndex + 1} / ${state.reviewWords.length}</span>
        <span style="font-size:13px;color:var(--accent);">${pct}%</span>
      </div>
      <div class="progress-track" style="height:6px;"><div class="progress-fill" style="width:${pct}%;background:var(--accent);"></div></div>
    </div>
    <div class="card" style="text-align:center;padding:32px;margin-bottom:16px;">
      <div style="font-size:36px;font-weight:700;margin-bottom:6px;">${w.word}</div>
      <div style="font-size:15px;color:var(--accent2);font-family:var(--mono);margin-bottom:8px;">${w.phonetic}</div>
      <button class="btn btn-ghost btn-sm" onclick="window._speak('${w.word.replace(/'/g,"\\'")}')">🔊 Nghe</button>
    </div>
    <p style="text-align:center;font-size:13px;color:var(--text3);margin-bottom:12px;">Bạn có nhớ từ này không?</p>
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
      <button class="btn btn-danger"  onclick="reviewAnswer(false)">✗ Không nhớ</button>
      <button class="btn btn-success" onclick="reviewAnswer(true)">✓ Nhớ rồi</button>
    </div>`;
}

window.reviewAnswer = async function(correct) {
  const w = state.reviewWords[state.reviewIndex];
  if (correct) {
    toast(`✓ ${w.meaning}`, 'success');
    state.addXP(5);
  } else {
    toast(`✗ Nghĩa là: ${w.meaning}`, 'error');
  }
  try { await api.submitReview(w.id, correct); } catch(e) {}
  state.reviewIndex++;
  setTimeout(renderReviewCard, 800);
};
