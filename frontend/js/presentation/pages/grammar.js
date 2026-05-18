// wordwave/frontend/js/pages/grammar.js
import { api } from '../../infrastructure/api/apiClient.js';
import { renderPagination } from '../components/pagination.js';
import { toast, badgeClass } from '../../shared/utils.js';

let lessons = [];
let currentLevel = 'all';
let currentSearch = '';
let currentPage = 1;
let pageSize = 6;

export async function initGrammar() {
  if (lessons.length === 0) {
    try {
      lessons = await api.getGrammar();
    } catch {
      toast('Không thể tải ngữ pháp. Kiểm tra kết nối API.', 'error');
      return;
    }
  }

  renderGrammarPage(1);
}

export function filterGrammarByLevel(level) {
  document.querySelectorAll('#page-grammar .tab').forEach(t => t.classList.remove('active'));
  window.event?.target?.classList.add('active');
  currentLevel = level;
  renderGrammarPage(1);
}

export function filterGrammarByName(value) {
  currentSearch = normalizeSearch(value);
  renderGrammarPage(1);
}

function getFilteredLessons() {
  return lessons.filter(g => {
    const meta = getGrammarMeta(g);
    const matchesLevel = currentLevel === 'all' || g.level === currentLevel;
    const matchesSearch = !currentSearch
      || normalizeSearch(g.title).includes(currentSearch)
      || normalizeSearch(meta.nameVi).includes(currentSearch);
    return matchesLevel && matchesSearch;
  });
}

function renderGrammarPage(page) {
  const data = getFilteredLessons();
  const totalPages = Math.max(1, Math.ceil(data.length / pageSize));
  currentPage = Math.min(Math.max(page, 1), totalPages);
  const start = (currentPage - 1) * pageSize;
  renderGrammar(data.slice(start, start + pageSize));

  renderPagination('grammar-pagination', {
    page: currentPage,
    total: data.length,
    limit: pageSize,
    onPageChange: renderGrammarPage,
    onPageSizeChange: nextSize => {
      pageSize = nextSize;
      renderGrammarPage(1);
    },
  });
}

function renderGrammar(data) {
  const container = document.getElementById('grammar-list');
  if (!container) return;

  container.innerHTML = data.length ? data.map(g => `
    <div class="grammar-card" id="gc-${escapeHtml(g.id)}">
      <div class="grammar-header" onclick="toggleGrammar(${Number(g.id)})">
        <div class="grammar-heading">
          <span class="badge ${badgeClass(g.level)}">${escapeHtml(g.level)}</span>
          <div class="grammar-title-wrap">
            <div class="grammar-title">${escapeHtml(g.title)}</div>
            <div class="grammar-title-vi">${escapeHtml(getGrammarMeta(g).nameVi)}</div>
          </div>
        </div>
        <div class="grammar-desc-row">
          <div class="grammar-desc">${escapeHtml(g.description)}</div>
          <div class="grammar-usage"><span>Cách dùng:</span> ${escapeHtml(getGrammarMeta(g).usageVi)}</div>
        </div>
      </div>
      <div class="grammar-content" id="gContent-${escapeHtml(g.id)}">
        <div style="background:rgba(99,179,237,0.08);border:1px solid var(--border);border-radius:var(--r2);padding:12px;margin-bottom:12px;font-family:var(--mono);font-size:13px;color:var(--accent2);">${escapeHtml(g.formula)}</div>
        ${(g.examples || []).map(e => `
          <div class="example-block">
            <div class="example-en">${escapeHtml(e.en)}</div>
            <div class="example-vi">${escapeHtml(e.vi)}</div>
          </div>`).join('')}
        <div style="margin-top:12px;padding:10px 12px;background:rgba(246,173,85,0.08);border:1px solid rgba(246,173,85,0.2);border-radius:var(--r2);font-size:13px;color:var(--accent3);">
          &#128161; ${escapeHtml(g.tips)}
        </div>
      </div>
      <div class="grammar-footer" onclick="toggleGrammar(${Number(g.id)})">
        <span style="font-size:12px;color:var(--text3);">Nhấn để xem chi tiết</span>
        <span style="color:var(--accent);" id="gArrow-${escapeHtml(g.id)}">&#9660;</span>
      </div>
    </div>`).join('') : '<div style="color:var(--text3);text-align:center;padding:20px;">Không có ngữ pháp phù hợp</div>';
}

function normalizeSearch(value) {
  return String(value || '').trim().toLowerCase();
}

function getGrammarMeta(grammar) {
  const meta = grammarMeta[grammar.title] || {};
  return {
    nameVi: meta.nameVi || toVietnameseName(grammar.title),
    usageVi: meta.usageVi || toVietnameseUsage(grammar),
  };
}

function toVietnameseName(title) {
  return title
    .replace(/Present/g, 'Hiện tại')
    .replace(/Past/g, 'Quá khứ')
    .replace(/Future/g, 'Tương lai')
    .replace(/Perfect/g, 'hoàn thành')
    .replace(/Continuous/g, 'tiếp diễn')
    .replace(/Simple/g, 'đơn')
    .replace(/Passive/g, 'bị động')
    .replace(/Conditional/g, 'câu điều kiện')
    .replace(/Clauses/g, 'mệnh đề')
    .replace(/Clause/g, 'mệnh đề')
    .replace(/Relative/g, 'quan hệ')
    .replace(/Modal/g, 'động từ khuyết thiếu')
    .replace(/Gerund/g, 'danh động từ')
    .replace(/Infinitive/g, 'động từ nguyên mẫu')
    .replace(/Inversion/g, 'đảo ngữ');
}

function toVietnameseUsage(grammar) {
  const text = `${grammar.title} ${grammar.description}`.toLowerCase();
  if (text.includes('ielts')) return 'Dùng trong IELTS Writing và Speaking để diễn đạt ý học thuật rõ hơn, có liên kết và ít lỗi ngữ pháp hơn.';
  if (text.includes('conditional')) return 'Dùng để nói về điều kiện, giả định, kết quả có thể xảy ra hoặc tình huống không có thật.';
  if (text.includes('passive')) return 'Dùng khi muốn nhấn mạnh hành động hoặc đối tượng chịu tác động thay vì người thực hiện.';
  if (text.includes('relative')) return 'Dùng để bổ sung hoặc xác định thông tin cho danh từ, giúp câu dài nhưng vẫn rõ nghĩa.';
  if (text.includes('reported')) return 'Dùng khi thuật lại lời nói, câu hỏi hoặc yêu cầu của người khác.';
  if (text.includes('modal')) return 'Dùng để thể hiện khả năng, nghĩa vụ, lời khuyên, suy đoán hoặc mức độ chắc chắn.';
  if (text.includes('perfect')) return 'Dùng để nối một hành động với một thời điểm khác, thường nhấn mạnh kết quả hoặc trải nghiệm.';
  if (text.includes('continuous')) return 'Dùng để nhấn mạnh hành động đang diễn ra tại một thời điểm cụ thể.';
  if (text.includes('contrast') || text.includes('although') || text.includes('despite')) return 'Dùng để nối hai ý trái ngược, rất hữu ích khi viết câu phức trong IELTS.';
  return 'Dùng để xây câu đúng ngữ pháp, diễn đạt ý tự nhiên hơn trong giao tiếp và bài viết.';
}

const grammarMeta = {
  'Present Simple': { nameVi: 'Thì hiện tại đơn', usageVi: 'Dùng để nói về thói quen, sự thật hiển nhiên, lịch trình và các hành động lặp lại.' },
  'Present Continuous': { nameVi: 'Thì hiện tại tiếp diễn', usageVi: 'Dùng để nói về hành động đang xảy ra hoặc tình huống tạm thời quanh hiện tại.' },
  'Past Simple': { nameVi: 'Thì quá khứ đơn', usageVi: 'Dùng để nói về hành động đã kết thúc tại một thời điểm xác định trong quá khứ.' },
  'Present Perfect': { nameVi: 'Thì hiện tại hoàn thành', usageVi: 'Dùng để nói về trải nghiệm, kết quả hoặc hành động bắt đầu trong quá khứ còn liên quan đến hiện tại.' },
  'Past Perfect': { nameVi: 'Thì quá khứ hoàn thành', usageVi: 'Dùng để chỉ hành động xảy ra trước một hành động khác trong quá khứ.' },
  'First Conditional': { nameVi: 'Câu điều kiện loại 1', usageVi: 'Dùng cho khả năng thật trong tương lai và kết quả có thể xảy ra.' },
  'Second Conditional': { nameVi: 'Câu điều kiện loại 2', usageVi: 'Dùng cho tình huống giả định ở hiện tại hoặc tương lai.' },
  'Mixed Conditionals': { nameVi: 'Câu điều kiện hỗn hợp', usageVi: 'Dùng khi điều kiện và kết quả thuộc hai mốc thời gian khác nhau.' },
  'IELTS Complex Sentences': { nameVi: 'Câu phức trong IELTS', usageVi: 'Dùng để kết hợp mệnh đề chính và phụ, giúp bài Writing đạt tiêu chí ngữ pháp đa dạng hơn.' },
  'IELTS Academic Hedging': { nameVi: 'Diễn đạt thận trọng học thuật', usageVi: 'Dùng may, might, tend to, appear to để tránh khẳng định quá tuyệt đối trong Writing Task 2.' },
  'IELTS Concession Sentences': { nameVi: 'Câu nhượng bộ trong IELTS', usageVi: 'Dùng để thừa nhận một ý đối lập trước khi đưa ra lập luận chính.' },
  'IELTS Cause And Effect Grammar': { nameVi: 'Ngữ pháp nguyên nhân - kết quả', usageVi: 'Dùng để giải thích lý do và hệ quả trong Task 2 hoặc Speaking Part 3.' },
  'IELTS Comparison Structures': { nameVi: 'Cấu trúc so sánh IELTS', usageVi: 'Dùng để so sánh số liệu trong Task 1 và so sánh quan điểm trong Task 2.' },
  'IELTS Noun Clauses': { nameVi: 'Mệnh đề danh từ IELTS', usageVi: 'Dùng what, whether, that để biến cả một ý thành chủ ngữ hoặc tân ngữ trong câu học thuật.' },
  'IELTS Advanced Relative Clauses': { nameVi: 'Mệnh đề quan hệ nâng cao', usageVi: 'Dùng which, where, whose hoặc mệnh đề quan hệ rút gọn để bổ sung thông tin chính xác.' },
  'IELTS Participle Phrases': { nameVi: 'Cụm phân từ IELTS', usageVi: 'Dùng V-ing hoặc V3 để rút gọn câu, tránh lặp chủ ngữ và tăng độ linh hoạt.' },
  'IELTS Nominalisation': { nameVi: 'Danh từ hóa trong IELTS', usageVi: 'Dùng danh từ học thuật thay cho động từ/cụm dài để câu trang trọng và súc tích hơn.' },
  'IELTS Referencing With This And These': { nameVi: 'Liên kết ý bằng this/these', usageVi: 'Dùng this hoặc these kèm danh từ để nối lại ý trước đó và tăng coherence.' },
  'IELTS Parallel Structures': { nameVi: 'Cấu trúc song song IELTS', usageVi: 'Dùng các thành phần cùng dạng để câu cân đối, rõ logic và dễ đọc.' },
  'IELTS Sentence Variety': { nameVi: 'Đa dạng cấu trúc câu IELTS', usageVi: 'Dùng xen kẽ câu đơn, câu ghép và câu phức để bài viết tự nhiên hơn.' },
  'IELTS Stance Adverbs': { nameVi: 'Trạng từ thể hiện lập trường', usageVi: 'Dùng arguably, generally, clearly để thể hiện thái độ và mức độ chắc chắn của người viết.' },
  'IELTS Complex Prepositional Phrases': { nameVi: 'Cụm giới từ học thuật', usageVi: 'Dùng due to, in terms of, with regard to để diễn đạt quan hệ ý trong văn học thuật.' },
  'IELTS Passive For Processes': { nameVi: 'Bị động mô tả quy trình', usageVi: 'Dùng nhiều trong Writing Task 1 process để mô tả bước xử lý mà không cần nêu người thực hiện.' },
  'IELTS Data Description Grammar': { nameVi: 'Ngữ pháp mô tả số liệu', usageVi: 'Dùng động từ xu hướng, so sánh và trạng từ mức độ để mô tả biểu đồ chính xác.' },
  'IELTS Opinion Grammar': { nameVi: 'Ngữ pháp nêu quan điểm', usageVi: 'Dùng it is clear that, I would argue that, there is evidence that để trình bày lập luận mạch lạc.' },
  'IELTS Problem Solution Grammar': { nameVi: 'Ngữ pháp bài problem-solution', usageVi: 'Dùng cấu trúc chỉ vấn đề, nguyên nhân, giải pháp và kết quả trong Task 2.' },
  'IELTS Advantage Disadvantage Grammar': { nameVi: 'Ngữ pháp ưu - nhược điểm', usageVi: 'Dùng để cân bằng hai mặt của một vấn đề và phát triển ý rõ ràng.' },
  'IELTS Reduced Adverb Clauses': { nameVi: 'Mệnh đề trạng ngữ rút gọn', usageVi: 'Dùng after/before/while + V-ing hoặc V3 để rút gọn câu khi cùng chủ ngữ.' },
};

function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

window.toggleGrammar = function(id) {
  const el = document.getElementById('gContent-' + id);
  const arrow = document.getElementById('gArrow-' + id);
  if (!el) return;
  el.classList.toggle('show');
  if (arrow) arrow.innerHTML = el.classList.contains('show') ? '&#9650;' : '&#9660;';
};
