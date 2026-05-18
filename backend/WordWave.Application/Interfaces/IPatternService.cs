using WordWave.Application.Contracts.Patterns;

namespace WordWave.Application.Interfaces;

public interface IPatternService
{
    Task<List<SentencePatternDto>> GetAllAsync(PatternQuery query);
    Task<SentencePatternDto?> GetByIdAsync(int id);
}
