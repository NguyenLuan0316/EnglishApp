using Microsoft.EntityFrameworkCore;
using Npgsql;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;
using WordWave.Application.Contracts.Auth;

namespace WordWave.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;
    public UserRepository(AuthDbContext db) => _db = db;
    public Task<AppUser?> GetByEmailAsync(string normalizedEmail) => _db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
    public Task<AppUser?> GetByIdAsync(Guid id) => _db.Users.SingleOrDefaultAsync(x => x.Id == id);

    public async Task<bool> AddAsync(AppUser user)
    {
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }

    public async Task SaveAsync() => await _db.SaveChangesAsync();
    public async Task RecordFailedLoginAsync(Guid userId)
    {
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        await _db.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(update => update.SetProperty(x => x.LockoutEnd, x => x.FailedLoginCount >= 4 ? lockoutEnd : x.LockoutEnd).SetProperty(x => x.FailedLoginCount, x => x.FailedLoginCount >= 4 ? 0 : x.FailedLoginCount + 1));
    }
    public async Task AddSessionAsync(UserSession session)
    {
        await _db.Sessions.Where(x => x.ExpiresAt <= DateTime.UtcNow).ExecuteDeleteAsync();
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
    }
    public Task<UserSession?> GetSessionAsync(Guid id) => _db.Sessions.AsNoTracking().Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
    public async Task DeleteSessionAsync(Guid id) => await _db.Sessions.Where(x => x.Id == id).ExecuteDeleteAsync();
    public async Task DeleteUserSessionsAsync(Guid userId) => await _db.Sessions.Where(x => x.UserId == userId).ExecuteDeleteAsync();
    public Task<List<AppUser>> GetUsersAsync(string? search, int page, int limit)
    {
        var query = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.NormalizedEmail.Contains(term) || x.DisplayName.ToUpper().Contains(term));
        }
        return query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip((page - 1) * limit).Take(limit).ToListAsync();
    }

    public async Task<bool> UpdateAccessAsync(Guid userId, string role, bool isActive)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // Serialize access changes so concurrent requests cannot remove the last admin.
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(81357041)");
        var user = await _db.Users.SingleAsync(x => x.Id == userId);
        await _db.Entry(user).ReloadAsync();
        if (user.Role == UserRoles.Admin && user.IsActive && (role != UserRoles.Admin || !isActive) && await _db.Users.CountAsync(x => x.Role == UserRoles.Admin && x.IsActive) <= 1)
        {
            return false;
        }
        user.Role = role;
        user.IsActive = isActive;
        await _db.SaveChangesAsync();
        await DeleteUserSessionsAsync(userId);
        await transaction.CommitAsync();
        return true;
    }

    public async Task<UserMutationStatus> UpdateProfileAsync(Guid userId, string email, string displayName, string role, bool isActive)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(81357041)");
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user is null) return UserMutationStatus.NotFound;
        await _db.Entry(user).ReloadAsync();
        if (user.Role == UserRoles.Admin && user.IsActive && (role != UserRoles.Admin || !isActive) && await _db.Users.CountAsync(x => x.Role == UserRoles.Admin && x.IsActive) <= 1) return UserMutationStatus.LastAdmin;
        user.Email = email.Trim();
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.DisplayName = displayName.Trim();
        user.Role = role;
        user.IsActive = isActive;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return UserMutationStatus.DuplicateEmail;
        }
        await DeleteUserSessionsAsync(userId);
        await transaction.CommitAsync();
        return UserMutationStatus.Success;
    }

    public async Task<UserMutationStatus> DeleteAsync(Guid userId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(81357041)");
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);
        if (user is null) return UserMutationStatus.NotFound;
        if (user.Role == UserRoles.Admin && user.IsActive && await _db.Users.CountAsync(x => x.Role == UserRoles.Admin && x.IsActive) <= 1) return UserMutationStatus.LastAdmin;
        var learnerId = userId.ToString();
        await _db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ielts_attempts WHERE learner_id = {learnerId}");
        await _db.Users.Where(x => x.Id == userId).ExecuteDeleteAsync();
        await transaction.CommitAsync();
        return UserMutationStatus.Success;
    }

    public async Task<bool> SetPasswordAsync(Guid userId, string passwordHash)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var count = await _db.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(update => update.SetProperty(x => x.PasswordHash, passwordHash).SetProperty(x => x.FailedLoginCount, 0).SetProperty(x => x.LockoutEnd, (DateTime?)null));
        await DeleteUserSessionsAsync(userId);
        await transaction.CommitAsync();
        return count > 0;
    }
}
