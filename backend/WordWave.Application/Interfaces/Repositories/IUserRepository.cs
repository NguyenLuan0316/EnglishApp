using WordWave.Domain.Models;
using WordWave.Application.Contracts.Auth;

namespace WordWave.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string normalizedEmail);
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<bool> AddAsync(AppUser user);
    Task SaveAsync();
    Task RecordFailedLoginAsync(Guid userId);
    Task AddSessionAsync(UserSession session);
    Task<UserSession?> GetSessionAsync(Guid id);
    Task DeleteSessionAsync(Guid id);
    Task DeleteUserSessionsAsync(Guid userId);
    Task<List<AppUser>> GetUsersAsync(string? search, int page, int limit);
    Task<bool> UpdateAccessAsync(Guid userId, string role, bool isActive);
    Task<UserMutationStatus> UpdateProfileAsync(Guid userId, string email, string displayName, string role, bool isActive);
    Task<UserMutationStatus> DeleteAsync(Guid userId);
    Task<bool> SetPasswordAsync(Guid userId, string passwordHash);
}
