import { api } from '../../infrastructure/api/apiClient.js';
import { authState } from '../../application/state/authState.js';

const el = id => document.getElementById(id);
let page = 1, search = '', loading = false, busy = false;
let edited = null, resetTarget = null, deleteTarget = null, trigger = null;
const limit = 20;

function closeEditors() {
  el('user-editor').hidden = el('user-password-editor').hidden = true;
  el('user-editor-form').reset();
  el('user-reset-form').reset();
  trigger?.focus();
}
async function mutate(form, action, done) {
  if (busy) return;
  busy = true;
  const controls = [...form.querySelectorAll('input, select, button')];
  const disabled = controls.map(c => c.disabled);
  controls.forEach(c => { c.disabled = true; });
  form.setAttribute('aria-busy', 'true');
  const message = form.querySelector('[role="alert"]');
  message.textContent = '';
  try { await action(); await done(); }
  catch (error) { message.textContent = error.message; message.focus(); }
  finally {
    controls.forEach((c, i) => { c.disabled = disabled[i]; });
    form.removeAttribute('aria-busy');
    busy = false;
  }
}
function openEditor(user, button) {
  if (busy) return;
  closeEditors();
  trigger = button;
  edited = user;
  const form = el('user-editor-form');
  form.querySelector('[role="alert"]').textContent = '';
  form.elements.displayName.value = user?.displayName || '';
  form.elements.email.value = user?.email || '';
  form.elements.role.value = user?.role || 'Learner';
  form.elements.isActive.checked = user?.isActive ?? true;
  form.elements.role.disabled = form.elements.isActive.disabled = user?.id === authState.user.id;
  for (const name of ['password', 'confirmPassword']) {
    form.elements[name].disabled = !!user;
    form.elements[name].required = !user;
  }
  el('user-initial-password').hidden = el('user-initial-confirm').hidden = !!user;
  el('user-editor-heading').textContent = user ? 'Sửa tài khoản' : 'Thêm tài khoản';
  form.querySelector('[type="submit"]').textContent = user ? 'Lưu thay đổi' : 'Tạo tài khoản';
  el('user-editor').hidden = false;
  el('user-editor-heading').focus();
  el('user-editor').scrollIntoView({ block: 'nearest' });
}
function openPassword(user, button) {
  if (busy) return;
  if (user.id === authState.user.id) { window.navigate('account'); return; }
  closeEditors();
  trigger = button;
  resetTarget = user;
  el('user-reset-form').querySelector('[role="alert"]').textContent = '';
  el('user-password-target').textContent = user.displayName + ' · ' + user.email;
  el('user-password-editor').hidden = false;
  el('user-password-heading').focus();
  el('user-password-editor').scrollIntoView({ block: 'nearest' });
}
export async function initUsersAdmin() {
  closeEditors();
  page = 1;
  search = el('users-search-form').elements.search.value.trim();
  el('users-add').onclick = e => openEditor(null, e.currentTarget);
  el('user-editor-cancel').onclick = el('user-password-cancel').onclick = closeEditors;
  el('user-delete-cancel').onclick = () => { if (!busy) el('user-delete-dialog').close(); };
  el('user-delete-dialog').oncancel = e => { if (busy) e.preventDefault(); };
  el('user-editor-form').onsubmit = e => {
    e.preventDefault();
    const form = e.currentTarget, user = edited;
    const data = { email: form.elements.email.value, displayName: form.elements.displayName.value, role: form.elements.role.value, isActive: form.elements.isActive.checked };
    const password = form.elements.password.value, confirmation = form.elements.confirmPassword.value;
    mutate(form, async () => {
      if (!user && password !== confirmation) throw new Error('Hai mật khẩu chưa trùng nhau.');
      if (user) await api.updateUser(user.id, data);
      else await api.createUser({ ...data, password });
    }, async () => {
      if (user?.id === authState.user.id) { location.assign('/login?updated=1'); return; }
      closeEditors();
      search = data.email.trim(); page = 1;
      el('users-search-form').elements.search.value = search;
      el('users-notice').textContent = user ? 'Đã cập nhật tài khoản. Các phiên cũ đã được thu hồi.' : 'Đã tạo tài khoản. Người dùng có thể đăng nhập bằng mật khẩu vừa đặt.';
      await loadUsers();
    });
  };
  el('user-reset-form').onsubmit = e => {
    e.preventDefault();
    const form = e.currentTarget, user = resetTarget;
    const password = form.elements.newPassword.value, confirmation = form.elements.confirmPassword.value;
    mutate(form, async () => {
      if (password !== confirmation) throw new Error('Hai mật khẩu mới chưa trùng nhau.');
      await api.resetUserPassword(user.id, password);
    }, () => { closeEditors(); el('users-notice').textContent = 'Đã đặt lại mật khẩu cho ' + user.email + '. Các phiên cũ đã được thu hồi.'; });
  };
  el('user-delete-form').onsubmit = e => {
    e.preventDefault();
    const user = deleteTarget;
    mutate(e.currentTarget, () => api.deleteUser(user.id), async () => {
      el('user-delete-dialog').close();
      closeEditors();
      el('users-notice').textContent = 'Đã xóa tài khoản ' + user.email + '.';
      await loadUsers();
    });
  };
  el('users-search-form').onsubmit = e => { e.preventDefault(); if (busy) return; search = e.currentTarget.elements.search.value.trim(); page = 1; loadUsers(); };
  el('users-prev').onclick = () => { if (page > 1 && !loading && !busy) { page--; loadUsers(); } };
  el('users-next').onclick = () => { if (!loading && !busy) { page++; loadUsers(); } };
  el('users-retry').onclick = loadUsers;
  await loadUsers();
}
function button(text, click, danger = false) {
  const b = document.createElement('button');
  b.type = 'button';
  b.className = danger ? 'btn btn-ghost btn-sm user-delete-action' : 'btn btn-secondary btn-sm';
  b.textContent = text;
  b.onclick = () => click(b);
  return b;
}
async function loadUsers() {
  if (loading) return;
  loading = true;
  const rows = el('users-rows'), status = el('users-status'), prev = el('users-prev'), next = el('users-next');
  const submit = el('users-search-form').querySelector('[type="submit"]');
  status.textContent = 'Đang tải tài khoản…';
  rows.replaceChildren();
  prev.disabled = next.disabled = submit.disabled = true;
  el('users-retry').hidden = true;
  try {
    let users = await api.getUsers({ search, page, limit });
    if (!users.length && page > 1) { page--; users = await api.getUsers({ search, page, limit }); }
    status.textContent = users.length ? users.length + ' tài khoản · Trang ' + page : 'Không có tài khoản phù hợp.';
    for (const user of users) {
      const row = document.createElement('tr'), name = document.createElement('td'), strong = document.createElement('strong'), email = document.createElement('span');
      strong.textContent = user.displayName; email.textContent = user.email; email.className = 'user-email'; name.append(strong, email);
      const role = document.createElement('td'); role.textContent = user.role === 'Admin' ? 'Quản trị viên' : 'Học viên';
      const active = document.createElement('td'); active.textContent = user.isActive ? 'Hoạt động' : 'Đã khóa';
      const cell = document.createElement('td'), actions = document.createElement('div'); actions.className = 'user-row-actions';
      actions.append(button('Sửa', b => openEditor(user, b)), button(user.id === authState.user.id ? 'Đổi mật khẩu' : 'Đặt lại mật khẩu', b => openPassword(user, b)));
      const remove = button('Xóa', b => {
        if (busy) return;
        deleteTarget = user; trigger = b;
        el('user-delete-target').textContent = user.displayName + ' · ' + user.email;
        el('user-delete-form').querySelector('[role="alert"]').textContent = '';
        el('user-delete-dialog').showModal();
      }, true);
      remove.disabled = user.id === authState.user.id;
      remove.setAttribute('aria-label', 'Xóa tài khoản ' + user.email);
      actions.append(remove); cell.append(actions); row.append(name, role, active, cell); rows.append(row);
    }
    prev.disabled = page <= 1; next.disabled = users.length < limit;
  } catch (error) { status.textContent = error.message; el('users-retry').hidden = false; }
  finally { submit.disabled = false; loading = false; }
}
