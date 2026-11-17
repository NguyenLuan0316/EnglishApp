namespace WordWave.Domain.Models;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AppUser User { get; set; } = null!;
}
