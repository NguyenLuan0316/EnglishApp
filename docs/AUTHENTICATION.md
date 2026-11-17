# Authentication and access control

WordWave uses PostgreSQL accounts with two roles: `Admin` and `Learner`. Authentication is enforced by the API, including for direct HTTP requests. All learning routes require a signed-in user. Only health, CSRF-token retrieval, registration and login allow anonymous requests.

## Use locally

1. Start the local PostgreSQL cluster and run the API in Development. The current local database is `wordwave_test` on `127.0.0.1:55432`.
2. Open `http://localhost:5500/login`. New registrations always receive the Learner role; clients cannot select their own role.
3. Sign in as an administrator and open **Quản trị → Quản lý tài khoản**. Use **Thêm tài khoản** to set the name, email, initial password, role and active status. Each row has **Sửa**, **Đặt lại mật khẩu** and **Xóa** actions. Editing updates name/email/role/status; password reset uses a separate form with confirmation. A delete dialog names the selected account and explains that its sessions, review progress and IELTS attempts are permanently removed.
4. Use the user icon and signed-in name at the top right to open **Đổi mật khẩu** or **Đăng xuất**. The dropdown also identifies the account email and role. It closes on outside click, Escape or focus leaving; keyboard users can use Tab or arrow keys. Password change remains available on your own row and requires the current password. Signing out ends the current session. Password changes/resets, account edits and access changes revoke the affected account's sessions. Updating your own profile redirects to login with the updated email. An administrator cannot delete their own account, reset their own password without the current password, or remove the last active administrator.

During this local implementation, an initial admin was provisioned as `admin@wordwave.local`. Its generated password is stored only on this machine in `%LOCALAPPDATA%\WordWave\local-admin.json`. No administrator password is checked into source. The `preview.learner@wordwave.test` account is a local UI-test learner, not a production fixture.

## Provision accounts in another environment

Apply only the authentication context's reviewed migrations to the intended database:

```powershell
dotnet ef database update --context AuthDbContext --project backend/WordWave.Infrastructure --startup-project backend/WordWave.Api
```

Authentication uses `AuthDbContext` and `__AuthMigrationsHistory` independently from the existing content `AppDbContext` migration chain, whose initial content baseline is incomplete. The authentication migration adds `app_users`, `user_sessions` and `user_word_progress` without recreating learning-content tables. Specify `--context AppDbContext` explicitly for future content migrations; specify `--context AuthDbContext` for account/progress migrations. Startup does not automatically apply migrations.

To provision the first administrator, supply `Auth__AdminEmail` and `Auth__AdminPassword` from a secret store or the process environment before launching the API. The password must contain 10–128 characters. Bootstrap creates a new admin only when the email does not exist; it never promotes an existing learner or resets a password on restart. Remove the bootstrap settings after provisioning. There is no default production admin password.

## API contract

| Method | Path | Access / behavior |
| --- | --- | --- |
| GET | `/api/auth/csrf` | Anonymous; returns `{ token }` and sets the verification cookie |
| POST | `/api/auth/register` | Anonymous; `{ email, displayName, password }`; creates Learner only |
| POST | `/api/auth/login` | Anonymous; `{ email, password }`; returns the user and sets an HttpOnly session cookie |
| GET | `/api/auth/me` | Current signed-in user |
| POST | `/api/auth/logout` | Revokes current server session and clears its cookie |
| PUT | `/api/auth/password` | `{ currentPassword, newPassword }`; revokes all account sessions |
| GET | `/api/admin/users` | Admin; `search`, `page` (default 1), `limit` (default 20, maximum 100) |
| POST | `/api/admin/users` | Admin; `{ email, displayName, password, role, isActive }`; creates account |
| PUT | `/api/admin/users/{id}` | Admin; `{ email, displayName, role, isActive }`; updates profile and access |
| DELETE | `/api/admin/users/{id}` | Admin; permanently removes account and its owned learning data |
| PUT | `/api/admin/users/{id}/password` | Admin; `{ newPassword }`; resets another account's password, clears login lockout, revokes sessions |
| PUT | `/api/admin/users/{id}/access` | Admin; `{ role: "Admin" \| "Learner", isActive: boolean }` |
| All | `/api/admin/toeic/*` | Admin only |
| All other learning APIs | `/api/*` | Signed-in Admin or Learner |

For every POST/PUT/PATCH/DELETE, obtain a CSRF token and send it in `X-CSRF-TOKEN`, with the cookie received from the same client session. Fetch a new token after login/logout/password changes because the identity binding changes. The frontend does this automatically and sends `credentials: 'include'` on all requests. Missing/invalid authentication returns 401, insufficient role or invalid CSRF returns 403, validation errors return 400 and duplicate email returns 409. A normal login failure uses a generic message for nonexistent, disabled or locked accounts.

Emails are normalized for a database-enforced case-insensitive unique key. Passwords use ASP.NET Core PasswordHasher (PBKDF2, random salt, 210,000 iterations). Password hashes and session identifiers are never returned in user DTOs. Cookies are encrypted/authenticated by ASP.NET Core Data Protection, HttpOnly and limited to eight hours. Session validity and active status are checked against PostgreSQL on each request. Expired sessions are removed during subsequent logins.

Login/registration share a 10-requests-per-minute limiter per network address. Five failed password attempts lock an existing account for 15 minutes. Administrators cannot disable/demote their own current account. Access changes are serialized in PostgreSQL to prevent removing the last active administrator.

## Learner ownership and persistence

IELTS attempt ownership comes exclusively from the authenticated account ID; legacy caller-supplied `learnerId` values are ignored. Historical `default` attempts are not automatically assigned to a new user. Review progress is now stored in `user_word_progress`, per account, and survives API restarts. Review scheduling retains the previous 0/1/3/7-day rules. Vocabulary existence is checked before recording progress.

Browser XP, streak and level keys are namespaced by account. XP remains browser-local; server review progress and IELTS attempts are durable. Authentication transitions reload the application so cached practice state and timers are not transferred between users.

## Deployment considerations

Production requires HTTPS. Session/CSRF cookies use Secure + SameSite=None outside Development to support the existing separate frontend/API hosts; browsers that block third-party cookies require hosting the frontend and API on the same site or a same-site reverse proxy. CORS permits credentials only for the existing explicit origin list. Use the same local hostname for frontend and API; the API client automatically follows `localhost` or `127.0.0.1`.

Persist and protect the ASP.NET Core Data Protection key ring across deployments and share it when running multiple API instances. Configure the connection string and bootstrap secrets for the intended environment. This change was verified locally and has not been deployed to Render/Netlify. Email verification, email-based password recovery and MFA are not included.

## Verification

```powershell
dotnet build backend/WordWave.sln
dotnet test backend/WordWave.sln
```

The API integration suite creates a uniquely named disposable PostgreSQL database on localhost, initializes content plus auth schema, runs HTTP/session/role/CSRF/ownership checks, and drops only that database afterward. The configured local database user needs CREATE DATABASE permission. Set `WORDWAVE_TEST_CONNECTION` to select a different local PostgreSQL test server; remote hosts are rejected. Tests do not use or mutate application accounts in `wordwave_test`. The shared test fixture disables only the interactive login rate limiter, leaving account lockout and all authentication/authorization checks enabled.

Browser verification covered login/logout, learner rejection on admin routes, account view, admin user listing/search/save, direct nested-route refresh, and small/large viewports. Contrast of the shared muted-text token was increased for readability. Existing decorative palette/glow and older small-label warnings remain outside this authentication change; no detector warnings were suppressed.

The live local API was also checked with its actual rate limiter: ten failed logins returned 401 and the eleventh returned 429. Credentialed CORS was allowed for the configured local frontend and denied for an untrusted origin. The integration lockout test submits five failed attempts concurrently to verify the atomic failure counter.

Account-management integration tests additionally cover admin creation, invalid role/password input, case-insensitive duplicate emails on create/edit, updated-email login, immediate session revocation on edit/reset/delete, old/new passwords after reset, self-deletion rejection, missing targets, full removal of owned IELTS/review/session records, reusing a deleted email, and learner denial on every management endpoint. Deletion and password reset use database transactions. No schema change is needed for these CRUD operations.
