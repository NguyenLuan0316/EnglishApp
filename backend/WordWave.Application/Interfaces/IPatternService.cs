using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IPatternService
{
    Task<List<SentencePattern>> GetAllAsync();
    Task<SentencePattern?> GetByIdAsync(int id);
}
