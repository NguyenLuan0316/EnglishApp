using WordWave.Application.Contracts.Review;
using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IReviewService
{
    Task<List<VocabWord>> GetDailyAsync(int max = 20);
    Task<SubmitReviewResult> SubmitAsync(SubmitReviewRequest req);
    Task<ReviewProgressDto> GetProgressAsync();
}
