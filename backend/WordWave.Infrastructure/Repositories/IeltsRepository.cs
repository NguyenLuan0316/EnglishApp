using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class IeltsRepository : IIeltsRepository
{
    private readonly AppDbContext _db;

    public IeltsRepository(AppDbContext db) => _db = db;

    public async Task<List<IeltsTest>> GetTestsAsync()
    {
        return await _db.IeltsTests
            .AsNoTracking()
            .Where(x => x.IsPublic)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<IeltsAttempt>> GetAttemptsAsync(string learnerId)
    {
        return await _db.IeltsAttempts
            .AsNoTracking()
            .Where(x => x.LearnerId == learnerId)
            .ToListAsync();
    }

    public async Task<IeltsTest?> GetTestByIdAsync(int id)
    {
        return await _db.IeltsTests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublic);
    }

    public async Task<IeltsAttempt?> GetAttemptAsync(int testId, string learnerId)
    {
        return await _db.IeltsAttempts
            .FirstOrDefaultAsync(x => x.IeltsTestId == testId && x.LearnerId == learnerId);
    }

    public async Task<IeltsAttempt> UpsertAttemptAsync(IeltsAttempt attempt)
    {
        var existing = await _db.IeltsAttempts
            .FirstOrDefaultAsync(x => x.IeltsTestId == attempt.IeltsTestId && x.LearnerId == attempt.LearnerId);

        if (existing is null)
        {
            _db.IeltsAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return attempt;
        }

        existing.StateData = attempt.StateData;
        existing.ResultData = attempt.ResultData;
        existing.IsSubmitted = attempt.IsSubmitted;
        existing.OverallBand = attempt.OverallBand;
        existing.UpdatedAt = attempt.UpdatedAt;
        existing.SubmittedAt = attempt.SubmittedAt;
        await _db.SaveChangesAsync();
        return existing;
    }
}
