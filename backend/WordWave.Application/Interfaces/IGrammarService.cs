using WordWave.Application.Contracts.Grammar;

namespace WordWave.Application.Interfaces;

public interface IGrammarService
{
    Task<List<GrammarLessonDto>> GetAllAsync(string? level);
    Task<GrammarLessonDto?> GetByIdAsync(int id);
}
