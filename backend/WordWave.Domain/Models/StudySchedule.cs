namespace WordWave.Domain.Models;

public static class StudySchedule
{
    public static DateOnly Date(DateTime utcNow) => DateOnly.FromDateTime(utcNow.AddHours(7));

    public static DateTime NextReview(DateTime now, int correctCount, int wrongCount, bool correct)
    {
        if (!correct) { return now; }
        var ratio = (double)correctCount / Math.Max(1, correctCount + wrongCount);
        var days = ratio switch { >= 0.9 => 7, >= 0.7 => 3, >= 0.5 => 1, _ => 0 };
        return now.AddDays(days);
    }
}
