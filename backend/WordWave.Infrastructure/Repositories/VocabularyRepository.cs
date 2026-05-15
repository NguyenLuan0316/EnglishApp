using Microsoft.EntityFrameworkCore;
using WordWave.Application.Contracts.Vocabulary;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _db;
    public VocabularyRepository(AppDbContext db) => _db = db;

    public async Task<(int total, List<VocabWord> data)> GetPagedAsync(VocabularyQuery request)
    {
        var source = _db.Vocabulary.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Level)) source = source.Where(w => w.Level == request.Level);
        if (!string.IsNullOrWhiteSpace(request.Topic)) source = source.Where(w => w.Topic == request.Topic);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            source = source.Where(w =>
                w.Word.ToLower().Contains(search) ||
                w.Meaning.ToLower().Contains(search));
        }

        var total = await source.CountAsync();
        var data = await source
            .OrderBy(w => w.Id)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync();

        return (total, data);
    }

    public async Task<List<VocabWord>> GetRandomAsync(string? level, string? topic, int count = 10)
    {
        var query = _db.Vocabulary.AsQueryable();
        if (!string.IsNullOrEmpty(level)) query = query.Where(w => w.Level == level);
        if (!string.IsNullOrEmpty(topic)) query = query.Where(w => w.Topic == topic);
        return await query.OrderBy(_ => EF.Functions.Random()).Take(count).ToListAsync();
    }

    public Task<List<string>> GetTopicsAsync() => _db.Vocabulary
        .Where(w => !string.IsNullOrWhiteSpace(w.Topic))
        .Select(w => w.Topic)
        .Distinct()
        .OrderBy(topic => topic)
        .ToListAsync();

    public Task<VocabWord?> GetByIdAsync(int id) => _db.Vocabulary.FindAsync(id).AsTask().ContinueWith(t => (VocabWord?)t.Result);

    public Task<List<VocabWord>> GetAllAsync() => _db.Vocabulary.ToListAsync();
}
