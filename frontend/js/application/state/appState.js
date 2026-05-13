// wordwave/frontend/js/state.js
export const state = {
  xp:      parseInt(localStorage.getItem('ww_xp')   || '0'),
  streak:  parseInt(localStorage.getItem('ww_streak')|| '0'),
  level:   localStorage.getItem('ww_level') || 'A1',

  addXP(n) {
    this.xp += n;
    localStorage.setItem('ww_xp', this.xp);
    document.getElementById('xp-display')?.textContent &&
      (document.getElementById('xp-display').textContent = this.xp);
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
