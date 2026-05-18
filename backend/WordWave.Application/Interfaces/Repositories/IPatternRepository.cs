using WordWave.Application.Contracts.Patterns;
using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IPatternRepository
{
    Task<List<SentencePattern>> GetAllAsync(PatternQuery query);
    Task<SentencePattern?> GetByIdAsync(int id);
}
