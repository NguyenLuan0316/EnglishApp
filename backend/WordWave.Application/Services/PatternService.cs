using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class PatternService : IPatternService
{
    private readonly IPatternRepository _repo;
    public PatternService(IPatternRepository repo) => _repo = repo;

    public Task<List<SentencePattern>> GetAllAsync() => _repo.GetAllAsync();

    public Task<SentencePattern?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
}
