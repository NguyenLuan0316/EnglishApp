using WordWave.Application.Contracts.Vocabulary;
using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IVocabularyRepository
{
    Task<(int total, List<VocabWord> data)> GetPagedAsync(VocabularyQuery query);
    Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10);
    Task<List<string>> GetTopicsAsync();
    Task<List<VocabularyTopicSummaryDto>> GetTopicSummariesAsync(string? level);
    Task<VocabWord?> GetByIdAsync(int id);
    Task<List<VocabWord>> GetAllAsync();
}
