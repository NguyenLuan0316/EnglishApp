// wordwave/backend/WordWave.Api/Controllers/PatternsController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatternsController : ApiControllerBase
{
    private readonly IPatternService _service;
    public PatternsController(IPatternService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        return p is null ? NotFoundProblem($"Pattern {id} was not found.") : Ok(p);
    }
}
