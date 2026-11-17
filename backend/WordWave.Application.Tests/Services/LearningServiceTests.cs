using WordWave.Application.Contracts.Patterns;
using WordWave.Application.Contracts.Review;
using WordWave.Application.Contracts.Vocabulary;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Application.Services;
using WordWave.Domain.Models;
using Xunit;

namespace WordWave.Application.Tests.Services;

public class LearningServiceTests
{
    [Fact]
    public async Task DailyPlanContainsAllSourcesAndRemainsStableAfterLearning()
    {
        var fixture = new Fixture();
        var service = fixture.Service();
        var plan = await service.GetDailyAsync();
        Assert.Equal(15, plan.Total);
        Assert.Equal(10, plan.Lessons.Count(x => x.ContentType == "vocabulary"));
        Assert.Equal(2, plan.Lessons.Count(x => x.ContentType == "grammar"));
        Assert.Equal(3, plan.Lessons.Count(x => x.ContentType == "patterns"));
        var first = plan.Lessons[0];
        var result = await service.SubmitAsync(new(first.ContentType, first.ContentId, true, true));
        Assert.Equal(1, result.Summary.DailyCompleted);
        Assert.Equal(1, result.Summary.TotalLearned);
        Assert.Equal(10, result.Summary.TotalXp);
        Assert.Equal(plan.Lessons.Select(x => (x.ContentType, x.ContentId)), (await service.GetDailyAsync()).Lessons.Select(x => (x.ContentType, x.ContentId)));
        var repeat = await service.SubmitAsync(new(first.ContentType, first.ContentId, true, true));
        Assert.False(repeat.Recorded);
        Assert.Equal(10, repeat.Summary.TotalXp);
    }

    [Fact]
    public async Task DueContentPrecedesNewContentAndFutureReviewsAreExcluded()
    {
        var fixture = new Fixture();
        fixture.Store.Progress.Add(new() { UserId = fixture.User, ContentType = "vocabulary", ContentId = 20, CorrectCount = 1, NextReview = fixture.Clock.Now.AddDays(-1) });
        fixture.Store.Progress.Add(new() { UserId = fixture.User, ContentType = "vocabulary", ContentId = 1, CorrectCount = 1, NextReview = fixture.Clock.Now.AddDays(1) });
        var plan = await fixture.Service().GetDailyAsync();
        Assert.Equal(20, plan.Lessons[0].ContentId);
        Assert.False(plan.Lessons[0].IsNew);
        Assert.DoesNotContain(plan.Lessons, x => x.ContentType == "vocabulary" && x.ContentId == 1);
        Assert.All(plan.Lessons.Where(x => x.ContentType == "grammar"), x => Assert.Equal("A1", x.Level));
    }

    [Fact]
    public async Task ProgressAndPlansAreScopedToAuthenticatedAccount()
    {
        var fixture = new Fixture();
        var first = fixture.Service();
        var plan = await first.GetDailyAsync();
        var lesson = plan.Lessons.First(x => x.ContentType == "grammar");
        await first.SubmitAsync(new(lesson.ContentType, lesson.ContentId, true, true));
        var other = fixture.Service(Guid.NewGuid());
        var summary = await other.GetProgressAsync();
        Assert.Equal(0, summary.TotalLearned);
        Assert.Equal(0, summary.TotalXp);
        Assert.Equal(0, summary.DailyCompleted);
        Assert.Equal(0, summary.Streak);
        Assert.Equal(1, (await first.GetProgressAsync()).ByContent["grammar"].Learned);
    }

    [Fact]
    public async Task DayRolloverCreatesFreshPlanAndStreakUsesLocalStudyDays()
    {
        var fixture = new Fixture();
        fixture.Clock.Now = new DateTime(2026, 11, 16, 16, 59, 0, DateTimeKind.Utc);
        var service = fixture.Service();
        var dayOne = await service.GetDailyAsync();
        await service.SubmitAsync(new(dayOne.Lessons[0].ContentType, dayOne.Lessons[0].ContentId, true, true));
        fixture.Clock.Now = fixture.Clock.Now.AddMinutes(2);
        var dayTwo = await service.GetDailyAsync();
        Assert.Equal(new DateOnly(2026, 11, 17), dayTwo.Date);
        Assert.Equal(0, dayTwo.Completed);
        Assert.Equal(1, (await service.GetProgressAsync()).Streak);
        await service.SubmitAsync(new(dayTwo.Lessons[0].ContentType, dayTwo.Lessons[0].ContentId, true, true));
        Assert.Equal(2, (await service.GetProgressAsync()).Streak);
        fixture.Clock.Now = fixture.Clock.Now.AddDays(2);
        Assert.Equal(0, (await service.GetProgressAsync()).Streak);
    }

    [Fact]
    public async Task MissingContentAndUnplannedDailySubmissionsDoNotCreateProgress()
    {
        var fixture = new Fixture();
        var service = fixture.Service();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SubmitAsync(new("grammar", 99999, true)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(new("grammar", 1, true, true)));
        Assert.Empty(fixture.Store.Progress);
        var plan = await service.GetDailyAsync();
        fixture.Catalog.Words.Clear();
        fixture.Catalog.Grammar.Clear();
        fixture.Catalog.Patterns.Clear();
        var empty = await service.GetDailyAsync();
        Assert.Equal(0, empty.Total);
        Assert.Equal(0, (await service.GetProgressAsync()).TotalLearned);
    }

    [Fact]
    public void IncorrectAnswerIsDueImmediatelyEvenAfterManyCorrectAnswers()
    {
        var now = DateTime.UtcNow;
        Assert.Equal(now, StudySchedule.NextReview(now, 20, 1, false));
        Assert.Equal(now.AddDays(7), StudySchedule.NextReview(now, 1, 0, true));
    }

    private sealed class Fixture
    {
        public Guid User { get; } = Guid.NewGuid();
        public FakeTime Clock { get; } = new();
        public Catalog Catalog { get; } = new();
        public Store Store { get; } = new();
        public LearningService Service(Guid? user = null) => new(Catalog, Catalog, Catalog, Store, new Current(user ?? User), Clock);
    }

    private sealed class FakeTime : TimeProvider
    {
        public DateTime Now { get; set; } = new(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc);
        public override DateTimeOffset GetUtcNow() => new(Now);
    }
    private sealed record Current(Guid Id) : ICurrentUser;

    private sealed class Catalog : IVocabularyRepository, IGrammarRepository, IPatternRepository
    {
        public List<VocabWord> Words { get; } = Enumerable.Range(1, 20).Select(id => new VocabWord { Id = id, Word = $"word {id}", Meaning = "meaning", Level = "A1", Topic = "daily" }).ToList();
        public List<GrammarLesson> Grammar { get; } = Enumerable.Range(1, 5).Select(id => new GrammarLesson { Id = id, Title = $"Grammar {id}", Level = id <= 3 ? "A1" : "B1" }).ToList();
        public List<SentencePattern> Patterns { get; } = Enumerable.Range(1, 8).Select(id => new SentencePattern { Id = id, Sentence = $"Sentence {id}" }).ToList();
        Task<List<VocabWord>> IVocabularyRepository.GetAllAsync() => Task.FromResult(Words);
        Task<VocabWord?> IVocabularyRepository.GetByIdAsync(int id) => Task.FromResult(Words.FirstOrDefault(x => x.Id == id));
        Task<List<GrammarLesson>> IGrammarRepository.GetAllAsync() => Task.FromResult(Grammar);
        Task<GrammarLesson?> IGrammarRepository.GetByIdAsync(int id) => Task.FromResult(Grammar.FirstOrDefault(x => x.Id == id));
        public Task<List<SentencePattern>> GetAllAsync(PatternQuery query) => Task.FromResult(Patterns);
        Task<SentencePattern?> IPatternRepository.GetByIdAsync(int id) => Task.FromResult(Patterns.FirstOrDefault(x => x.Id == id));
        public Task<(int total, List<VocabWord> data)> GetPagedAsync(VocabularyQuery query) => Task.FromResult((Words.Count, Words));
        public Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10) => Task.FromResult(Words.Take(count).ToList());
        public Task<List<string>> GetTopicsAsync() => Task.FromResult(new List<string> { "daily" });
        public Task<List<VocabularyTopicSummaryDto>> GetTopicSummariesAsync(string? level) => Task.FromResult(new List<VocabularyTopicSummaryDto>());
    }

    private sealed class Store : IUserLearningRepository
    {
        public List<UserContentProgress> Progress { get; } = [];
        private readonly List<UserDailyLesson> _plans = [];
        private readonly List<UserStudyActivity> _activity = [];
        public Task<List<UserContentProgress>> GetProgressAsync(Guid userId) => Task.FromResult(Progress.Where(x => x.UserId == userId).ToList());
        public Task<List<UserStudyActivity>> GetActivityAsync(Guid userId) => Task.FromResult(_activity.Where(x => x.UserId == userId).ToList());
        public Task<List<UserDailyLesson>> GetOrCreatePlanAsync(Guid userId, DateOnly date, IReadOnlyList<UserDailyLesson> candidates)
        {
            var rows = _plans.Where(x => x.UserId == userId && x.StudyDate == date).ToList();
            if (rows.Count == 0) { rows = candidates.ToList(); _plans.AddRange(rows); }
            return Task.FromResult(rows);
        }
        public Task<bool> RecordAsync(Guid userId, string type, int id, bool correct, bool daily, DateTime now)
        {
            var date = StudySchedule.Date(now);
            if (daily)
            {
                var row = _plans.SingleOrDefault(x => x.UserId == userId && x.StudyDate == date && x.ContentType == type && x.ContentId == id) ?? throw new InvalidOperationException();
                if (row.CompletedAt.HasValue) return Task.FromResult(false);
                row.CompletedAt = now;
                row.Correct = correct;
            }
            var progress = Progress.SingleOrDefault(x => x.UserId == userId && x.ContentType == type && x.ContentId == id);
            if (progress is null) { progress = new() { UserId = userId, ContentType = type, ContentId = id }; Progress.Add(progress); }
            if (correct) progress.CorrectCount++; else progress.WrongCount++;
            progress.NextReview = StudySchedule.NextReview(now, progress.CorrectCount, progress.WrongCount, correct);
            progress.LastReviewed = now;
            var activity = _activity.SingleOrDefault(x => x.UserId == userId && x.StudyDate == date);
            if (activity is null) { activity = new() { UserId = userId, StudyDate = date }; _activity.Add(activity); }
            activity.Attempts++;
            if (correct) { activity.CorrectCount++; activity.Xp += 10; }
            return Task.FromResult(true);
        }
    }
}
