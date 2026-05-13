using WordWave.Application.Contracts.Patterns;

namespace WordWave.Application.Interfaces;

public interface IPatternService
{
    Task<List<SentencePatternDto>> GetAllAsync();
    Task<SentencePatternDto?> GetByIdAsync(int id);
}
