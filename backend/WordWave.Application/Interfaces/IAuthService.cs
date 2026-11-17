using WordWave.Application.Contracts.Auth;

namespace WordWave.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult<UserDto>> RegisterAsync(string email, string displayName, string password);
    Task<AuthResult<LoginSessionDto>> LoginAsync(string email, string password);
    Task<UserDto?> ValidateSessionAsync(Guid sessionId);
    Task LogoutAsync(Guid sessionId);
    Task<AuthResult<UserDto>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<List<UserDto>> GetUsersAsync(string? search, int page, int limit);
    Task<AuthResult<UserDto>> SetAccessAsync(Guid actorId, Guid userId, string role, bool isActive);
    Task BootstrapAdminAsync(string email, string displayName, string password);
    Task<AuthResult<UserDto>> CreateUserAsync(string email, string displayName, string password, string role, bool isActive);
    Task<AuthResult<UserDto>> UpdateUserAsync(Guid actorId, Guid userId, string email, string displayName, string role, bool isActive);
    Task<AuthResult<UserDto>> DeleteUserAsync(Guid actorId, Guid userId);
    Task<AuthResult<UserDto>> ResetPasswordAsync(Guid actorId, Guid userId, string newPassword);
}
