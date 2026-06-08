using WordWave.Application.Contracts.Ielts;

namespace WordWave.Application.Interfaces;

public interface IIeltsService
{
    Task<IReadOnlyList<IeltsTestSummaryDto>> GetTestsAsync(string learnerId);
    Task<IeltsTestDetailDto?> GetTestByIdAsync(int id, string learnerId);
    Task<IeltsAttemptDto?> GetAttemptAsync(int testId, string learnerId);
    Task<IeltsAttemptDto?> SaveAttemptAsync(int testId, IeltsSaveAttemptRequestDto request);
    Task<IeltsAttemptDto?> SubmitAsync(int testId, IeltsSubmitRequestDto request);
}
