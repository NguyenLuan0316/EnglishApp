using Microsoft.EntityFrameworkCore;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Infrastructure.Repositories;

public class ToeicRepository : IToeicRepository
{
    private readonly AppDbContext _db;

    public ToeicRepository(AppDbContext db) => _db = db;

    public async Task<List<ToeicTest>> GetTestsAsync()
    {
        return await _db.ToeicTests
            .AsNoTracking()
            .Where(x => x.IsPublic)
            .Include(x => x.Parts)
                .ThenInclude(x => x.Questions)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<ToeicTest?> GetTestByIdAsync(int id)
    {
        return await _db.ToeicTests
            .AsNoTracking()
            .Include(x => x.Parts)
                .ThenInclude(x => x.Passages)
            .Include(x => x.Parts)
                .ThenInclude(x => x.Audios)
            .Include(x => x.Parts)
                .ThenInclude(x => x.Questions)
                    .ThenInclude(x => x.Answers)
            .Include(x => x.Parts)
                .ThenInclude(x => x.Questions)
                    .ThenInclude(x => x.Passage)
            .Include(x => x.Parts)
                .ThenInclude(x => x.Questions)
                    .ThenInclude(x => x.Audio)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublic);
    }

    public async Task<List<ToeicQuestion>> GetQuestionsAsync(int? part)
    {
        var query = _db.ToeicQuestions
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.Passage)
            .Include(x => x.Audio)
            .Include(x => x.Answers)
            .AsQueryable();

        if (part.HasValue)
        {
            query = query.Where(x => x.Part != null && x.Part.PartNumber == part.Value);
        }

        return await query
            .OrderBy(x => x.Part!.PartNumber)
            .ThenBy(x => x.QuestionNumber)
            .ToListAsync();
    }

    public async Task<List<ToeicQuestion>> GetQuestionsByTestIdAsync(int testId)
    {
        return await _db.ToeicQuestions
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.Answers)
            .Where(x => x.Part != null && x.Part.ToeicTestId == testId)
            .OrderBy(x => x.Part!.PartNumber)
            .ThenBy(x => x.QuestionNumber)
            .ToListAsync();
    }

    public async Task<int> AddTestAsync(ToeicTest test)
    {
        _db.ToeicTests.Add(test);
        await _db.SaveChangesAsync();
        return test.Id;
    }

    public async Task AddImportLogAsync(ToeicImportLog log)
    {
        _db.ToeicImportLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ToeicImportLog>> GetImportLogsAsync(int page, int limit)
    {
        return await _db.ToeicImportLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public Task<int> CountImportLogsAsync()
    {
        return _db.ToeicImportLogs.CountAsync();
    }
}
