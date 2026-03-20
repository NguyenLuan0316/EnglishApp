using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _db;
    public VocabularyRepository(AppDbContext db) => _db = db;

    public async Task<(int total, List<VocabWord> data)> GetPagedAsync(string? level, string? topic, string? search, int page = 1, int limit = 20)
    {
        var query = _db.Vocabulary.AsQueryable();
        if (!string.IsNullOrEmpty(level)) query = query.Where(w => w.Level == level);
        if (!string.IsNullOrEmpty(topic)) query = query.Where(w => w.Topic == topic);
        if (!string.IsNullOrEmpty(search)) query = query.Where(w =>
            w.Word.ToLower().Contains(search.ToLower()) ||
            w.Meaning.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(w => w.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
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

    public Task<List<string>> GetTopicsAsync() => _db.Vocabulary.Select(w => w.Topic).Distinct().ToListAsync();

    public Task<VocabWord?> GetByIdAsync(int id) => _db.Vocabulary.FindAsync(id).AsTask().ContinueWith(t => (VocabWord?)t.Result);

    public Task<List<VocabWord>> GetAllAsync() => _db.Vocabulary.ToListAsync();
}
