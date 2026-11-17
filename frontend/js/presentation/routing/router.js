// wordwave/frontend/js/router.js
import { initDashboard }  from '../pages/dashboard.js';
import { initVocabulary } from '../pages/vocabulary.js';
import { initGrammar }    from '../pages/grammar.js';
import { initPatterns }   from '../pages/patterns.js';
import { initFlashcard }  from '../pages/flashcard.js';
import { initQuiz }       from '../pages/quiz.js';
import { initMatching }   from '../pages/matching.js';
import { initBuilder }    from '../pages/builder.js';
import { initFillBlank }  from '../pages/fillblank.js';
import { initListening }  from '../pages/listening.js';
import { initIelts }      from '../pages/ielts.js';
import { initReview }     from '../pages/review.js';
import { initProgress }   from '../pages/progress.js';
import { initToeicAdmin } from '../pages/toeicAdmin.js';
import { initLogin, initRegister, initAccount } from '../pages/auth.js';
import { initUsersAdmin } from '../pages/usersAdmin.js';
import { authState } from '../../application/state/authState.js';

const PAGES = {
  login: { title: 'Đăng nhập', init: initLogin, anonymous: true },
  register: { title: 'Tạo tài khoản', init: initRegister, anonymous: true },
  account: { title: 'Tài khoản của tôi', init: initAccount },
  usersAdmin: { title: 'Quản lý tài khoản', init: initUsersAdmin, admin: true },
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
  ielts:      { title: 'IELTS Mock Test',     init: initIelts      },
  review:     { title: 'Ôn Tập Hôm Nay',     init: initReview     },
  progress:   { title: 'Thống Kê',           init: initProgress   },
  toeicAdmin: { title: 'TOEIC Admin',       init: initToeicAdmin, admin: true },
};

const ROUTES = {
  '/login': 'login',
  '/register': 'register',
  '/account': 'account',
  '/admin/users': 'usersAdmin',
  '/': 'dashboard',
  '/home': 'dashboard',
  '/dashboard': 'dashboard',
  '/vocab': 'vocabulary',
  '/vocabulary': 'vocabulary',
  '/grammar': 'grammar',
  '/speaking': 'patterns',
  '/patterns': 'patterns',
  '/practice': 'flashcard',
  '/flashcard': 'flashcard',
  '/quiz': 'quiz',
  '/matching': 'matching',
  '/writing': 'builder',
  '/builder': 'builder',
  '/reading': 'fillblank',
  '/fillblank': 'fillblank',
  '/listening': 'listening',
  '/ielts': 'ielts',
  '/ielts-test': 'ielts',
  '/review': 'review',
  '/roadmap': 'progress',
  '/progress': 'progress',
  '/admin/toeic': 'toeicAdmin',
  '/toeic-admin': 'toeicAdmin',
};

const PAGE_PATHS = {
  login: '/login',
  register: '/register',
  account: '/account',
  usersAdmin: '/admin/users',
  dashboard: '/',
  vocabulary: '/vocab',
  grammar: '/grammar',
  patterns: '/speaking',
  flashcard: '/practice',
  quiz: '/quiz',
  matching: '/matching',
  builder: '/writing',
  fillblank: '/reading',
  listening: '/listening',
  ielts: '/ielts',
  review: '/review',
  progress: '/roadmap',
  toeicAdmin: '/admin/toeic',
};

function normalizePath(pathname = window.location.pathname) {
  let path = pathname.startsWith('/') ? pathname : `/${pathname}`;
  path = path.replace(/\/+$/, '') || '/';

  if (path.endsWith('/index.html')) {
    path = path.slice(0, -'/index.html'.length) || '/';
  }

  const parts = path.split('/').filter(Boolean);
  if (parts[0] === 'frontend') {
    path = '/' + parts.slice(1).join('/');
  }

  return path || '/';
}

function pageFromPath(pathname) {
  return ROUTES[normalizePath(pathname)];
}

function pageFromLocation() {
  return pageFromPath(window.location.pathname) || 'dashboard';
}

function pagePath(page) {
  return PAGE_PATHS[page] || '/';
}

function renderPage(page) {
  if (!authState.user && !PAGES[page]?.anonymous) {
    sessionStorage.setItem('ww_returnTo', pagePath(page));
    navigate('login', { replace: true });
    return false;
  }
  if (authState.user && PAGES[page]?.anonymous) {
    navigate('dashboard', { replace: true });
    return false;
  }
  if (PAGES[page]?.admin && authState.user?.role !== 'Admin') {
    navigate('dashboard', { replace: true });
    return false;
  }
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
  return true;
}

export function navigate(page, options = {}) {
  const target = PAGES[page] ? page : pageFromPath(page) || pageFromLocation();
  if (!renderPage(target)) return;

  if (options.skipHistory) return;

  const path = pagePath(target);
  if (normalizePath() === path) {
    history.replaceState({ page: target }, '', window.location.href);
    return;
  }

  const method = options.replace ? 'replaceState' : 'pushState';
  history[method]({ page: target }, '', path);
}

export function initRouter() {
  const page = pageFromLocation();
  if (renderPage(page)) history.replaceState({ page }, '', window.location.href);
  window.addEventListener('popstate', () => {
    renderPage(pageFromLocation());
  });
}

export function toggleSidebar() {
  document.getElementById('sidebar')?.classList.toggle('open');
}
