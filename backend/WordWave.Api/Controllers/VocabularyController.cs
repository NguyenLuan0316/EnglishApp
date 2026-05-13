using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Contracts.Vocabulary;
using WordWave.Application.Interfaces;
using WordWave.Api.Models.Requests;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _service;
    public VocabularyController(IVocabularyService service) => _service = service;

    // GET /api/vocabulary?level=A1&topic=office&search=manage&page=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] VocabularyQueryRequest req)
    {
        var result = await _service.GetPagedAsync(new VocabularyQuery(req.Level, req.Topic, req.Search, req.Page, req.Limit));
        return Ok(new { total = result.Total, page = result.Page, limit = result.Limit, data = result.Data });
    }

    // GET /api/vocabulary/random?level=B1&count=10
    [HttpGet("random")]
    public async Task<IActionResult> GetRandom([FromQuery] RandomVocabularyRequest req)
    {
        var result = await _service.GetRandomAsync(req.Level, req.Topic, req.Count);
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
