using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordWave.Api.Data;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly AppDbContext _db;
    public VocabularyController(AppDbContext db) => _db = db;

    // GET /api/vocabulary?level=A1&topic=office&search=manage&page=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? level,
        [FromQuery] string? topic,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var query = _db.Vocabulary.AsQueryable();

        if (!string.IsNullOrEmpty(level)) query = query.Where(w => w.Level == level);
        if (!string.IsNullOrEmpty(topic)) query = query.Where(w => w.Topic == topic);
        if (!string.IsNullOrEmpty(search)) query = query.Where(w =>
            w.Word.ToLower().Contains(search.ToLower()) ||
            w.Meaning.ToLower().Contains(search.ToLower()));

        var total = await query.CountAsync();
        var paginated = await query
            .OrderBy(w => w.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new { total, page, limit, data = paginated });
    }

    // GET /api/vocabulary/random?level=B1&count=10
    [HttpGet("random")]
    public async Task<IActionResult> GetRandom(
        [FromQuery] string? level,
        [FromQuery] string? topic,
        [FromQuery] int count = 10)
    {
        var query = _db.Vocabulary.AsQueryable();
        if (!string.IsNullOrEmpty(level)) query = query.Where(w => w.Level == level);
        if (!string.IsNullOrEmpty(topic)) query = query.Where(w => w.Topic == topic);

        var result = await query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics() =>
        Ok(await _db.Vocabulary.Select(w => w.Topic).Distinct().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var word = await _db.Vocabulary.FindAsync(id);
        return word is null ? NotFound() : Ok(word);
    }
}

