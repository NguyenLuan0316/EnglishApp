using WordWave.Application.Contracts.Patterns;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class PatternService : IPatternService
{
    private readonly IPatternRepository _repo;
    public PatternService(IPatternRepository repo) => _repo = repo;

    public async Task<List<SentencePatternDto>> GetAllAsync(PatternQuery query)
    {
        var patterns = await _repo.GetAllAsync(query);
        return patterns.Select(Map).ToList();
    }

    public async Task<SentencePatternDto?> GetByIdAsync(int id)
    {
        var pattern = await _repo.GetByIdAsync(id);
        return pattern is null ? null : Map(pattern);
    }

    private static SentencePatternDto Map(SentencePattern pattern)
    {
        return new SentencePatternDto(
            pattern.Id,
            pattern.Sentence,
            pattern.Type,
            pattern.Meaning,
            pattern.Explanation,
            pattern.Examples
        );
    }
}
