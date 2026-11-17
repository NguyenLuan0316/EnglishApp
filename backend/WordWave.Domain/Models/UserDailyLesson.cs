namespace WordWave.Domain.Models;

public class UserDailyLesson
{
    public Guid UserId { get; set; }
    public DateOnly StudyDate { get; set; }
    public string ContentType { get; set; } = "";
    public int ContentId { get; set; }
    public int Position { get; set; }
    public bool IsNew { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool? Correct { get; set; }
}
