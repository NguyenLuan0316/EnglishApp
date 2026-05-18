import { api } from '../infrastructure/api/apiClient.js';
import { badgeClass } from '../shared/utils.js';
import { initBuilder } from './pages/builder.js';
import { initFillBlank } from './pages/fillblank.js';
import { initFlashcard } from './pages/flashcard.js';
import { initIelts } from './pages/ielts.js';
import { filterGrammarByLevel } from './pages/grammar.js';
import { initMatching } from './pages/matching.js';
import { initQuiz } from './pages/quiz.js';
import { filterVocab, loadMoreVocab, setTopicFilter } from './pages/vocabulary.js';
import { initRouter, navigate, toggleSidebar } from './routing/router.js';

window.navigate = navigate;
window.toggleSidebar = toggleSidebar;
window.filterVocab = filterVocab;
window.setTopicFilter = setTopicFilter;
window.loadMoreVocab = loadMoreVocab;
window.filterGrammarByLevel = filterGrammarByLevel;
window.initFlashcard = initFlashcard;
window.initQuiz = initQuiz;
window.initMatching = initMatching;
window.initBuilder = initBuilder;
window.initFillBlank = initFillBlank;
window.initIelts = initIelts;

window.handleGlobalSearch = function(q) {
  if (q.length < 2) {
    document.getElementById('search-modal').classList.remove('show');
    return;
  }

  api.getVocab({ search: q, limit: 10 }).then(res => {
    const words = res.data || [];
    document.getElementById('search-results').innerHTML = words.length
      ? words.map(v => `
          <div class="word-card" style="margin-bottom:8px;">
            <div class="word-info">
              <div style="display:flex;align-items:center;gap:8px;">
                <span class="word-english">${v.word}</span>
                <span class="badge ${badgeClass(v.level)}">${v.level}</span>
              </div>
              <div class="word-phonetic">${v.phonetic}</div>
              <div class="word-meaning">${v.meaning}</div>
            </div>
            <button class="speak-btn" onclick="window._speak('${v.word.replace(/'/g,"\\'")}')">ðŸ”Š</button>
          </div>`).join('')
      : '<div style="color:var(--text3);text-align:center;padding:20px;">KhÃ´ng tÃ¬m tháº¥y tá»« nÃ o</div>';
    document.getElementById('search-modal').classList.add('show');
  }).catch(() => {});
};

document.querySelectorAll('.modal-overlay').forEach(modal =>
  modal.addEventListener('click', event => {
    if (event.target === modal) modal.classList.remove('show');
  })
);

initRouter();
