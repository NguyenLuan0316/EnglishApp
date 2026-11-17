using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class UserProgressRepository : IUserProgressRepository
{
    private readonly AuthDbContext _db;
    private readonly IUserLearningRepository _learning;
    private readonly TimeProvider _time;
    public UserProgressRepository(AuthDbContext db, IUserLearningRepository learning, TimeProvider time)
    {
        _db = db;
        _learning = learning;
        _time = time;
    }

    public Task<List<UserWordProgress>> GetAllAsync(Guid userId) => _db.WordProgress.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();

    public async Task<UserWordProgress> RecordAsync(Guid userId, int wordId, bool correct)
    {
        await _learning.RecordAsync(userId, "vocabulary", wordId, correct, false, _time.GetUtcNow().UtcDateTime);
        return await _db.WordProgress.AsNoTracking().SingleAsync(x => x.UserId == userId && x.WordId == wordId);
    }
}
