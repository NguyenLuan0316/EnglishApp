using System.Security.Cryptography;
using System.Text;
using WordWave.Application.Contracts.Patterns;
using WordWave.Application.Contracts.Review;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class LearningService : ILearningService
{
    private readonly IVocabularyRepository _vocabulary;
    private readonly IGrammarRepository _grammar;
    private readonly IPatternRepository _patterns;
    private readonly IUserLearningRepository _learning;
    private readonly ICurrentUser _user;
    private readonly TimeProvider _time;
    private static readonly Dictionary<string, int> DailyLimits = new() { ["vocabulary"] = 10, ["grammar"] = 2, ["patterns"] = 3 };

    public LearningService(IVocabularyRepository vocabulary, IGrammarRepository grammar, IPatternRepository patterns, IUserLearningRepository learning, ICurrentUser user, TimeProvider time)
    {
        _vocabulary = vocabulary;
        _grammar = grammar;
        _patterns = patterns;
        _learning = learning;
        _user = user;
        _time = time;
    }

    public static DateOnly StudyDate(DateTime utcNow) => StudySchedule.Date(utcNow);

    public async Task<DailyLearningDto> GetDailyAsync()
    {
        var catalog = await LoadCatalogAsync();
        var progress = await _learning.GetProgressAsync(_user.Id);
        return await GetPlanAsync(catalog, progress, _time.GetUtcNow().UtcDateTime);
    }

    public async Task<SubmitLearningResult> SubmitAsync(SubmitLearningRequest request)
    {
        if (!DailyLimits.ContainsKey(request.ContentType) || request.ContentId < 1)
        {
            throw new ArgumentException("Invalid learning content.");
        }

        var exists = request.ContentType switch
        {
            "vocabulary" => await _vocabulary.GetByIdAsync(request.ContentId) is not null,
            "grammar" => await _grammar.GetByIdAsync(request.ContentId) is not null,
            "patterns" => await _patterns.GetByIdAsync(request.ContentId) is not null,
            _ => false
        };
        if (!exists)
        {
            throw new KeyNotFoundException("Bài học không còn tồn tại trong thư viện.");
        }

        var recorded = await _learning.RecordAsync(_user.Id, request.ContentType, request.ContentId, request.Correct, request.Daily, _time.GetUtcNow().UtcDateTime);
        return new(recorded, await GetProgressAsync());
    }

    public async Task<LearningProgressDto> GetProgressAsync()
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var today = StudyDate(now);
        var catalog = await LoadCatalogAsync();
        var progress = await _learning.GetProgressAsync(_user.Id);
        var learned = progress.Where(x => x.CorrectCount > 0).Select(x => (x.ContentType, x.ContentId)).ToHashSet();
        var byContent = DailyLimits.Keys.ToDictionary(type => type, type => new LevelProgressDto(catalog.Count(x => x.Type == type), catalog.Count(x => x.Type == type && learned.Contains((x.Type, x.Id)))));
        var vocabulary = catalog.Where(x => x.Type == "vocabulary").ToList();
        var byLevel = vocabulary.GroupBy(x => x.Level).ToDictionary(g => g.Key, g => new LevelProgressDto(g.Count(), g.Count(x => learned.Contains((x.Type, x.Id)))));
        var byTopic = vocabulary.GroupBy(x => x.Topic).ToDictionary(g => g.Key, g => new LevelProgressDto(g.Count(), g.Count(x => learned.Contains((x.Type, x.Id)))));
        var plan = await GetPlanAsync(catalog, progress, now);
        var activity = await _learning.GetActivityAsync(_user.Id);
        var dates = activity.Where(x => x.Attempts > 0).Select(x => x.StudyDate).ToHashSet();
        var cursor = dates.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return new(today, byContent.Values.Sum(x => x.Learned), activity.Sum(x => x.Xp), streak, progress.Sum(x => x.CorrectCount + x.WrongCount), progress.Sum(x => x.CorrectCount), activity.FirstOrDefault(x => x.StudyDate == today)?.Attempts ?? 0, plan.Total, plan.Completed, byContent, byLevel, byTopic, activity.Where(x => x.StudyDate >= today.AddDays(-363) && x.StudyDate <= today).OrderBy(x => x.StudyDate).Select(x => new StudyActivityDto(x.StudyDate, x.Attempts, x.CorrectCount, x.Xp)).ToList());
    }

    private async Task<DailyLearningDto> GetPlanAsync(List<Content> catalog, List<UserContentProgress> progress, DateTime now)
    {
        var today = StudyDate(now);
        var known = progress.ToDictionary(x => (x.ContentType, x.ContentId));
        var candidates = new List<UserDailyLesson>();
        foreach (var (type, limit) in DailyLimits)
        {
            var eligible = catalog.Where(x => x.Type == type && (!known.TryGetValue((x.Type, x.Id), out var p) || p.NextReview <= now));
            var selected = eligible.OrderBy(x => known.ContainsKey((x.Type, x.Id)) ? 0 : 1).ThenBy(x => known.GetValueOrDefault((x.Type, x.Id))?.NextReview ?? DateTime.MaxValue).ThenBy(x => LevelRank(x.Level)).ThenBy(x => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{_user.Id}/{today:yyyy-MM-dd}/{x.Type}/{x.Id}")))).Take(limit);
            foreach (var content in selected)
            {
                candidates.Add(new UserDailyLesson { UserId = _user.Id, StudyDate = today, ContentType = content.Type, ContentId = content.Id, Position = candidates.Count, IsNew = !known.ContainsKey((content.Type, content.Id)) });
            }
        }

        var rows = await _learning.GetOrCreatePlanAsync(_user.Id, today, candidates);
        var lookup = catalog.ToDictionary(x => (x.Type, x.Id));
        var lessons = rows.OrderBy(x => x.Position).Where(x => lookup.ContainsKey((x.ContentType, x.ContentId))).Select(row =>
        {
            var content = lookup[(row.ContentType, row.ContentId)];
            return new DailyLessonDto(content.Type, content.Id, content.Title, content.Meaning, content.Detail, content.Level, content.Examples, row.IsNew, row.CompletedAt.HasValue, row.Correct);
        }).ToList();
        return new(today, lessons.Count, lessons.Count(x => x.Completed), lessons);
    }

    private async Task<List<Content>> LoadCatalogAsync()
    {
        // These repositories share one EF context, so reads must remain sequential.
        var words = await _vocabulary.GetAllAsync();
        var grammar = await _grammar.GetAllAsync();
        var patterns = await _patterns.GetAllAsync(new PatternQuery(null, null));
        return words.Select(x => new Content("vocabulary", x.Id, x.Word, x.Meaning, x.Phonetic, x.Level, x.Topic, new[] { x.Example, x.ExampleMeaning }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()))
            .Concat(grammar.Select(x => new Content("grammar", x.Id, x.Title, x.Description, $"{x.Formula}\n{x.Tips}".Trim(), x.Level, "", x.GrammarExamples.SelectMany(e => new[] { e.En, e.Vi }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray())))
            .Concat(patterns.Select(x => new Content("patterns", x.Id, x.Sentence, x.Explanation, x.Meaning, "", "", x.Examples)))
            .ToList();
    }

    private static int LevelRank(string level) => level switch { "A1" => 0, "A2" => 1, "B1" => 2, "B2" => 3, "C1" => 4, _ => 5 };
    private sealed record Content(string Type, int Id, string Title, string Meaning, string Detail, string Level, string Topic, string[] Examples);
}
