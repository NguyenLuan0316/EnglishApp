using Microsoft.EntityFrameworkCore;
using System;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class GrammarRepository : IGrammarRepository
{
    private readonly AppDbContext _db;

    public GrammarRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<GrammarLesson>> GetAllAsync()
    {
        return await _db.GrammarLessons.ToListAsync();
    }

    public async Task<GrammarLesson?> GetByIdAsync(int id)
    {
        return await _db.GrammarLessons.FirstOrDefaultAsync(x => x.Id == id);
    }
}