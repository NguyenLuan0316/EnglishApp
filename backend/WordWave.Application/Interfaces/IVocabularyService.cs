using WordWave.Application.Contracts.Paging;
using WordWave.Application.Contracts.Vocabulary;
using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IVocabularyService
{
    Task<PagedResult<VocabWord>> GetPagedAsync(VocabularyQuery query);
    Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10);
    Task<List<string>> GetTopicsAsync();
    Task<VocabWord?> GetByIdAsync(int id);
    Task<List<VocabWord>> GetAllAsync();
}
