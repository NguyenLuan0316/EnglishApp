using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IVocabularyService
{
    Task<(int total, int page, int limit, List<VocabWord> data)> GetPagedAsync(string? level, string? topic, string? search, int page = 1, int limit = 20);
    Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10);
    Task<List<string>> GetTopicsAsync();
    Task<VocabWord?> GetByIdAsync(int id);
    Task<List<VocabWord>> GetAllAsync();
}
