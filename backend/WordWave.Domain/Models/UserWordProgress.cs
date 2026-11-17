namespace WordWave.Domain.Models;

public class UserWordProgress
{
    public Guid UserId { get; set; }
    public int WordId { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime NextReview { get; set; }
    public DateTime? LastReviewed { get; set; }
}
