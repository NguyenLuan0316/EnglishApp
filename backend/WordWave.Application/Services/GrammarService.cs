using WordWave.Application.Contracts.Grammar;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class GrammarService : IGrammarService
{
    private readonly IGrammarRepository _repo;

    public GrammarService(IGrammarRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<GrammarLessonDto>> GetAllAsync(string? level)
    {
        var data = await _repo.GetAllAsync();

        if (!string.IsNullOrEmpty(level))
        {
            data = data.Where(g => g.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return data.Select(Map).ToList();
    }

    public async Task<GrammarLessonDto?> GetByIdAsync(int id)
    {
        var lesson = await _repo.GetByIdAsync(id);
        return lesson is null ? null : Map(lesson);
    }

    private static GrammarLessonDto Map(GrammarLesson lesson)
    {
        return new GrammarLessonDto(
            lesson.Id,
            lesson.Title,
            lesson.Level,
            lesson.Description,
            lesson.Formula,
            lesson.Tips,
            lesson.GrammarExamples.Select(x => new GrammarExampleDto(x.En, x.Vi)).ToList()
        );
    }
}
