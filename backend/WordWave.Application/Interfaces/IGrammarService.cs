using WordWave.Domain.Models;

namespace WordWave.Application.Interfaces;

public interface IGrammarService
{
    Task<List<GrammarLesson>> GetAllAsync(string? level);
    Task<GrammarLesson?> GetByIdAsync(int id);
}