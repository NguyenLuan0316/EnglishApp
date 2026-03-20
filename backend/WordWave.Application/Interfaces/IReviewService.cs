using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IReviewService
{
    Task<List<VocabWord>> GetDailyAsync(int max = 20);
    Task<object> SubmitAsync(SubmitRequest req);
    Task<object> GetProgressAsync();
}
