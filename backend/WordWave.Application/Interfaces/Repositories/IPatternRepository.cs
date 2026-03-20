using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IPatternRepository
{
    Task<List<SentencePattern>> GetAllAsync();
    Task<SentencePattern?> GetByIdAsync(int id);
}
