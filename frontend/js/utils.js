// wordwave/frontend/js/utils.js

export function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

export function speak(text) {
  if (!('speechSynthesis' in window)) return;
  const u = new SpeechSynthesisUtterance(text);
  u.lang = 'en-US';
  u.rate = 0.85;
  speechSynthesis.cancel();
  speechSynthesis.speak(u);
}

export function toast(msg, type = '') {
  const el = document.getElementById('toast');
  if (!el) return;
  el.textContent  = msg;
  el.className    = 'toast ' + type + ' show';
  clearTimeout(window._toastTimer);
  window._toastTimer = setTimeout(() => el.classList.remove('show'), 2800);
}

export function confetti() {
  const colors = ['#63b3ed','#4fd1c5','#f6ad55','#b794f4','#68d391','#f687b3'];
  for (let i = 0; i < 40; i++) {
    const el = document.createElement('div');
    el.className = 'confetti-piece';
    el.style.cssText = `left:${Math.random()*100}vw;background:${colors[Math.floor(Math.random()*colors.length)]};animation-duration:${1+Math.random()}s;animation-delay:${Math.random()*0.4}s;`;
    document.body.appendChild(el);
    setTimeout(() => el.remove(), 2000);
  }
}

export function badgeClass(level) {
  return { A1:'badge-a1', A2:'badge-a2', B1:'badge-b1', B2:'badge-b2', C1:'badge-c1' }[level] || '';
}

export function levelColor(level) {
  return { A1:'var(--green)', A2:'var(--accent)', B1:'var(--accent2)', B2:'var(--accent3)', C1:'var(--purple)' }[level] || 'var(--accent)';
}

export function el(id) { return document.getElementById(id); }

export function html(id, content) {
  const e = document.getElementById(id);
  if (e) e.innerHTML = content;
}
