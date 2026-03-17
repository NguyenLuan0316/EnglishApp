// wordwave/backend/WordWave.Api/Controllers/VocabularyController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;
using WordWave.Api.Models;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private static readonly List<VocabWord> _words = VocabData.Words;

    // GET /api/vocabulary?level=A1&topic=food&search=hello&page=1&limit=20
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? level, [FromQuery] string? topic,
                                [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var query = _words.AsEnumerable();

        if (!string.IsNullOrEmpty(level))
            query = query.Where(w => w.Level.Equals(level, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(topic))
            query = query.Where(w => w.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(search))
        {
            var q = search.ToLower();
            query = query.Where(w => w.Word.ToLower().Contains(q) || w.Meaning.ToLower().Contains(q));
        }

        var total     = query.Count();
        var paginated = query.Skip((page - 1) * limit).Take(limit).ToList();

        return Ok(new { total, page, limit, data = paginated });
    }

    // GET /api/vocabulary/topics
    [HttpGet("topics")]
    public IActionResult GetTopics() =>
        Ok(_words.Select(w => w.Topic).Distinct().OrderBy(t => t));

    // GET /api/vocabulary/levels
    [HttpGet("levels")]
    public IActionResult GetLevels() =>
        Ok(_words.Select(w => w.Level).Distinct().OrderBy(l => l));

    // GET /api/vocabulary/random?level=A1&topic=food&count=10
    [HttpGet("random")]
    public IActionResult GetRandom([FromQuery] string? level, [FromQuery] string? topic, [FromQuery] int count = 10)
    {
        var pool = _words.AsEnumerable();
        if (!string.IsNullOrEmpty(level)) pool = pool.Where(w => w.Level == level);
        if (!string.IsNullOrEmpty(topic)) pool = pool.Where(w => w.Topic == topic);
        var result = pool.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        return Ok(result);
    }

    // GET /api/vocabulary/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var word = _words.FirstOrDefault(w => w.Id == id);
        return word is null ? NotFound(new { error = "Not found" }) : Ok(word);
    }
}
