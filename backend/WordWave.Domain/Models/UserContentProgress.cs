namespace WordWave.Domain.Models;

public class UserContentProgress
{
    public Guid UserId { get; set; }
    public string ContentType { get; set; } = "";
    public int ContentId { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime NextReview { get; set; }
    public DateTime? LastReviewed { get; set; }
}
