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

    public async Task<List<GrammarLesson>> GetAllAsync(string? level)
    {
        var data = await _repo.GetAllAsync();

        if (!string.IsNullOrEmpty(level))
        {
            data = data.Where(g => g.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return data;
    }

    public async Task<GrammarLesson?> GetByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }
}