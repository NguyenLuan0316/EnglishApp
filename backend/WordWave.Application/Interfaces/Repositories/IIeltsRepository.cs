using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IIeltsRepository
{
    Task<List<IeltsTest>> GetTestsAsync();
    Task<List<IeltsAttempt>> GetAttemptsAsync(string learnerId);
    Task<IeltsTest?> GetTestByIdAsync(int id);
    Task<IeltsAttempt?> GetAttemptAsync(int testId, string learnerId);
    Task<IeltsAttempt> UpsertAttemptAsync(IeltsAttempt attempt);
}
