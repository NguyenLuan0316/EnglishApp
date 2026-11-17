using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class UserLearningRepository : IUserLearningRepository
{
    private readonly AuthDbContext _db;
    public UserLearningRepository(AuthDbContext db) => _db = db;

    public async Task<List<UserContentProgress>> GetProgressAsync(Guid userId)
    {
        var words = await _db.WordProgress.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        var content = await _db.ContentProgress.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        content.AddRange(words.Select(x => new UserContentProgress { UserId = x.UserId, ContentType = "vocabulary", ContentId = x.WordId, CorrectCount = x.CorrectCount, WrongCount = x.WrongCount, LastReviewed = x.LastReviewed, NextReview = x.NextReview }));
        return content;
    }

    public Task<List<UserStudyActivity>> GetActivityAsync(Guid userId) => _db.StudyActivity.AsNoTracking().Where(x => x.UserId == userId).OrderBy(x => x.StudyDate).ToListAsync();

    public async Task<List<UserDailyLesson>> GetOrCreatePlanAsync(Guid userId, DateOnly date, IReadOnlyList<UserDailyLesson> candidates)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await LockUserAsync(userId);
        var plan = await _db.DailyLessons.AsNoTracking().Where(x => x.UserId == userId && x.StudyDate == date).OrderBy(x => x.Position).ToListAsync();
        if (plan.Count == 0)
        {
            plan = candidates.Select(x => new UserDailyLesson { UserId = userId, StudyDate = date, ContentType = x.ContentType, ContentId = x.ContentId, Position = x.Position, IsNew = x.IsNew }).ToList();
            _db.DailyLessons.AddRange(plan);
            await _db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return plan;
    }

    public async Task<bool> RecordAsync(Guid userId, string contentType, int contentId, bool correct, bool daily, DateTime now)
    {
        if (contentType is not ("vocabulary" or "grammar" or "patterns")) { throw new ArgumentException("Unsupported learning content.", nameof(contentType)); }
        var date = StudySchedule.Date(now);
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await LockUserAsync(userId);
        if (daily)
        {
            var lesson = await _db.DailyLessons.SingleOrDefaultAsync(x => x.UserId == userId && x.StudyDate == date && x.ContentType == contentType && x.ContentId == contentId);
            if (lesson is null) { throw new InvalidOperationException("Bài học không thuộc danh sách hôm nay. Vui lòng tải lại danh sách."); }
            if (lesson.CompletedAt.HasValue)
            {
                await transaction.CommitAsync();
                return false;
            }
            lesson.CompletedAt = now;
            lesson.Correct = correct;
        }

        if (contentType == "vocabulary")
        {
            var progress = await _db.WordProgress.SingleOrDefaultAsync(x => x.UserId == userId && x.WordId == contentId);
            if (progress is null)
            {
                progress = new UserWordProgress { UserId = userId, WordId = contentId };
                _db.WordProgress.Add(progress);
            }
            if (correct) { progress.CorrectCount++; } else { progress.WrongCount++; }
            progress.LastReviewed = now;
            progress.NextReview = StudySchedule.NextReview(now, progress.CorrectCount, progress.WrongCount, correct);
        }
        else
        {
            var progress = await _db.ContentProgress.SingleOrDefaultAsync(x => x.UserId == userId && x.ContentType == contentType && x.ContentId == contentId);
            if (progress is null)
            {
                progress = new UserContentProgress { UserId = userId, ContentType = contentType, ContentId = contentId };
                _db.ContentProgress.Add(progress);
            }
            if (correct) { progress.CorrectCount++; } else { progress.WrongCount++; }
            progress.LastReviewed = now;
            progress.NextReview = StudySchedule.NextReview(now, progress.CorrectCount, progress.WrongCount, correct);
        }

        var activity = await _db.StudyActivity.SingleOrDefaultAsync(x => x.UserId == userId && x.StudyDate == date);
        if (activity is null)
        {
            activity = new UserStudyActivity { UserId = userId, StudyDate = date };
            _db.StudyActivity.Add(activity);
        }
        activity.Attempts++;
        if (correct)
        {
            activity.CorrectCount++;
            activity.Xp += 10;
        }
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private Task LockUserAsync(Guid userId)
    {
        var key = $"study/{userId}";
        return _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))");
    }
}
