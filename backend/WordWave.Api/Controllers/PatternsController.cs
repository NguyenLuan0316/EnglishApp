// wordwave/backend/WordWave.Api/Controllers/PatternsController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Models.Requests;
using WordWave.Application.Contracts.Patterns;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatternsController : ApiControllerBase
{
    private readonly IPatternService _service;
    public PatternsController(IPatternService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PatternQueryRequest req)
    {
        var result = await _service.GetAllAsync(new PatternQuery(req.Search, req.Purpose ?? req.Type));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        return p is null ? NotFoundProblem($"Pattern {id} was not found.") : Ok(p);
    }
}
