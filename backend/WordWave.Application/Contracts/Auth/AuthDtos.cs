namespace WordWave.Application.Contracts.Auth;

public sealed record UserDto(Guid Id, string Email, string DisplayName, string Role, bool IsActive, DateTime CreatedAt);
public sealed record LoginSessionDto(Guid SessionId, UserDto User, DateTime ExpiresAt);
public sealed record AuthResult<T>(T? Value, string? Error = null, int Status = 200) where T : class;
public enum UserMutationStatus { Success, NotFound, DuplicateEmail, LastAdmin }
