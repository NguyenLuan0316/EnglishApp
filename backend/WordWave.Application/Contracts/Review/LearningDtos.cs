namespace WordWave.Application.Contracts.Review;

public sealed record DailyLessonDto(string ContentType, int ContentId, string Title, string Meaning, string Detail, string Level, string[] Examples, bool IsNew, bool Completed, bool? Correct);
public sealed record DailyLearningDto(DateOnly Date, int Total, int Completed, IReadOnlyList<DailyLessonDto> Lessons);
public sealed record StudyActivityDto(DateOnly Date, int Attempts, int CorrectCount, int Xp);
public sealed record LearningProgressDto(DateOnly Date, int TotalLearned, int TotalXp, int Streak, int Attempts, int CorrectCount, int TodayAttempts, int DailyTotal, int DailyCompleted, IReadOnlyDictionary<string, LevelProgressDto> ByContent, IReadOnlyDictionary<string, LevelProgressDto> ByLevel, IReadOnlyDictionary<string, LevelProgressDto> ByTopic, IReadOnlyList<StudyActivityDto> Activity);
public sealed record SubmitLearningResult(bool Recorded, LearningProgressDto Summary);
