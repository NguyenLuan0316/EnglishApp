using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces.Repositories;

public interface IGrammarRepository
{
    Task<List<GrammarLesson>> GetAllAsync();
    Task<GrammarLesson?> GetByIdAsync(int id);
}