// wordwave/backend/WordWave.Api/Controllers/ReviewController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;
using WordWave.Api.Models;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    // In-memory store (thay bằng DB trong production)
    private static readonly Dictionary<int, WordProgress> _store = new();

    private static WordProgress GetOrInit(int wordId)
    {
        if (!_store.ContainsKey(wordId))
            _store[wordId] = new WordProgress { WordId = wordId, NextReview = DateTime.UtcNow };
        return _store[wordId];
    }

    // SM-2 đơn giản: tính ngày ôn tiếp theo
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

    // GET /api/review/daily  — từ cần ôn hôm nay (tối đa 20)
    [HttpGet("daily")]
    public IActionResult GetDaily()
    {
        var now = DateTime.UtcNow;
        var due = VocabData.Words
            .Where(w => GetOrInit(w.Id).NextReview <= now)
            .Take(20)
            .ToList();
        return Ok(due);
    }

    // POST /api/review/submit  body: { wordId, correct }
    [HttpPost("submit")]
    public IActionResult Submit([FromBody] SubmitRequest req)
    {
        var p = GetOrInit(req.WordId);
        if (req.Correct) p.CorrectCount++; else p.WrongCount++;
        p.LastReviewed = DateTime.UtcNow;
        p.NextReview   = CalcNextReview(p.CorrectCount, p.WrongCount);
        return Ok(new { success = true, progress = p });
    }

    // GET /api/review/progress  — tổng tiến độ
    [HttpGet("progress")]
    public IActionResult GetProgress()
    {
        var total   = VocabData.Words.Count;
        var learned = _store.Values.Count(p => p.CorrectCount > 0);

        var byLevel = VocabData.Words
            .GroupBy(w => w.Level)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Total   = g.Count(),
                    Learned = g.Count(w => _store.TryGetValue(w.Id, out var p) && p.CorrectCount > 0)
                });

        return Ok(new { total, learned, byLevel });
    }
}
