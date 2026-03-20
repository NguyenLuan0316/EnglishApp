using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _service;
    public VocabularyController(IVocabularyService service) => _service = service;

    // GET /api/vocabulary?level=A1&topic=office&search=manage&page=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? level,
        [FromQuery] string? topic,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var result = await _service.GetPagedAsync(level, topic, search, page, limit);
        return Ok(new { total = result.total, page = result.page, limit = result.limit, data = result.data });
    }

    // GET /api/vocabulary/random?level=B1&count=10
    [HttpGet("random")]
    public async Task<IActionResult> GetRandom(
        [FromQuery] string? level,
        [FromQuery] string? topic,
        [FromQuery] int count = 10)
    {
        var result = await _service.GetRandomAsync(level, topic, count);
        return Ok(result);
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics() => Ok(await _service.GetTopicsAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var word = await _service.GetByIdAsync(id);
        return word is null ? NotFound() : Ok(word);
    }
}

