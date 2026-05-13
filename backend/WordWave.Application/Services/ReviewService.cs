using WordWave.Application.Contracts.Review;
using WordWave.Application.Interfaces;
using WordWave.Application.Interfaces.Repositories;
using WordWave.Domain.Models;

namespace WordWave.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IVocabularyRepository _vocabRepo;
    private static readonly Dictionary<int, WordProgress> _store = new();

    public ReviewService(IVocabularyRepository vocabRepo) => _vocabRepo = vocabRepo;

    private static WordProgress GetOrInit(int wordId)
    {
        if (!_store.ContainsKey(wordId))
            _store[wordId] = new WordProgress { WordId = wordId, NextReview = DateTime.UtcNow };
        return _store[wordId];
    }

    private static DateTime CalcNextReview(int correct, int wrong)
    {
        var total = correct + wrong;
        double ratio = total == 0 ? 0 : (double)correct / total;
        int daysLater = ratio switch
        {
            >= 0.9 => 7,
            >= 0.7 => 3,
            >= 0.5 => 1,
            _      => 0,
        };
        return DateTime.UtcNow.AddDays(daysLater);
    }

    public async Task<List<VocabWord>> GetDailyAsync(int max = 20)
    {
        var all = await _vocabRepo.GetAllAsync();
        var now = DateTime.UtcNow;
        return all.Where(w => GetOrInit(w.Id).NextReview <= now).Take(max).ToList();
    }

    public Task<SubmitReviewResult> SubmitAsync(SubmitReviewRequest req)
    {
        var p = GetOrInit(req.WordId);
        if (req.Correct) p.CorrectCount++; else p.WrongCount++;
        p.LastReviewed = DateTime.UtcNow;
        p.NextReview   = CalcNextReview(p.CorrectCount, p.WrongCount);
        return Task.FromResult(new SubmitReviewResult(
            true,
            new ReviewWordProgressDto(p.WordId, p.CorrectCount, p.WrongCount, p.NextReview, p.LastReviewed)
        ));
    }

    public async Task<ReviewProgressDto> GetProgressAsync()
    {
        var all = await _vocabRepo.GetAllAsync();
        var total   = all.Count;
        var learned = _store.Values.Count(p => p.CorrectCount > 0);

        var byLevel = all
            .GroupBy(w => w.Level)
            .ToDictionary(
                g => g.Key,
                g => new LevelProgressDto(
                    g.Count(),
                    g.Count(w => _store.TryGetValue(w.Id, out var p) && p.CorrectCount > 0)
                ));

        return new ReviewProgressDto(total, learned, byLevel);
    }
}
