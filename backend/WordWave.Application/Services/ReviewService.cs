using WordWave.Application.Contracts.Review;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IVocabularyRepository _vocabRepo;
    private readonly IUserProgressRepository _progress;
    private readonly ICurrentUser _currentUser;

    public ReviewService(IVocabularyRepository vocabRepo, IUserProgressRepository progress, ICurrentUser currentUser)
    {
        _vocabRepo = vocabRepo;
        _progress = progress;
        _currentUser = currentUser;
    }

    public async Task<List<VocabWord>> GetDailyAsync(int max = 20)
    {
        var all = await _vocabRepo.GetAllAsync();
        var progress = (await _progress.GetAllAsync(_currentUser.Id)).ToDictionary(x => x.WordId);
        var now = DateTime.UtcNow;
        return all.Where(w => !progress.TryGetValue(w.Id, out var p) || p.NextReview <= now).Take(max).ToList();
    }

    public async Task<SubmitReviewResult> SubmitAsync(SubmitReviewRequest req)
    {
        if (await _vocabRepo.GetByIdAsync(req.WordId) is null)
        {
            throw new KeyNotFoundException($"Vocabulary word {req.WordId} was not found.");
        }
        var p = await _progress.RecordAsync(_currentUser.Id, req.WordId, req.Correct);
        return new(true, new(p.WordId, p.CorrectCount, p.WrongCount, p.NextReview, p.LastReviewed));
    }

    public async Task<ReviewProgressDto> GetProgressAsync()
    {
        var all = await _vocabRepo.GetAllAsync();
        var learned = (await _progress.GetAllAsync(_currentUser.Id)).Where(x => x.CorrectCount > 0).Select(x => x.WordId).ToHashSet();
        var byLevel = all.GroupBy(w => w.Level).ToDictionary(g => g.Key, g => new LevelProgressDto(g.Count(), g.Count(w => learned.Contains(w.Id))));
        return new(all.Count, all.Count(w => learned.Contains(w.Id)), byLevel);
    }
}
