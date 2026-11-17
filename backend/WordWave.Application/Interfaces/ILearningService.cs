using WordWave.Application.Contracts.Review;

namespace WordWave.Application.Interfaces;

public interface ILearningService
{
    Task<DailyLearningDto> GetDailyAsync();
    Task<LearningProgressDto> GetProgressAsync();
    Task<SubmitLearningResult> SubmitAsync(SubmitLearningRequest request);
}
