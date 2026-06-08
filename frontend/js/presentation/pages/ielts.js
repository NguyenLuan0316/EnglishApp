import { api } from '../../infrastructure/api/apiClient.js';
import { confetti, toast } from '../../shared/utils.js';

const LEARNER_ID = 'default';
const MODULES = [
  { key: 'listening', title: 'Listening', minutes: 30, total: 40 },
  { key: 'reading', title: 'Reading', minutes: 60, total: 40 },
  { key: 'writing', title: 'Writing', minutes: 60, total: 2 },
  { key: 'speaking', title: 'Speaking', minutes: 14, total: 3 },
];
const TOTAL_ITEMS = MODULES.reduce((sum, item) => sum + item.total, 0);

let tests = [];
let currentTest = null;
let examState = null;
let resultData = null;
let timerId = null;
let recognition = null;
let saveTimer = null;

export async function initIelts() {
  ensureTimer();
  if (!currentTest) {
    await loadTests();
  }
  renderIelts();
}

async function loadTests() {
  try {
    tests = await api.getIeltsTests(LEARNER_ID);
  } catch {
    toast('Không thể tải danh sách đề IELTS từ backend.', 'error');
    tests = [];
  }
}

async function selectTest(testId) {
  try {
    const detail = await api.getIeltsTestById(testId, LEARNER_ID);
    currentTest = {
      id: detail.id,
      title: detail.title,
      description: detail.description,
      sourceType: detail.sourceType,
      sourceName: detail.sourceName,
      questionCount: detail.questionCount,
      createdAt: detail.createdAt,
      ...detail.testData,
    };
    examState = detail.attempt?.stateData || createFreshState(currentTest.id);
    resultData = detail.attempt?.resultData || null;
    if (detail.attempt?.isSubmitted) {
      examState.submitted = true;
      examState.results = resultData;
    }
    renderIelts();
  } catch {
    toast('Không thể tải đề IELTS.', 'error');
  }
}

async function backToList() {
  syncInputs();
  stopAudio();
  stopSpeech();
  currentTest = null;
  examState = null;
  resultData = null;
  await loadTests();
  renderIelts();
}

function createFreshState(testId) {
  return {
    testId,
    started: false,
    submitted: false,
    activeModule: 'listening',
    activePart: { listening: 0, reading: 0, writing: 0, speaking: 0 },
    remaining: MODULES.reduce((acc, item) => {
      acc[item.key] = item.minutes * 60;
      return acc;
    }, {}),
    answers: {},
    updatedAt: new Date().toISOString(),
  };
}

function ensureTimer() {
  if (timerId) return;
  timerId = window.setInterval(() => {
    if (!currentTest || !examState?.started || examState.submitted) return;
    const key = examState.activeModule;
    if ((examState.remaining[key] || 0) <= 0) return;

    examState.remaining[key] -= 1;
    examState.updatedAt = new Date().toISOString();
    updateTimerDisplay();
    updateProgressDisplay();
    queueSave();
    if (examState.remaining[key] === 0) {
      saveCurrentAttempt();
      toast(`${getModuleConfig(key).title} time is over.`, 'error');
    }
  }, 1000);
}

function renderIelts() {
  const root = document.getElementById('ielts-root');
  if (!root) return;

  if (!currentTest || !examState) {
    root.innerHTML = renderTestList();
  } else if (examState.submitted) {
    root.innerHTML = renderResults();
  } else if (examState.started) {
    root.innerHTML = renderExamShell();
  } else {
    root.innerHTML = renderTestOverview();
  }

  bindInputs();
  updateTimerDisplay();
  updateProgressDisplay();
}

function renderTestList() {
  return `
    <div class="ielts-library">
      <div class="ielts-library-header">
        <div>
          <div class="ielts-kicker">IELTS test bank</div>
          <h1>Danh sách đề thi IELTS</h1>
          <p>Đề thi và bài làm được lưu trong database qua API backend. Bạn có thể mở app ở máy khác và tiếp tục cùng dữ liệu.</p>
        </div>
        <div class="ielts-start-actions">
          <button class="btn btn-primary" onclick="ieltsReloadTests()">Tải lại danh sách</button>
        </div>
      </div>
      <div class="ielts-test-grid">
        ${tests.length ? tests.map(renderTestCard).join('') : '<div class="card">Chưa có đề IELTS trong database.</div>'}
      </div>
    </div>
  `;
}

function renderTestCard(test) {
  const attempt = test.attempt;
  const status = attempt?.isSubmitted
    ? `Band ${formatBand(attempt.overallBand)}`
    : attempt?.started
      ? `${attempt.answeredCount}/${TOTAL_ITEMS} answered`
      : 'Chưa làm';

  return `
    <div class="ielts-test-card">
      <div class="ielts-card-label">${escapeHtml(test.sourceName || test.sourceType || 'WordWave')}</div>
      <h2>${escapeHtml(test.title)}</h2>
      <p>${escapeHtml(test.description)}</p>
      <div class="ielts-test-meta">
        <span>${formatDate(test.createdAt)}</span>
        <span>${status}</span>
      </div>
      <div class="ielts-test-actions">
        <button class="btn btn-primary" onclick="ieltsSelectTest(${test.id})">${attempt?.isSubmitted ? 'Xem điểm' : attempt?.started ? 'Tiếp tục' : 'Làm đề'}</button>
      </div>
    </div>
  `;
}

function renderTestOverview() {
  const progress = countAnsweredForTest();

  return `
    <div class="ielts-start">
      <div class="ielts-start-main">
        <div class="ielts-kicker">Academic mock test</div>
        <h1>${escapeHtml(currentTest.title)}</h1>
        <p>${escapeHtml(currentTest.description)} Đề có đủ Listening, Reading, Writing và Speaking với timer, lưu nháp và bảng điểm sau khi nộp.</p>
        <div class="ielts-test-meta" style="margin-top:14px;">
          <span>${progress}/${TOTAL_ITEMS} answered</span>
          <span>Created ${formatDate(currentTest.createdAt)}</span>
        </div>
        <div class="ielts-start-actions">
          <button class="btn btn-primary btn-lg" onclick="ieltsStartExam()">${examState.started ? 'Tiếp tục làm bài' : 'Bắt đầu làm đề'}</button>
          <button class="btn btn-ghost btn-lg" onclick="ieltsResetExam()">Làm lại đề này</button>
          <button class="btn btn-secondary btn-lg" onclick="ieltsBackToList()">Danh sách đề</button>
        </div>
      </div>
      <div class="ielts-start-grid">
        ${MODULES.map(item => `
          <div class="ielts-start-card">
            <div class="ielts-card-label">${item.title}</div>
            <div class="ielts-card-value">${item.minutes} phút</div>
            <div class="ielts-card-meta">${countAnsweredForModule(item.key)}/${item.total} ${item.key === 'writing' ? 'tasks' : item.key === 'speaking' ? 'parts' : 'questions'}</div>
          </div>
        `).join('')}
      </div>
    </div>
  `;
}

function renderExamShell() {
  const activeConfig = getModuleConfig(examState.activeModule);

  return `
    <div class="ielts-exam">
      <div class="ielts-exam-header">
        <div>
          <div class="ielts-kicker">${escapeHtml(currentTest.title)}</div>
          <h1>${activeConfig.title}</h1>
        </div>
        <div class="ielts-header-actions">
          <div class="ielts-timer" id="ielts-timer">--:--</div>
          <button class="btn btn-secondary" onclick="ieltsSaveDraft()">Lưu nháp</button>
          <button class="btn btn-ghost" onclick="ieltsBackToList()">Danh sách đề</button>
          <button class="btn btn-primary" onclick="ieltsSubmitExam()">Nộp bài</button>
        </div>
      </div>
      <div class="ielts-progress-line">
        <div class="progress-track">
          <div class="progress-fill" id="ielts-total-progress" style="width:0%;background:linear-gradient(90deg,var(--accent),var(--accent2));"></div>
        </div>
        <span id="ielts-progress-text">0/${TOTAL_ITEMS} answered</span>
      </div>
      <div class="ielts-module-tabs">
        ${MODULES.map(item => `
          <button class="ielts-module-tab ${examState.activeModule === item.key ? 'active' : ''}" onclick="ieltsSetModule('${item.key}')">
            <span>${item.title}</span>
            <small>${countAnsweredForModule(item.key)}/${item.total}</small>
          </button>
        `).join('')}
      </div>
      <div class="ielts-workspace">
        ${renderPartNav()}
        <section class="ielts-panel">
          ${renderActiveModule()}
        </section>
      </div>
    </div>
  `;
}

function renderPartNav() {
  const key = examState.activeModule;
  const parts = getPartsForModule(key);

  return `
    <aside class="ielts-part-nav">
      <div class="ielts-part-title">Phần thi</div>
      ${parts.map((part, index) => `
        <button class="ielts-part-btn ${examState.activePart[key] === index ? 'active' : ''}" onclick="ieltsSetPart(${index})">
          <span>${escapeHtml(part.title || part.id)}</span>
          <small>${partProgressText(key, index)}</small>
        </button>
      `).join('')}
    </aside>
  `;
}

function renderActiveModule() {
  if (examState.activeModule === 'listening') return renderListening();
  if (examState.activeModule === 'reading') return renderReading();
  if (examState.activeModule === 'writing') return renderWriting();
  return renderSpeaking();
}

function renderListening() {
  const partIndex = examState.activePart.listening;
  const part = currentTest.listening.parts[partIndex];

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${escapeHtml(part.title)}</h2>
        <p>${escapeHtml(part.instruction)}</p>
      </div>
      <div class="ielts-audio-actions">
        <button class="btn btn-secondary" onclick="ieltsPlayListening(${partIndex})">Play audio</button>
        <button class="btn btn-ghost" onclick="ieltsStopAudio()">Stop</button>
      </div>
    </div>
    <div class="ielts-question-list">
      ${part.questions.map(renderQuestion).join('')}
    </div>
    ${renderPartPager('listening')}
  `;
}

function renderReading() {
  const partIndex = examState.activePart.reading;
  const part = currentTest.reading.parts[partIndex];

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${escapeHtml(part.title)}</h2>
        <p>${escapeHtml(part.instruction)}</p>
      </div>
    </div>
    <div class="ielts-reading-grid">
      <article class="ielts-passage">
        ${(part.passage || []).map(paragraph => `<p>${escapeHtml(paragraph)}</p>`).join('')}
      </article>
      <div class="ielts-question-list">
        ${part.questions.map(renderQuestion).join('')}
      </div>
    </div>
    ${renderPartPager('reading')}
  `;
}

function renderWriting() {
  const taskIndex = examState.activePart.writing;
  const task = currentTest.writing.tasks[taskIndex];
  const answer = getAnswer(task.id);

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${escapeHtml(task.title)}</h2>
        <p>${task.minutes} minutes. Minimum ${task.minWords} words.</p>
      </div>
    </div>
    <div class="ielts-writing-task">
      <div class="ielts-writing-prompt">${escapeHtml(task.prompt)}</div>
      <textarea class="ielts-textarea" data-ielts-answer="${task.id}" rows="16" placeholder="Write your answer here...">${escapeHtml(answer)}</textarea>
      <div class="ielts-word-count" id="ielts-count-${task.id}">0 words</div>
    </div>
    ${renderPartPager('writing')}
  `;
}

function renderSpeaking() {
  const partIndex = examState.activePart.speaking;
  const part = currentTest.speaking.parts[partIndex];
  const answer = getAnswer(part.id);

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${escapeHtml(part.title)}</h2>
        <p>${escapeHtml(part.time)}</p>
      </div>
      <div class="ielts-audio-actions">
        <button class="btn btn-secondary" onclick="ieltsStartSpeech('${part.id}')">Start speech</button>
        <button class="btn btn-ghost" onclick="ieltsStopSpeech()">Stop</button>
      </div>
    </div>
    <div class="ielts-speaking-prompts">
      ${(part.prompts || []).map(prompt => `<div class="ielts-prompt-line">${escapeHtml(prompt)}</div>`).join('')}
    </div>
    <textarea class="ielts-textarea" data-ielts-answer="${part.id}" rows="14" placeholder="Your spoken transcript or notes will appear here...">${escapeHtml(answer)}</textarea>
    <div class="ielts-word-count" id="ielts-count-${part.id}">0 words</div>
    ${renderPartPager('speaking')}
  `;
}

function renderQuestion(question) {
  const answer = getAnswer(question.id);

  if (question.type === 'choice') {
    return `
      <label class="ielts-question">
        <span class="ielts-q-number">${question.number}</span>
        <span class="ielts-q-body">
          <span class="ielts-q-prompt">${escapeHtml(question.prompt)}</span>
          <select class="ielts-select" data-ielts-answer="${question.id}">
            <option value="">Choose answer</option>
            ${(question.options || []).map(option => `
              <option value="${escapeAttr(option)}" ${answer === option ? 'selected' : ''}>${escapeHtml(option)}</option>
            `).join('')}
          </select>
        </span>
      </label>
    `;
  }

  return `
    <label class="ielts-question">
      <span class="ielts-q-number">${question.number}</span>
      <span class="ielts-q-body">
        <span class="ielts-q-prompt">${escapeHtml(question.prompt)}</span>
        <input class="ielts-input" data-ielts-answer="${question.id}" value="${escapeAttr(answer)}" autocomplete="off"/>
      </span>
    </label>
  `;
}

function renderPartPager(moduleKey) {
  const current = examState.activePart[moduleKey];
  const total = getPartsForModule(moduleKey).length;
  const previousDisabled = current === 0 ? 'disabled' : '';
  const nextDisabled = current >= total - 1 ? 'disabled' : '';

  return `
    <div class="ielts-part-pager">
      <button class="btn btn-ghost" ${previousDisabled} onclick="ieltsSetPart(${current - 1})">Previous</button>
      <button class="btn btn-secondary" ${nextDisabled} onclick="ieltsSetPart(${current + 1})">Next</button>
    </div>
  `;
}

function bindInputs() {
  document.querySelectorAll('[data-ielts-answer]').forEach(input => {
    const id = input.dataset.ieltsAnswer;
    const eventName = input.tagName === 'SELECT' ? 'change' : 'input';
    input.addEventListener(eventName, () => {
      examState.answers[id] = input.value;
      examState.updatedAt = new Date().toISOString();
      queueSave();
      updateProgressDisplay();
      updateWordCounter(id, input.value);
    });
    updateWordCounter(id, input.value);
  });
}

function startExam() {
  if (!currentTest || !examState) return;
  examState.started = true;
  examState.submitted = false;
  examState.results = null;
  examState.updatedAt = new Date().toISOString();
  saveCurrentAttempt();
  renderIelts();
}

function resetExam() {
  if (!currentTest) return;
  stopAudio();
  stopSpeech();
  examState = createFreshState(currentTest.id);
  resultData = null;
  saveCurrentAttempt();
  renderIelts();
}

async function saveDraft() {
  syncInputs();
  await saveCurrentAttempt();
  toast('Đã lưu nháp IELTS lên backend.', 'success');
}

async function submitExam() {
  if (!currentTest || !examState) return;
  syncInputs();
  examState.submitted = true;
  examState.updatedAt = new Date().toISOString();

  try {
    const attempt = await api.submitIeltsTest(currentTest.id, LEARNER_ID, examState);
    resultData = attempt.resultData;
    examState.results = resultData;
    examState.submitted = true;
    renderIelts();
    if ((resultData?.overallBand || 0) >= 7) confetti();
  } catch {
    examState.submitted = false;
    toast('Không thể nộp bài IELTS.', 'error');
  }
}

function setModule(key) {
  if (!MODULES.some(item => item.key === key)) return;
  syncInputs();
  stopAudio();
  examState.activeModule = key;
  queueSave();
  renderIelts();
}

function setPart(index) {
  const key = examState.activeModule;
  const total = getPartsForModule(key).length;
  if (index < 0 || index >= total) return;
  syncInputs();
  stopAudio();
  examState.activePart[key] = index;
  queueSave();
  renderIelts();
}

function playListening(partIndex) {
  const part = currentTest?.listening.parts[partIndex];
  if (!part || !('speechSynthesis' in window)) return;
  stopAudio();

  const utterance = new SpeechSynthesisUtterance(part.transcript || '');
  utterance.lang = 'en-US';
  utterance.rate = 0.86;
  utterance.pitch = 1;
  speechSynthesis.speak(utterance);
}

function stopAudio() {
  if ('speechSynthesis' in window) {
    speechSynthesis.cancel();
  }
}

function startSpeech(targetId) {
  const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (!SpeechRecognition) {
    toast('Trình duyệt chưa hỗ trợ speech recognition. Bạn có thể nhập transcript thủ công.', 'error');
    return;
  }

  stopSpeech();
  recognition = new SpeechRecognition();
  recognition.lang = 'en-US';
  recognition.continuous = true;
  recognition.interimResults = true;

  recognition.onresult = event => {
    let finalText = getAnswer(targetId);
    let interimText = '';
    for (let i = event.resultIndex; i < event.results.length; i++) {
      const chunk = event.results[i][0].transcript;
      if (event.results[i].isFinal) {
        finalText = `${finalText} ${chunk}`.trim();
      } else {
        interimText = chunk;
      }
    }

    const nextValue = `${finalText}${interimText ? ` ${interimText}` : ''}`.trim();
    const field = document.querySelector(`[data-ielts-answer="${targetId}"]`);
    if (field) field.value = nextValue;
    examState.answers[targetId] = nextValue;
    updateWordCounter(targetId, nextValue);
    queueSave();
  };

  recognition.onerror = () => {
    toast('Không thể ghi nhận giọng nói lúc này.', 'error');
  };

  recognition.start();
  toast('Đang ghi speaking...', 'success');
}

function stopSpeech() {
  if (recognition) {
    recognition.stop();
    recognition = null;
  }
}

function syncInputs() {
  if (!examState) return;
  document.querySelectorAll('[data-ielts-answer]').forEach(input => {
    examState.answers[input.dataset.ieltsAnswer] = input.value;
  });
}

function queueSave() {
  clearTimeout(saveTimer);
  saveTimer = setTimeout(saveCurrentAttempt, 500);
}

async function saveCurrentAttempt() {
  if (!currentTest || !examState || examState.submitted) return;
  try {
    await api.saveIeltsAttempt(currentTest.id, LEARNER_ID, examState);
  } catch {
    toast('Không thể lưu nháp IELTS lên backend.', 'error');
  }
}

function renderResults() {
  const results = resultData || examState.results || {};

  return `
    <div class="ielts-results">
      <div class="ielts-result-hero">
        <div>
          <div class="ielts-kicker">${escapeHtml(currentTest.title)}</div>
          <h1>Overall Band ${formatBand(results.overallBand)}</h1>
          <p>Listening và Reading được chấm theo đáp án trên backend. Writing và Speaking là điểm ước tính dựa trên độ dài, mạch lạc, từ vựng và dấu hiệu bám đề.</p>
        </div>
        <div class="ielts-overall-band">${formatBand(results.overallBand)}</div>
      </div>
      <div class="ielts-result-grid">
        ${renderBandCard('Listening', results.listening?.band, `${results.listening?.correct || 0}/${results.listening?.total || 40} correct`)}
        ${renderBandCard('Reading', results.reading?.band, `${results.reading?.correct || 0}/${results.reading?.total || 40} correct`)}
        ${renderBandCard('Writing', results.writing?.band, `Task 1: ${formatBand(results.writing?.task1?.band)} | Task 2: ${formatBand(results.writing?.task2?.band)}`)}
        ${renderBandCard('Speaking', results.speaking?.band, `${results.speaking?.wordCount || 0} words captured`)}
      </div>
      <div class="ielts-result-actions">
        <button class="btn btn-secondary" onclick="ieltsBackToExam()">Xem lại bài làm</button>
        <button class="btn btn-ghost" onclick="ieltsResetExam()">Làm lại đề này</button>
        <button class="btn btn-secondary" onclick="ieltsBackToList()">Danh sách đề</button>
      </div>
      <div class="ielts-feedback-grid">
        ${renderProductionFeedback('Writing Task 1', results.writing?.task1)}
        ${renderProductionFeedback('Writing Task 2', results.writing?.task2)}
        ${renderProductionFeedback('Speaking', results.speaking)}
      </div>
      ${renderObjectiveReview('Listening', results.listening)}
      ${renderObjectiveReview('Reading', results.reading)}
    </div>
  `;
}

function renderBandCard(title, band, meta) {
  return `
    <div class="ielts-band-card">
      <div class="ielts-card-label">${title}</div>
      <div class="ielts-card-value">${formatBand(band)}</div>
      <div class="ielts-card-meta">${escapeHtml(meta)}</div>
    </div>
  `;
}

function renderProductionFeedback(title, score = {}) {
  const criteria = score.criteria || {};
  const feedback = score.feedback || ['No feedback available.'];
  return `
    <div class="ielts-feedback-card">
      <div class="section-title">${title}</div>
      <div class="ielts-criteria">
        <span>Task ${formatBand(criteria.task)}</span>
        <span>Coherence ${formatBand(criteria.coherence)}</span>
        <span>Lexical ${formatBand(criteria.lexical)}</span>
        <span>Grammar ${formatBand(criteria.grammar)}</span>
      </div>
      <div class="ielts-card-meta">${score.wordCount || 0} words</div>
      <ul>
        ${feedback.map(item => `<li>${escapeHtml(item)}</li>`).join('')}
      </ul>
    </div>
  `;
}

function renderObjectiveReview(title, result = {}) {
  const items = result.items || [];
  return `
    <details class="ielts-review-block">
      <summary>${title} answer review (${result.correct || 0}/${result.total || 40})</summary>
      <div class="ielts-review-table">
        ${items.map(item => `
          <div class="ielts-review-row ${item.isCorrect ? 'correct' : 'wrong'}">
            <span>${item.number}</span>
            <span>${escapeHtml(item.submitted || '-')}</span>
            <span>${escapeHtml(item.correctAnswer || '-')}</span>
          </div>
        `).join('')}
      </div>
    </details>
  `;
}

function backToExam() {
  examState.submitted = false;
  renderIelts();
}

function updateTimerDisplay() {
  const timer = document.getElementById('ielts-timer');
  if (!timer || !examState) return;
  const seconds = examState.remaining[examState.activeModule] || 0;
  timer.textContent = formatTime(seconds);
  timer.classList.toggle('warning', seconds <= 300);
}

function updateProgressDisplay() {
  if (!currentTest || !examState) return;
  const answered = countAnsweredForTest();
  const percent = TOTAL_ITEMS ? Math.round(answered * 100 / TOTAL_ITEMS) : 0;
  const bar = document.getElementById('ielts-total-progress');
  const textNode = document.getElementById('ielts-progress-text');
  if (bar) bar.style.width = `${percent}%`;
  if (textNode) textNode.textContent = `${answered}/${TOTAL_ITEMS} answered`;
}

function updateWordCounter(id, value) {
  const counter = document.getElementById(`ielts-count-${id}`);
  if (!counter) return;
  counter.textContent = `${tokenize(value).length} words`;
}

function partProgressText(moduleKey, index) {
  const part = getPartsForModule(moduleKey)[index];
  const ids = partQuestionIds(moduleKey, part);
  const answered = ids.filter(id => getAnswer(id).trim()).length;
  return `${answered}/${ids.length}`;
}

function countAnsweredForTest() {
  return MODULES.reduce((sum, item) => sum + countAnsweredForModule(item.key), 0);
}

function countAnsweredForModule(moduleKey) {
  if (moduleKey === 'listening' || moduleKey === 'reading') {
    return getObjectiveQuestions(moduleKey).filter(question => getAnswer(question.id).trim()).length;
  }
  if (moduleKey === 'writing') {
    return currentTest.writing.tasks.filter(task => getAnswer(task.id).trim()).length;
  }
  return currentTest.speaking.parts.filter(part => getAnswer(part.id).trim()).length;
}

function partQuestionIds(moduleKey, part) {
  if (moduleKey === 'writing') return [part.id];
  if (moduleKey === 'speaking') return [part.id];
  return part.questions.map(question => question.id);
}

function getPartsForModule(moduleKey) {
  if (moduleKey === 'listening') return currentTest.listening.parts;
  if (moduleKey === 'reading') return currentTest.reading.parts;
  if (moduleKey === 'writing') return currentTest.writing.tasks;
  return currentTest.speaking.parts;
}

function getObjectiveQuestions(moduleKey) {
  const source = moduleKey === 'listening' ? currentTest.listening.parts : currentTest.reading.parts;
  return source.flatMap(part => part.questions);
}

function getModuleConfig(moduleKey) {
  return MODULES.find(item => item.key === moduleKey) || MODULES[0];
}

function getAnswer(id) {
  return String(examState?.answers?.[id] || '');
}

function tokenize(value) {
  return String(value || '').toLowerCase().match(/[a-z]+(?:'[a-z]+)?|[0-9]+/g) || [];
}

function formatBand(value) {
  return Number(value || 0).toFixed(1);
}

function formatTime(totalSeconds) {
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
}

function formatDate(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  return date.toLocaleDateString('vi-VN');
}

function escapeHtml(value) {
  return String(value || '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

function escapeAttr(value) {
  return escapeHtml(value).replace(/`/g, '&#096;');
}

window.ieltsReloadTests = async function() {
  await loadTests();
  renderIelts();
};
window.ieltsSelectTest = selectTest;
window.ieltsBackToList = backToList;
window.ieltsStartExam = startExam;
window.ieltsResetExam = resetExam;
window.ieltsSaveDraft = saveDraft;
window.ieltsSubmitExam = submitExam;
window.ieltsSetModule = setModule;
window.ieltsSetPart = setPart;
window.ieltsPlayListening = playListening;
window.ieltsStopAudio = stopAudio;
window.ieltsStartSpeech = startSpeech;
window.ieltsStopSpeech = stopSpeech;
window.ieltsBackToExam = backToExam;
window.initIelts = initIelts;

window.addEventListener('beforeunload', () => {
  syncInputs();
  saveCurrentAttempt();
});
