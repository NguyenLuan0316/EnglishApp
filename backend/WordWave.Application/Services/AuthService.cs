using WordWave.Application.Contracts.Auth;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    public AuthService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<AuthResult<UserDto>> RegisterAsync(string email, string displayName, string password)
    {
        var user = new AppUser { Email = email.Trim(), NormalizedEmail = Normalize(email), DisplayName = displayName.Trim() };
        user.PasswordHash = _passwords.Hash(user, password);
        return await _users.AddAsync(user) ? new(ToDto(user), Status: 201) : new(null, "Email này đã được sử dụng.", 409);
    }

    public async Task<AuthResult<LoginSessionDto>> LoginAsync(string email, string password)
    {
        var user = await _users.GetByEmailAsync(Normalize(email));
        if (user is null || !user.IsActive || user.LockoutEnd > DateTime.UtcNow)
        {
            // Perform password hashing for unknown, disabled and locked accounts too.
            var dummy = new AppUser();
            dummy.PasswordHash = _passwords.Hash(dummy, "WordWave timing equalization");
            _passwords.Verify(dummy, password);
            return new(null, "Email hoặc mật khẩu không đúng, hoặc tài khoản tạm thời không khả dụng.", 401);
        }

        if (!_passwords.Verify(user, password))
        {
            await _users.RecordFailedLoginAsync(user.Id);
            return new(null, "Email hoặc mật khẩu không đúng, hoặc tài khoản tạm thời không khả dụng.", 401);
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        await _users.SaveAsync();
        var session = new UserSession { UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddHours(8) };
        await _users.AddSessionAsync(session);
        return new(new(session.Id, ToDto(user), session.ExpiresAt));
    }

    public async Task<UserDto?> ValidateSessionAsync(Guid sessionId)
    {
        var session = await _users.GetSessionAsync(sessionId);
        return session is not null && session.ExpiresAt > DateTime.UtcNow && session.User.IsActive ? ToDto(session.User) : null;
    }

    public Task LogoutAsync(Guid sessionId) => _users.DeleteSessionAsync(sessionId);

    public async Task<AuthResult<UserDto>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null || !_passwords.Verify(user, currentPassword))
        {
            return new(null, "Mật khẩu hiện tại không đúng.", 400);
        }
        await _users.SetPasswordAsync(userId, _passwords.Hash(user, newPassword));
        return new(ToDto(user));
    }

    public async Task<List<UserDto>> GetUsersAsync(string? search, int page, int limit) => (await _users.GetUsersAsync(search, page, limit)).Select(ToDto).ToList();

    public async Task<AuthResult<UserDto>> SetAccessAsync(Guid actorId, Guid userId, string role, bool isActive)
    {
        if (!UserRoles.IsValid(role))
        {
            return new(null, "Vai trò không hợp lệ.", 400);
        }
        if (actorId == userId && (role != UserRoles.Admin || !isActive))
        {
            return new(null, "Bạn không thể tự khóa hoặc hạ quyền tài khoản đang đăng nhập.", 409);
        }
        if (await _users.GetByIdAsync(userId) is null)
        {
            return new(null, "Không tìm thấy tài khoản.", 404);
        }
        if (!await _users.UpdateAccessAsync(userId, role, isActive))
        {
            return new(null, "Cần giữ lại ít nhất một quản trị viên đang hoạt động.", 409);
        }
        return new(ToDto((await _users.GetByIdAsync(userId))!));
    }

    public async Task BootstrapAdminAsync(string email, string displayName, string password)
    {
        if (await _users.GetByEmailAsync(Normalize(email)) is not null)
        {
            return;
        }
        var user = new AppUser { Email = email.Trim(), NormalizedEmail = Normalize(email), DisplayName = displayName, Role = UserRoles.Admin };
        user.PasswordHash = _passwords.Hash(user, password);
        await _users.AddAsync(user);
    }

    private static string Normalize(string email) => email.Trim().ToUpperInvariant();
    public async Task<AuthResult<UserDto>> CreateUserAsync(string email, string displayName, string password, string role, bool isActive)
    {
        if (!UserRoles.IsValid(role)) return new(null, "Vai trò không hợp lệ.", 400);
        var user = new AppUser { Email = email.Trim(), NormalizedEmail = Normalize(email), DisplayName = displayName.Trim(), Role = role, IsActive = isActive };
        user.PasswordHash = _passwords.Hash(user, password);
        return await _users.AddAsync(user) ? new(ToDto(user), Status: 201) : new(null, "Email này đã được sử dụng.", 409);
    }

    public async Task<AuthResult<UserDto>> UpdateUserAsync(Guid actorId, Guid userId, string email, string displayName, string role, bool isActive)
    {
        if (!UserRoles.IsValid(role)) return new(null, "Vai trò không hợp lệ.", 400);
        if (actorId == userId && (role != UserRoles.Admin || !isActive)) return new(null, "Bạn không thể tự khóa hoặc hạ quyền tài khoản đang đăng nhập.", 409);
        var status = await _users.UpdateProfileAsync(userId, email, displayName, role, isActive);
        return status switch
        {
            UserMutationStatus.NotFound => new(null, "Không tìm thấy tài khoản.", 404),
            UserMutationStatus.DuplicateEmail => new(null, "Email này đã được sử dụng.", 409),
            UserMutationStatus.LastAdmin => new(null, "Cần giữ lại ít nhất một quản trị viên đang hoạt động.", 409),
            _ => new(ToDto((await _users.GetByIdAsync(userId))!))
        };
    }

    public async Task<AuthResult<UserDto>> DeleteUserAsync(Guid actorId, Guid userId)
    {
        if (actorId == userId) return new(null, "Bạn không thể xóa tài khoản đang đăng nhập.", 409);
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return new(null, "Không tìm thấy tài khoản.", 404);
        var status = await _users.DeleteAsync(userId);
        return status switch
        {
            UserMutationStatus.NotFound => new(null, "Không tìm thấy tài khoản.", 404),
            UserMutationStatus.LastAdmin => new(null, "Không thể xóa quản trị viên hoạt động cuối cùng.", 409),
            _ => new(ToDto(user))
        };
    }

    public async Task<AuthResult<UserDto>> ResetPasswordAsync(Guid actorId, Guid userId, string newPassword)
    {
        if (actorId == userId) return new(null, "Hãy dùng chức năng Đổi mật khẩu cá nhân cho tài khoản của bạn.", 409);
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return new(null, "Không tìm thấy tài khoản.", 404);
        return await _users.SetPasswordAsync(userId, _passwords.Hash(user, newPassword)) ? new(ToDto(user)) : new(null, "Không tìm thấy tài khoản.", 404);
    }

    private static UserDto ToDto(AppUser user) => new(user.Id, user.Email, user.DisplayName, user.Role, user.IsActive, user.CreatedAt);
}
