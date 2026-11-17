// wordwave/frontend/js/state.js
export const state = {
  userId: null,
  xp: 0,
  streak: 0,
  level: 'A1',

  setUser(id) {
    this.userId = id;
    this.xp = 0;
    this.streak = 0;
    this.learningSummary = null;
    this.level = localStorage.getItem(`ww_${id}_level`) || 'A1';
  },

  learningSummary: null,

  applyLearningSummary(summary) {
    this.learningSummary = summary;
    this.xp = summary.totalXp;
    this.streak = summary.streak;
    for (const [id, value] of Object.entries({ 'xp-display': this.xp, 'xp-total': this.xp, 'streak-display': this.streak })) {
      const element = document.getElementById(id);
      if (element) element.textContent = value;
    }
    const remaining = document.getElementById('sidebar-daily-remaining');
    if (remaining) remaining.textContent = `${Math.max(0, summary.dailyTotal - summary.dailyCompleted)} bài còn lại hôm nay`;
    const bar = document.getElementById('sidebar-daily-progress');
    if (bar) bar.style.width = `${summary.dailyTotal ? Math.round(summary.dailyCompleted / summary.dailyTotal * 100) : 0}%`;
  },

  // Flashcard
  fcWords: [], fcIndex: 0, fcFlipped: false,

  // Quiz
  quizWords: [], quizIndex: 0, quizScore: 0, quizAnswered: false,

  // Matching
  matchPairs: [], matchSelected: null, matchSelectedSide: null, matchCorrect: 0,

  // Builder
  builderTarget: [], builderSentence: [], builderWords: [],

  // Fill blank
  fbQuestions: [], fbIndex: 0,

  // Listening
  listenWord: null, listenLevel: 'A1', listenCorrect: 0, listenWrong: 0,

  // Review
  reviewWords: [], reviewIndex: 0,
};

window.addEventListener('wordwave:learning-updated', event => state.applyLearningSummary(event.detail));
