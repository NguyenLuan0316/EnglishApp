using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IVocabularyRepository
{
    Task<(int total, List<VocabWord> data)> GetPagedAsync(string? level, string? topic, string? search, int page = 1, int limit = 20);
    Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10);
    Task<List<string>> GetTopicsAsync();
    Task<VocabWord?> GetByIdAsync(int id);
    Task<List<VocabWord>> GetAllAsync();
}
