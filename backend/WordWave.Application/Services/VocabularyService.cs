using WordWave.Application.Contracts.Paging;
using WordWave.Application.Contracts.Vocabulary;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repo;

    public VocabularyService(IVocabularyRepository repo) => _repo = repo;

    public async Task<PagedResult<VocabWord>> GetPagedAsync(VocabularyQuery query)
    {
        var (total, data) = await _repo.GetPagedAsync(query);
        return new PagedResult<VocabWord>(total, query.Page, query.Limit, data);
    }

    public Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10) => _repo.GetRandomAsync(level, topic, count);

    public Task<List<string>> GetTopicsAsync() => _repo.GetTopicsAsync();

    public Task<VocabWord?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task<List<VocabWord>> GetAllAsync() => _repo.GetAllAsync();
}
