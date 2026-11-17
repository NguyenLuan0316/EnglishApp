import { api } from '../../infrastructure/api/apiClient.js';
import { authState } from '../../application/state/authState.js';
import { state } from '../../application/state/appState.js';

export async function restoreSession() {
  try {
    authState.user = await api.me();
    state.setUser(authState.user.id);
  } catch (error) {
    if (error.status !== 401) throw error;
    authState.user = null;
  }
  document.body.dataset.auth = authState.user ? 'authenticated' : 'anonymous';
  document.querySelectorAll('[data-admin-only]').forEach(el => { el.hidden = authState.user?.role !== 'Admin'; });
  if (authState.user) {
    document.getElementById('account-name').textContent = authState.user.displayName;
    document.getElementById('account-name').title = authState.user.displayName;
    document.getElementById('account-menu-trigger').setAttribute('aria-label', `Tài khoản: ${authState.user.displayName}`);
    document.getElementById('account-email').textContent = authState.user.email;
    document.getElementById('account-role').textContent = authState.user.role === 'Admin' ? 'Quản trị viên' : 'Học viên';
    document.getElementById('xp-display').textContent = '—';
    try {
      state.applyLearningSummary(await api.getLearningProgress());
    } catch {
      document.getElementById('streak-display').textContent = '—';
    }
  }
}

function setMessage(element, message, success = false) {
  element.textContent = message;
  element.classList.toggle('success', success);
}

async function submitForm(form, action, success) {
  const button = form.querySelector('[type="submit"]');
  const message = form.querySelector('[role="alert"]');
  if (button.disabled) return;
  button.disabled = true;
  form.setAttribute('aria-busy', 'true');
  const original = button.textContent;
  button.textContent = 'Đang xử lý…';
  setMessage(message, '');
  try {
    await action(Object.fromEntries(new FormData(form)));
    await success();
  } catch (error) {
    setMessage(message, error.message);
    message.focus();
  } finally {
    button.disabled = false;
    button.textContent = original;
    form.removeAttribute('aria-busy');
  }
}

export function initLogin() {
  const form = document.getElementById('login-form');
  const message = form.querySelector('[role="alert"]');
  const params = new URLSearchParams(location.search);
  setMessage(message, params.has('registered') ? 'Tạo tài khoản thành công. Bạn có thể đăng nhập.' : params.has('changed') ? 'Đã đổi mật khẩu. Vui lòng đăng nhập lại.' : params.has('updated') ? 'Đã cập nhật tài khoản. Vui lòng đăng nhập lại bằng thông tin mới.' : params.has('expired') ? 'Phiên đăng nhập đã hết hạn hoặc quyền truy cập đã thay đổi. Vui lòng đăng nhập lại.' : '', params.has('registered') || params.has('changed') || params.has('updated'));
  form.onsubmit = event => {
    event.preventDefault();
    submitForm(form, data => api.login(data), async () => {
      // Reload clears all previous learner's page state, timers and pending exercises.
      const target = sessionStorage.getItem('ww_returnTo') || '/';
      sessionStorage.removeItem('ww_returnTo');
      location.assign(target.startsWith('/') && !target.startsWith('//') && !target.startsWith('/login') && !target.startsWith('/register') ? target : '/');
    });
  };
}

export function initRegister() {
  const form = document.getElementById('register-form');
  form.onsubmit = event => {
    event.preventDefault();
    submitForm(form, async data => {
      if (data.password !== data.confirmPassword) throw new Error('Hai mật khẩu chưa trùng nhau.');
      await api.register({ email: data.email, displayName: data.displayName, password: data.password });
    }, () => location.assign('/login?registered=1'));
  };
}

export function initAccount() {
  document.getElementById('profile-name').textContent = authState.user.displayName;
  document.getElementById('profile-email').textContent = authState.user.email;
  document.getElementById('profile-role').textContent = authState.user.role === 'Admin' ? 'Quản trị viên' : 'Học viên';
  const form = document.getElementById('password-form');
  form.onsubmit = event => {
    event.preventDefault();
    submitForm(form, async data => {
      if (data.newPassword !== data.confirmPassword) throw new Error('Hai mật khẩu mới chưa trùng nhau.');
      await api.changePassword({ currentPassword: data.currentPassword, newPassword: data.newPassword });
    }, () => location.assign('/login?changed=1'));
  };
}

export function initAuthControls() {
  const menu = document.getElementById('account-menu');
  const trigger = document.getElementById('account-menu-trigger');
  const panel = document.getElementById('account-menu-panel');
  const actions = [...panel.querySelectorAll('button')];
  const setOpen = open => {
    panel.hidden = !open;
    trigger.setAttribute('aria-expanded', String(open));
  };
  trigger.onclick = () => setOpen(panel.hidden);
  trigger.onkeydown = event => {
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      setOpen(true);
      actions[event.key === 'ArrowDown' ? 0 : actions.length - 1].focus();
    }
  };
  menu.addEventListener('keydown', event => {
    if (event.key === 'Escape' && !panel.hidden) {
      event.preventDefault();
      setOpen(false);
      trigger.focus();
    } else if (!panel.hidden && actions.includes(document.activeElement) && ['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) {
      event.preventDefault();
      const index = actions.indexOf(document.activeElement);
      const next = event.key === 'Home' ? 0 : event.key === 'End' ? actions.length - 1 : (index + (event.key === 'ArrowDown' ? 1 : -1) + actions.length) % actions.length;
      actions[next].focus();
    }
  });
  document.addEventListener('pointerdown', event => { if (!menu.contains(event.target)) setOpen(false); });
  menu.addEventListener('focusout', event => { if (!menu.contains(event.relatedTarget)) setOpen(false); });
  document.getElementById('account-password-button').onclick = () => {
    setOpen(false);
    window.navigate('account');
    document.getElementById('current-password').focus();
  };
  document.querySelectorAll('[data-toggle-password]').forEach(button => {
    button.onclick = () => {
      const input = document.getElementById(button.dataset.togglePassword);
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      button.textContent = show ? 'Ẩn' : 'Hiện';
      button.setAttribute('aria-pressed', String(show));
    };
  });
  document.getElementById('logout-button').onclick = async event => {
    const button = event.currentTarget;
    button.disabled = true;
    document.getElementById('logout-error').textContent = '';
    try {
      await api.logout();
      location.assign('/login');
    } catch (error) {
      if (error.status === 401) { location.assign('/login'); return; }
      document.getElementById('logout-error').textContent = error.message;
      button.disabled = false;
    }
  };
  window.addEventListener('wordwave:session-expired', () => {
    sessionStorage.setItem('ww_returnTo', location.pathname);
    location.replace('/login?expired=1');
  }, { once: true });
}
