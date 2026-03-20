using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class PatternRepository : IPatternRepository
{
    private readonly AppDbContext _db;
    public PatternRepository(AppDbContext db) => _db = db;

    public Task<List<SentencePattern>> GetAllAsync() => _db.SentencePatterns.ToListAsync();

    public Task<SentencePattern?> GetByIdAsync(int id) => _db.SentencePatterns.FirstOrDefaultAsync(x => x.Id == id);
}
