namespace WordWave.Domain.Models;

public class UserStudyActivity
{
    public Guid UserId { get; set; }
    public DateOnly StudyDate { get; set; }
    public int Attempts { get; set; }
    public int CorrectCount { get; set; }
    public int Xp { get; set; }
}
