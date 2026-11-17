using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IUserLearningRepository
{
    Task<List<UserContentProgress>> GetProgressAsync(Guid userId);
    Task<List<UserStudyActivity>> GetActivityAsync(Guid userId);
    Task<List<UserDailyLesson>> GetOrCreatePlanAsync(Guid userId, DateOnly date, IReadOnlyList<UserDailyLesson> candidates);
    Task<bool> RecordAsync(Guid userId, string contentType, int contentId, bool correct, bool daily, DateTime now);
}
