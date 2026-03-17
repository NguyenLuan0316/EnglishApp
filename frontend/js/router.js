// wordwave/frontend/js/router.js
import { initDashboard }  from './pages/dashboard.js';
import { initVocabulary } from './pages/vocabulary.js';
import { initGrammar }    from './pages/grammar.js';
import { initPatterns }   from './pages/patterns.js';
import { initFlashcard }  from './pages/flashcard.js';
import { initQuiz }       from './pages/quiz.js';
import { initMatching }   from './pages/matching.js';
import { initBuilder }    from './pages/builder.js';
import { initFillBlank }  from './pages/fillblank.js';
import { initListening }  from './pages/listening.js';
import { initReview }     from './pages/review.js';
import { initProgress }   from './pages/progress.js';

const PAGES = {
  dashboard:  { title: 'Dashboard',         init: initDashboard  },
  vocabulary: { title: 'Từ Vựng',            init: initVocabulary },
  grammar:    { title: 'Ngữ Pháp',           init: initGrammar    },
  patterns:   { title: 'Mẫu Câu',            init: initPatterns   },
  flashcard:  { title: 'Flashcard',          init: initFlashcard  },
  quiz:       { title: 'Trắc Nghiệm',        init: initQuiz       },
  matching:   { title: 'Nối Từ',             init: initMatching   },
  builder:    { title: 'Ghép Câu',           init: initBuilder    },
  fillblank:  { title: 'Điền Từ',            init: initFillBlank  },
  listening:  { title: 'Luyện Nghe',         init: initListening  },
  review:     { title: 'Ôn Tập Hôm Nay',     init: initReview     },
  progress:   { title: 'Thống Kê',           init: initProgress   },
};

export function navigate(page) {
  // Hide all pages
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  // Activate target
  const pageEl = document.getElementById('page-' + page);
  if (pageEl) { pageEl.classList.add('active'); pageEl.classList.add('fade-in'); }
  // Update title
  const titleEl = document.getElementById('page-title');
  if (titleEl) titleEl.textContent = PAGES[page]?.title || page;
  // Highlight nav
  document.querySelectorAll('.nav-item').forEach(n => {
    n.classList.toggle('active', n.dataset.page === page);
  });
  // Run init function
  PAGES[page]?.init();
  // Close mobile sidebar
  document.getElementById('sidebar')?.classList.remove('open');
}

export function toggleSidebar() {
  document.getElementById('sidebar')?.classList.toggle('open');
}
