using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IToeicRepository
{
    Task<List<ToeicTest>> GetTestsAsync();
    Task<ToeicTest?> GetTestByIdAsync(int id);
    Task<List<ToeicQuestion>> GetQuestionsAsync(int? part);
    Task<List<ToeicQuestion>> GetQuestionsByTestIdAsync(int testId);
    Task<int> AddTestAsync(ToeicTest test);
    Task AddImportLogAsync(ToeicImportLog log);
    Task<List<ToeicImportLog>> GetImportLogsAsync(int page, int limit);
    Task<int> CountImportLogsAsync();
}
