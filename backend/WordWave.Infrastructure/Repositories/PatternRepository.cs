using Microsoft.EntityFrameworkCore;
using WordWave.Application.Contracts.Patterns;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class PatternRepository : IPatternRepository
{
    private readonly AppDbContext _db;
    public PatternRepository(AppDbContext db) => _db = db;

    public Task<List<SentencePattern>> GetAllAsync(PatternQuery query)
    {
        var patterns = _db.SentencePatterns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Purpose))
        {
            patterns = patterns.Where(x => x.Meaning == query.Purpose);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            patterns = patterns.Where(x =>
                EF.Functions.ILike(x.Sentence, term) ||
                EF.Functions.ILike(x.Type, term) ||
                EF.Functions.ILike(x.Meaning, term) ||
                EF.Functions.ILike(x.Explanation, term));
        }

        return patterns
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public Task<SentencePattern?> GetByIdAsync(int id) => _db.SentencePatterns.FirstOrDefaultAsync(x => x.Id == id);
}
