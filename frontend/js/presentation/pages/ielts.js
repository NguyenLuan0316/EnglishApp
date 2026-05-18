import { confetti, toast } from '../../shared/utils.js';

const STORAGE_KEY = 'ww_ielts_mock_test_state_v1';

const MODULES = [
  { key: 'listening', title: 'Listening', minutes: 30, total: 40 },
  { key: 'reading', title: 'Reading', minutes: 60, total: 40 },
  { key: 'writing', title: 'Writing', minutes: 60, total: 2 },
  { key: 'speaking', title: 'Speaking', minutes: 14, total: 3 },
];

const choice = (id, number, prompt, options, answer) => ({
  id,
  number,
  prompt,
  options,
  answer,
  type: 'choice',
});

const text = (id, number, prompt, answer) => ({
  id,
  number,
  prompt,
  answer,
  type: 'text',
});

const IELTS_TEST = {
  title: 'IELTS Academic Mock Test 1',
  listening: {
    parts: [
      {
        title: 'Part 1: Community Centre Course Booking',
        instruction: 'Questions 1-10. Complete the notes with no more than two words and/or a number.',
        transcript:
          'You will hear a conversation at a community centre. Receptionist: Good morning, North Park Community Centre. How can I help? Caller: Hello, I would like to book a place on the evening photography course. Receptionist: Certainly. Can I take your full name? Caller: Emma Clarke. Receptionist: Thank you. Do you already have a membership number? Caller: Yes, it is C47291. Receptionist: The course begins next Tuesday, and it runs from six thirty to eight. Caller: That suits me. Is it for beginners? Receptionist: This group is intermediate, so it is ideal if you already know the basics. The class meets in Studio 2. Caller: Who teaches it? Receptionist: Marco, one of our freelance photographers. Caller: What is the fee? Receptionist: Eighty-five pounds for six weeks. Please bring a water bottle because the studio gets warm. Full details are on our website, northpark.org.',
        questions: [
          text('L1', 1, 'Full name:', 'Emma Clarke'),
          text('L2', 2, 'Membership number:', 'C47291'),
          text('L3', 3, 'Course begins on:', 'Tuesday'),
          text('L4', 4, 'Time:', ['6.30', '6:30', 'six thirty']),
          text('L5', 5, 'Level:', 'intermediate'),
          text('L6', 6, 'Room:', 'Studio 2'),
          text('L7', 7, 'Teacher:', 'Marco'),
          text('L8', 8, 'Fee:', ['85', 'eighty five', 'eighty-five']),
          text('L9', 9, 'Students should bring a:', 'water bottle'),
          text('L10', 10, 'Website:', 'northpark.org'),
        ],
      },
      {
        title: 'Part 2: Library Orientation',
        instruction: 'Questions 11-20. Answer the questions using no more than three words and/or a number.',
        transcript:
          'Welcome to Greenford Library. If you need help by phone, the quickest extension is 204. The science collection is on the second floor. On Sundays, the library opens at 10 am. Group study rooms should be booked online because the desk gets very busy. In the media room you can borrow headsets. Last month the magazines were moved nearer the cafe. The quietest desks are in the north wing. If you need help using academic databases, please go to the research desk. This Friday we are running a workshop on referencing. Please complete the membership form by Friday if you want access to the evening service.',
        questions: [
          text('L11', 11, 'Phone extension for quick help:', '204'),
          text('L12', 12, 'Science collection location:', 'second floor'),
          text('L13', 13, 'Sunday opening time:', ['10 am', '10:00 am', '10']),
          choice('L14', 14, 'Group study rooms should be booked:', ['at the front desk', 'online', 'by email'], 'online'),
          text('L15', 15, 'Equipment available in the media room:', 'headsets'),
          text('L16', 16, 'Recently moved items:', 'magazines'),
          text('L17', 17, 'Quietest area:', 'north wing'),
          text('L18', 18, 'Place for database help:', 'research desk'),
          text('L19', 19, 'Workshop topic:', 'referencing'),
          text('L20', 20, 'Membership form deadline:', 'Friday'),
        ],
      },
      {
        title: 'Part 3: Student Project Discussion',
        instruction: 'Questions 21-30. Complete the notes.',
        transcript:
          'Two students are planning a research project. They decide that their topic will be urban gardens rather than public parks. They changed their survey method because the first version had a low response rate. Their sample group will be commuters, since they use the central square every day. For analysis, they will use a spreadsheet. The consultant meeting has moved to Wednesday. They cannot include several photos because of copyright. The final section of the report will focus on recommendations. Sofia will handle the budget, while Amir will write the background. Their lecturer liked the clear timeline in the proposal. The final report must be submitted by midnight.',
        questions: [
          text('L21', 21, 'Research topic:', 'urban gardens'),
          text('L22', 22, 'Reason for changing method:', 'low response rate'),
          text('L23', 23, 'Sample group:', 'commuters'),
          text('L24', 24, 'Analysis tool:', 'spreadsheet'),
          text('L25', 25, 'Consultant meeting day:', 'Wednesday'),
          text('L26', 26, 'Problem with some photos:', 'copyright'),
          text('L27', 27, 'Final report section focus:', 'recommendations'),
          text('L28', 28, 'Sofia is responsible for:', 'budget'),
          text('L29', 29, 'Lecturer liked the:', 'clear timeline'),
          text('L30', 30, 'Submission time:', 'midnight'),
        ],
      },
      {
        title: 'Part 4: Lecture on Renewable Energy',
        instruction: 'Questions 31-40. Complete the summary.',
        transcript:
          'This lecture looks at renewable energy systems in coastal regions. The main source considered today is tidal power. One early research site was the Bay of Fundy in Canada. Modern turbines often use carbon fibre because it is strong and light. A major environmental concern is fish migration. Energy can be stored in batteries when demand is low. Swansea is a city often discussed in relation to tidal lagoon projects. Installation costs have fallen by 18 percent in the last decade. Government support usually comes through tax credits. The future challenge is maintenance, especially in rough saltwater conditions. The lecture concludes that mixed systems are more reliable than any single source.',
        questions: [
          text('L31', 31, 'Main energy source:', 'tidal power'),
          text('L32', 32, 'Early research site:', 'Bay of Fundy'),
          text('L33', 33, 'Turbine material:', ['carbon fibre', 'carbon fiber']),
          text('L34', 34, 'Environmental concern:', 'fish migration'),
          text('L35', 35, 'Storage method:', 'batteries'),
          text('L36', 36, 'City example:', 'Swansea'),
          text('L37', 37, 'Cost reduction:', ['18 percent', '18%']),
          text('L38', 38, 'Policy support:', 'tax credits'),
          text('L39', 39, 'Future challenge:', 'maintenance'),
          text('L40', 40, 'Best long-term approach:', 'mixed systems'),
        ],
      },
    ],
  },
  reading: {
    parts: [
      {
        title: 'Passage 1: Urban Rooftop Farms',
        instruction: 'Questions 1-13. Read the passage and answer the questions.',
        passage: [
          'A. Rooftop farming has moved from novelty to practical urban planning tool. In dense cities, unused roofs can become productive spaces, but the best projects begin with careful structural checks. Engineers calculate load, drainage and wind exposure before a single container is installed.',
          'B. A roof has a distinct microclimate. It may be warmer, windier and drier than the street below. Successful farms use light soil, shade cloth and drip irrigation to protect plants. Many schemes collect rainwater from nearby surfaces and store it in tanks for dry weeks.',
          'C. The social value of rooftop farms is often as important as the harvest. Some buildings add beehives, compost points and weekend workshops. Volunteers may help with watering, seedling preparation and distribution to local food banks.',
          'D. However, rooftop agriculture is not free from problems. Insurance can be expensive, and lifts may not be suitable for moving supplies. Several larger farms now use a sensor network to monitor moisture and temperature. Restaurants often buy the most delicate herbs because they can receive them within hours of picking.',
        ],
        questions: [
          choice('R1', 1, 'Rooftop farming is still mainly treated as a novelty.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'FALSE'),
          choice('R2', 2, 'Structural checks should happen before planting begins.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'TRUE'),
          choice('R3', 3, 'Rooftop farms always produce more food than street-level farms.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'NOT GIVEN'),
          choice('R4', 4, 'Shade cloth can help protect rooftop crops.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'TRUE'),
          choice('R5', 5, 'Most volunteers are professional gardeners.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'NOT GIVEN'),
          choice('R6', 6, 'Restaurants may buy herbs from rooftop farms because delivery is fast.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'TRUE'),
          text('R7', 7, 'A roof creates a special ______ for plants.', 'microclimate'),
          text('R8', 8, 'Some farms store ______ in tanks.', 'rainwater'),
          text('R9', 9, 'Some projects include ______ for pollination and education.', 'beehives'),
          text('R10', 10, '______ may help with watering and preparing seedlings.', 'volunteers'),
          text('R11', 11, 'One financial difficulty is ______.', 'insurance'),
          text('R12', 12, 'Large farms may monitor conditions with a ______.', 'sensor network'),
          text('R13', 13, 'A key customer group for delicate herbs is ______.', 'restaurants'),
        ],
      },
      {
        title: 'Passage 2: The Science of Sleep and Memory',
        instruction: 'Questions 14-26. Choose the correct answer or complete the notes.',
        passage: [
          'A. Sleep is not a passive state. During the night, the brain sorts recent experiences and decides which details should remain available. Researchers describe this as consolidation, although the process is not fully understood.',
          'B. Deep sleep appears to strengthen factual memories. Slow waves pass across the cortex, and information is repeatedly reactivated. Students who sleep after learning vocabulary often recall more items the next day than those who stay awake.',
          'C. Rapid eye movement sleep is linked with emotional learning and flexible problem solving. Dreams may not be necessary, but the brain seems to connect distant ideas during this stage.',
          'D. Short naps can help, especially for shift workers who cannot maintain a normal night schedule. Yet naps are not magic. They can lead to overconfidence if people believe a brief rest has replaced proper sleep.',
          'E. Consumer sleep technology is useful, but its data should be interpreted carefully. Many devices estimate sleep stages from movement, not direct brain activity. Experts remain cautiously optimistic and still recommend a consistent schedule, a cool room of about 18 degrees and reduced caffeine because caffeine delays sleep.',
        ],
        questions: [
          choice('R14', 14, 'Paragraph A mainly explains that sleep:', ['sorts recent experiences', 'prevents all forgetting', 'is fully understood'], 'sorts recent experiences'),
          choice('R15', 15, 'Paragraph B focuses on:', ['dream reports', 'deep sleep and facts', 'exercise routines'], 'deep sleep and facts'),
          choice('R16', 16, 'Paragraph C links REM sleep with:', ['emotional learning', 'physical growth', 'hunger control'], 'emotional learning'),
          choice('R17', 17, 'Paragraph D says naps are useful but:', ['can create overconfidence', 'should last all afternoon', 'are harmful for shift workers'], 'can create overconfidence'),
          choice('R18', 18, 'Paragraph E warns that devices must be:', ['interpreted carefully', 'avoided completely', 'used only by doctors'], 'interpreted carefully'),
          text('R19', 19, 'Caffeine ______ sleep.', 'delays'),
          text('R20', 20, 'The recommended room temperature is about ______.', ['18 degrees', '18']),
          text('R21', 21, 'Many devices estimate sleep stages from ______.', 'movement'),
          text('R22', 22, 'Deep sleep includes electrical patterns called ______.', 'slow waves'),
          text('R23', 23, 'Naps may be especially useful for ______.', 'shift workers'),
          text('R24', 24, 'A possible problem after naps is ______.', 'overconfidence'),
          text('R25', 25, 'Experts are described as ______ about sleep technology.', 'cautiously optimistic'),
          text('R26', 26, 'Experts still recommend keeping a ______.', 'consistent schedule'),
        ],
      },
      {
        title: 'Passage 3: Materials That Repair Themselves',
        instruction: 'Questions 27-40. Complete the notes and answer TRUE, FALSE or NOT GIVEN.',
        passage: [
          'A. Self-healing materials were first developed for aerospace components, where small cracks can be difficult to inspect. The earliest systems placed tiny microcapsules inside a polymer. When a crack broke the capsules, resin flowed into the gap and hardened.',
          'B. Later designs copied natural repair systems. Some polymers behave like vines, forming new links when heated or exposed to ultraviolet light. Other materials use bacteria that produce minerals, but those bacteria need moisture to remain active.',
          'C. The technology could lower maintenance costs in bridges, roads and pipelines. It is not a replacement for inspection, because large gaps or repeated damage still require engineers. Cost is another barrier, and laboratory performance is often better than field performance.',
          'D. Researchers now see self-healing materials as a complement to traditional maintenance. Future applications may include bridges and medical implants, but commercial products remain selective rather than universal.',
        ],
        questions: [
          text('R27', 27, 'The first self-healing materials were developed for ______.', 'aerospace'),
          text('R28', 28, 'Early capsules contained ______.', 'resin'),
          text('R29', 29, 'Cracks broke the tiny ______.', 'microcapsules'),
          text('R30', 30, 'Some polymers are compared to ______.', 'vines'),
          text('R31', 31, 'A limitation is that ______ still need engineers.', 'large gaps'),
          text('R32', 32, 'The technology could reduce ______ costs.', 'maintenance'),
          text('R33', 33, 'Another barrier is ______.', 'cost'),
          choice('R34', 34, 'Laboratory performance is always the same as field performance.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'FALSE'),
          choice('R35', 35, 'Some bacteria-based materials need moisture.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'TRUE'),
          choice('R36', 36, 'All self-healing products are already widely commercialised.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'FALSE'),
          choice('R37', 37, 'Some polymers can form new links after exposure to ultraviolet light.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'TRUE'),
          choice('R38', 38, 'Self-healing materials remove the need for inspections.', ['TRUE', 'FALSE', 'NOT GIVEN'], 'FALSE'),
          text('R39', 39, 'Researchers describe the materials as a ______ to traditional maintenance.', 'complement'),
          text('R40', 40, 'One possible future infrastructure application is ______.', 'bridges'),
        ],
      },
    ],
  },
  writing: {
    tasks: [
      {
        id: 'W1',
        title: 'Task 1',
        minWords: 150,
        minutes: 20,
        prompt:
          'The chart below shows the percentage of commuters using five types of transport in a city in 2010 and 2025. Summarise the information by selecting and reporting the main features, and make comparisons where relevant. Data: bus 38% to 29%, metro 22% to 31%, bicycle 6% to 15%, car 30% to 20%, walking 4% to 5%.',
      },
      {
        id: 'W2',
        title: 'Task 2',
        minWords: 250,
        minutes: 40,
        prompt:
          'Some people believe universities should focus on employability, while others think higher education should develop broader knowledge and critical thinking. Discuss both views and give your own opinion.',
      },
    ],
  },
  speaking: {
    parts: [
      {
        id: 'S1',
        title: 'Part 1: Interview',
        time: '4-5 minutes',
        prompts: [
          'Do you work or study?',
          'What part of your daily routine do you enjoy most?',
          'How often do you use public transport?',
          'Do you prefer studying alone or with other people?',
        ],
      },
      {
        id: 'S2',
        title: 'Part 2: Long Turn',
        time: '1 minute preparation, 2 minutes speaking',
        prompts: [
          'Describe a place in your city that you think should be improved.',
          'You should say where it is, what problems it has, how it could be improved, and why these changes would matter.',
        ],
      },
      {
        id: 'S3',
        title: 'Part 3: Discussion',
        time: '4-5 minutes',
        prompts: [
          'What makes a city a good place to live?',
          'Should governments prioritise public transport over roads?',
          'How can local communities influence urban planning?',
          'Do you think technology will solve most city problems?',
        ],
      },
    ],
  },
};

let examState = createFreshState();
let stateLoaded = false;
let timerId = null;
let recognition = null;
let recognitionTargetId = null;

export function initIelts() {
  loadState();
  ensureTimer();
  renderIelts();
}

function createFreshState() {
  return {
    started: false,
    submitted: false,
    activeModule: 'listening',
    activePart: { listening: 0, reading: 0, writing: 0, speaking: 0 },
    remaining: MODULES.reduce((acc, item) => {
      acc[item.key] = item.minutes * 60;
      return acc;
    }, {}),
    answers: {},
    results: null,
  };
}

function loadState() {
  if (stateLoaded) return;
  stateLoaded = true;

  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null');
    if (!saved) return;
    const fresh = createFreshState();
    examState = {
      ...fresh,
      ...saved,
      activePart: { ...fresh.activePart, ...(saved.activePart || {}) },
      remaining: { ...fresh.remaining, ...(saved.remaining || {}) },
      answers: saved.answers || {},
      results: saved.results || null,
    };
  } catch {
    examState = createFreshState();
  }
}

function saveState() {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(examState));
}

function ensureTimer() {
  if (timerId) return;
  timerId = window.setInterval(() => {
    if (!examState.started || examState.submitted) return;
    const key = examState.activeModule;
    if (examState.remaining[key] <= 0) return;

    examState.remaining[key] -= 1;
    updateTimerDisplay();
    updateProgressDisplay();
    if (examState.remaining[key] % 15 === 0) saveState();
    if (examState.remaining[key] === 0) {
      saveState();
      toast(`${getModuleConfig(key).title} time is over.`, 'error');
    }
  }, 1000);
}

function renderIelts() {
  const root = document.getElementById('ielts-root');
  if (!root) return;

  root.innerHTML = examState.submitted
    ? renderResults()
    : examState.started
      ? renderExamShell()
      : renderStartScreen();

  bindInputs();
  updateTimerDisplay();
  updateProgressDisplay();
}

function renderStartScreen() {
  return `
    <div class="ielts-start">
      <div class="ielts-start-main">
        <div class="ielts-kicker">Academic mock test</div>
        <h1>IELTS full test practice</h1>
        <p>Listening, Reading, Writing và Speaking trong một luồng làm bài có timer, lưu đáp án và bảng điểm IELTS band sau khi nộp.</p>
        <div class="ielts-start-actions">
          <button class="btn btn-primary btn-lg" onclick="ieltsStartExam()">Bắt đầu làm đề</button>
          <button class="btn btn-ghost btn-lg" onclick="ieltsResetExam()">Làm lại từ đầu</button>
        </div>
      </div>
      <div class="ielts-start-grid">
        ${MODULES.map(item => `
          <div class="ielts-start-card">
            <div class="ielts-card-label">${item.title}</div>
            <div class="ielts-card-value">${item.minutes} phút</div>
            <div class="ielts-card-meta">${item.total} ${item.key === 'writing' ? 'tasks' : item.key === 'speaking' ? 'parts' : 'questions'}</div>
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
          <div class="ielts-kicker">${IELTS_TEST.title}</div>
          <h1>${activeConfig.title}</h1>
        </div>
        <div class="ielts-header-actions">
          <div class="ielts-timer" id="ielts-timer">--:--</div>
          <button class="btn btn-secondary" onclick="ieltsSaveDraft()">Lưu nháp</button>
          <button class="btn btn-primary" onclick="ieltsSubmitExam()">Nộp bài</button>
        </div>
      </div>
      <div class="ielts-progress-line">
        <div class="progress-track">
          <div class="progress-fill" id="ielts-total-progress" style="width:0%;background:linear-gradient(90deg,var(--accent),var(--accent2));"></div>
        </div>
        <span id="ielts-progress-text">0% hoàn thành</span>
      </div>
      <div class="ielts-module-tabs">
        ${MODULES.map(item => `
          <button class="ielts-module-tab ${examState.activeModule === item.key ? 'active' : ''}" onclick="ieltsSetModule('${item.key}')">
            <span>${item.title}</span>
            <small>${countAnswered(item.key)}/${item.total}</small>
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
          <span>${part.title || part.id}</span>
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
  const part = IELTS_TEST.listening.parts[partIndex];

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${part.title}</h2>
        <p>${part.instruction}</p>
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
  const part = IELTS_TEST.reading.parts[partIndex];

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${part.title}</h2>
        <p>${part.instruction}</p>
      </div>
    </div>
    <div class="ielts-reading-grid">
      <article class="ielts-passage">
        ${part.passage.map(paragraph => `<p>${escapeHtml(paragraph)}</p>`).join('')}
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
  const task = IELTS_TEST.writing.tasks[taskIndex];
  const answer = getAnswer(task.id);

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${task.title}</h2>
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
  const part = IELTS_TEST.speaking.parts[partIndex];
  const answer = getAnswer(part.id);

  return `
    <div class="ielts-section-head">
      <div>
        <h2>${part.title}</h2>
        <p>${part.time}</p>
      </div>
      <div class="ielts-audio-actions">
        <button class="btn btn-secondary" onclick="ieltsStartSpeech('${part.id}')">Start speech</button>
        <button class="btn btn-ghost" onclick="ieltsStopSpeech()">Stop</button>
      </div>
    </div>
    <div class="ielts-speaking-prompts">
      ${part.prompts.map(prompt => `<div class="ielts-prompt-line">${escapeHtml(prompt)}</div>`).join('')}
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
            ${question.options.map(option => `
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
      saveState();
      updateProgressDisplay();
      updateWordCounter(id, input.value);
    });
    updateWordCounter(id, input.value);
  });
}

function startExam() {
  examState.started = true;
  examState.submitted = false;
  examState.results = null;
  saveState();
  renderIelts();
}

function resetExam() {
  stopAudio();
  stopSpeech();
  examState = createFreshState();
  saveState();
  renderIelts();
}

function saveDraft() {
  syncInputs();
  saveState();
  toast('Đã lưu nháp IELTS.', 'success');
}

function submitExam() {
  syncInputs();
  examState.submitted = true;
  examState.results = scoreExam();
  saveState();
  renderIelts();

  if ((examState.results?.overallBand || 0) >= 7) {
    confetti();
  }
}

function setModule(key) {
  if (!MODULES.some(item => item.key === key)) return;
  syncInputs();
  stopAudio();
  examState.activeModule = key;
  saveState();
  renderIelts();
}

function setPart(index) {
  const key = examState.activeModule;
  const total = getPartsForModule(key).length;
  if (index < 0 || index >= total) return;
  syncInputs();
  stopAudio();
  examState.activePart[key] = index;
  saveState();
  renderIelts();
}

function playListening(partIndex) {
  const part = IELTS_TEST.listening.parts[partIndex];
  if (!part || !('speechSynthesis' in window)) return;
  stopAudio();

  const utterance = new SpeechSynthesisUtterance(part.transcript);
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
  recognitionTargetId = targetId;
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
    saveState();
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
    recognitionTargetId = null;
  }
}

function syncInputs() {
  document.querySelectorAll('[data-ielts-answer]').forEach(input => {
    examState.answers[input.dataset.ieltsAnswer] = input.value;
  });
}

function scoreExam() {
  const listening = scoreObjective('listening');
  const reading = scoreObjective('reading');
  const writing = scoreWriting();
  const speaking = scoreSpeaking();
  const overallBand = roundHalf((listening.band + reading.band + writing.band + speaking.band) / 4);

  return { listening, reading, writing, speaking, overallBand };
}

function scoreObjective(moduleKey) {
  const questions = getObjectiveQuestions(moduleKey);
  const items = questions.map(question => {
    const submitted = getAnswer(question.id);
    const isCorrect = matchesAnswer(submitted, question.answer);
    return {
      number: question.number,
      prompt: question.prompt,
      submitted,
      correctAnswer: displayAnswer(question.answer),
      isCorrect,
    };
  });
  const correct = items.filter(item => item.isCorrect).length;
  const answered = items.filter(item => item.submitted.trim()).length;
  const band = moduleKey === 'listening' ? listeningBand(correct) : readingBand(correct);

  return { total: questions.length, answered, correct, band, items };
}

function scoreWriting() {
  const task1 = scoreProduction(getAnswer('W1'), 150, 'task1');
  const task2 = scoreProduction(getAnswer('W2'), 250, 'task2');
  const band = roundHalf((task1.band + task2.band * 2) / 3);

  return { band, task1, task2 };
}

function scoreSpeaking() {
  const combined = IELTS_TEST.speaking.parts
    .map(part => getAnswer(part.id))
    .join(' ');
  const score = scoreProduction(combined, 180, 'speaking');
  return { ...score, band: score.band };
}

function scoreProduction(textValue, minWords, mode) {
  const words = tokenize(textValue);
  if (words.length === 0) {
    return {
      band: 0,
      wordCount: 0,
      criteria: { task: 0, coherence: 0, lexical: 0, grammar: 0 },
      feedback: ['No response submitted.'],
    };
  }

  const sentences = textValue.split(/[.!?]+/).map(x => x.trim()).filter(Boolean);
  const paragraphs = textValue.split(/\n+/).map(x => x.trim()).filter(Boolean);
  const uniqueRatio = new Set(words).size / words.length;
  const connectors = countMatches(words, [
    'however',
    'therefore',
    'moreover',
    'although',
    'whereas',
    'because',
    'firstly',
    'secondly',
    'overall',
    'in',
    'addition',
    'consequently',
  ]);
  const topicTerms = countMatches(words, mode === 'task1'
    ? ['increase', 'decrease', 'percentage', 'commuters', 'metro', 'bus', 'car', 'bicycle', 'walking']
    : mode === 'task2'
      ? ['university', 'employability', 'education', 'knowledge', 'critical', 'thinking', 'opinion']
      : ['city', 'transport', 'community', 'government', 'technology', 'planning']);

  const lengthRatio = Math.min(words.length / minWords, 1.25);
  const task = clamp(4 + lengthRatio * 1.8 + Math.min(topicTerms, 5) * 0.28, 4, 8.5);
  const coherence = clamp(4.2 + Math.min(paragraphs.length, 4) * 0.45 + Math.min(connectors, 8) * 0.18, 4, 8.5);
  const lexical = clamp(4.1 + uniqueRatio * 4.2 + Math.min(words.length, 320) / 220, 4, 8.5);
  const grammar = clamp(4.2 + Math.min(sentences.length, 14) * 0.18 + averageSentenceLength(words, sentences) / 20, 4, 8.5);
  let band = roundHalf((task + coherence + lexical + grammar) / 4);

  if (words.length < minWords * 0.5) band = Math.min(band, 5);
  if (words.length < minWords * 0.25) band = Math.min(band, 4);

  const feedback = [];
  if (words.length < minWords) feedback.push(`Under the suggested minimum of ${minWords} words.`);
  if (paragraphs.length < 2 && mode !== 'speaking') feedback.push('Use clearer paragraphing.');
  if (connectors < 3) feedback.push('Add more linking phrases to improve coherence.');
  if (uniqueRatio < 0.42) feedback.push('Increase lexical range and avoid repeating the same wording.');
  if (feedback.length === 0) feedback.push('Response length, cohesion and vocabulary range are on track.');

  return {
    band,
    wordCount: words.length,
    criteria: {
      task: roundHalf(task),
      coherence: roundHalf(coherence),
      lexical: roundHalf(lexical),
      grammar: roundHalf(grammar),
    },
    feedback,
  };
}

function renderResults() {
  const results = examState.results || scoreExam();

  return `
    <div class="ielts-results">
      <div class="ielts-result-hero">
        <div>
          <div class="ielts-kicker">Submitted result</div>
          <h1>Overall Band ${formatBand(results.overallBand)}</h1>
          <p>Listening và Reading được chấm theo đáp án. Writing và Speaking là điểm ước tính dựa trên độ dài, mạch lạc, từ vựng và dấu hiệu bám đề.</p>
        </div>
        <div class="ielts-overall-band">${formatBand(results.overallBand)}</div>
      </div>
      <div class="ielts-result-grid">
        ${renderBandCard('Listening', results.listening.band, `${results.listening.correct}/${results.listening.total} correct`)}
        ${renderBandCard('Reading', results.reading.band, `${results.reading.correct}/${results.reading.total} correct`)}
        ${renderBandCard('Writing', results.writing.band, `Task 1: ${formatBand(results.writing.task1.band)} | Task 2: ${formatBand(results.writing.task2.band)}`)}
        ${renderBandCard('Speaking', results.speaking.band, `${results.speaking.wordCount} words captured`)}
      </div>
      <div class="ielts-result-actions">
        <button class="btn btn-secondary" onclick="ieltsBackToExam()">Xem lại bài làm</button>
        <button class="btn btn-ghost" onclick="ieltsResetExam()">Làm đề mới</button>
      </div>
      <div class="ielts-feedback-grid">
        ${renderProductionFeedback('Writing Task 1', results.writing.task1)}
        ${renderProductionFeedback('Writing Task 2', results.writing.task2)}
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

function renderProductionFeedback(title, score) {
  return `
    <div class="ielts-feedback-card">
      <div class="section-title">${title}</div>
      <div class="ielts-criteria">
        <span>Task ${formatBand(score.criteria.task)}</span>
        <span>Coherence ${formatBand(score.criteria.coherence)}</span>
        <span>Lexical ${formatBand(score.criteria.lexical)}</span>
        <span>Grammar ${formatBand(score.criteria.grammar)}</span>
      </div>
      <div class="ielts-card-meta">${score.wordCount} words</div>
      <ul>
        ${score.feedback.map(item => `<li>${escapeHtml(item)}</li>`).join('')}
      </ul>
    </div>
  `;
}

function renderObjectiveReview(title, result) {
  return `
    <details class="ielts-review-block">
      <summary>${title} answer review (${result.correct}/${result.total})</summary>
      <div class="ielts-review-table">
        ${result.items.map(item => `
          <div class="ielts-review-row ${item.isCorrect ? 'correct' : 'wrong'}">
            <span>${item.number}</span>
            <span>${escapeHtml(item.submitted || '-')}</span>
            <span>${escapeHtml(item.correctAnswer)}</span>
          </div>
        `).join('')}
      </div>
    </details>
  `;
}

function backToExam() {
  examState.submitted = false;
  saveState();
  renderIelts();
}

function updateTimerDisplay() {
  const timer = document.getElementById('ielts-timer');
  if (!timer) return;
  const seconds = examState.remaining[examState.activeModule] || 0;
  timer.textContent = formatTime(seconds);
  timer.classList.toggle('warning', seconds <= 300);
}

function updateProgressDisplay() {
  const total = MODULES.reduce((sum, item) => sum + item.total, 0);
  const answered = MODULES.reduce((sum, item) => sum + countAnswered(item.key), 0);
  const percent = total ? Math.round(answered * 100 / total) : 0;
  const bar = document.getElementById('ielts-total-progress');
  const textNode = document.getElementById('ielts-progress-text');
  if (bar) bar.style.width = `${percent}%`;
  if (textNode) textNode.textContent = `${answered}/${total} answered`;
}

function updateWordCounter(id, value) {
  const counter = document.getElementById(`ielts-count-${id}`);
  if (!counter) return;
  counter.textContent = `${tokenize(value).length} words`;
}

function partProgressText(moduleKey, index) {
  const parts = getPartsForModule(moduleKey);
  const part = parts[index];
  const ids = partQuestionIds(moduleKey, part);
  const answered = ids.filter(id => getAnswer(id).trim()).length;
  return `${answered}/${ids.length}`;
}

function countAnswered(moduleKey) {
  if (moduleKey === 'listening' || moduleKey === 'reading') {
    return getObjectiveQuestions(moduleKey).filter(question => getAnswer(question.id).trim()).length;
  }
  if (moduleKey === 'writing') {
    return IELTS_TEST.writing.tasks.filter(task => getAnswer(task.id).trim()).length;
  }
  return IELTS_TEST.speaking.parts.filter(part => getAnswer(part.id).trim()).length;
}

function partQuestionIds(moduleKey, part) {
  if (moduleKey === 'writing') return [part.id];
  if (moduleKey === 'speaking') return [part.id];
  return part.questions.map(question => question.id);
}

function getPartsForModule(moduleKey) {
  if (moduleKey === 'listening') return IELTS_TEST.listening.parts;
  if (moduleKey === 'reading') return IELTS_TEST.reading.parts;
  if (moduleKey === 'writing') return IELTS_TEST.writing.tasks;
  return IELTS_TEST.speaking.parts;
}

function getObjectiveQuestions(moduleKey) {
  const source = moduleKey === 'listening' ? IELTS_TEST.listening.parts : IELTS_TEST.reading.parts;
  return source.flatMap(part => part.questions);
}

function getModuleConfig(moduleKey) {
  return MODULES.find(item => item.key === moduleKey) || MODULES[0];
}

function getAnswer(id) {
  return String(examState.answers[id] || '');
}

function matchesAnswer(submitted, accepted) {
  if (!String(submitted || '').trim()) return false;
  const acceptedList = Array.isArray(accepted) ? accepted : [accepted];
  const normalizedSubmitted = normalizeAnswer(submitted);
  return acceptedList.some(item => normalizeAnswer(item) === normalizedSubmitted);
}

function normalizeAnswer(value) {
  return String(value || '')
    .toLowerCase()
    .replace(/&/g, ' and ')
    .replace(/\bper cent\b/g, 'percent')
    .replace(/%/g, ' percent ')
    .replace(/[^a-z0-9.]+/g, ' ')
    .replace(/\b0+([0-9])/g, '$1')
    .replace(/\s+/g, ' ')
    .trim();
}

function displayAnswer(answer) {
  return Array.isArray(answer) ? answer[0] : answer;
}

function listeningBand(raw) {
  return bandFromMap(raw, [
    [39, 9], [37, 8.5], [35, 8], [32, 7.5], [30, 7], [26, 6.5],
    [23, 6], [18, 5.5], [16, 5], [13, 4.5], [10, 4], [8, 3.5],
    [6, 3], [4, 2.5], [1, 2], [0, 0],
  ]);
}

function readingBand(raw) {
  return bandFromMap(raw, [
    [39, 9], [37, 8.5], [35, 8], [33, 7.5], [30, 7], [27, 6.5],
    [23, 6], [19, 5.5], [15, 5], [13, 4.5], [10, 4], [8, 3.5],
    [6, 3], [4, 2.5], [1, 2], [0, 0],
  ]);
}

function bandFromMap(raw, map) {
  const match = map.find(([min]) => raw >= min);
  return match ? match[1] : 0;
}

function tokenize(value) {
  return String(value || '')
    .toLowerCase()
    .match(/[a-z]+(?:'[a-z]+)?|[0-9]+/g) || [];
}

function countMatches(words, terms) {
  const textValue = words.join(' ');
  return terms.reduce((sum, term) => sum + (textValue.includes(term) ? 1 : 0), 0);
}

function averageSentenceLength(words, sentences) {
  if (!sentences.length) return Math.min(words.length, 18);
  return words.length / sentences.length;
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function roundHalf(value) {
  return Math.round(value * 2) / 2;
}

function formatBand(value) {
  return Number(value || 0).toFixed(1);
}

function formatTime(totalSeconds) {
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
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

window.addEventListener('beforeunload', saveState);
