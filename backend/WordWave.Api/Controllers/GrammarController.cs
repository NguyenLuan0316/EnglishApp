using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Interfaces;
using WordWave.Api.Models.Requests;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrammarController : ControllerBase
{
    private readonly IGrammarService _service;

    public GrammarController(IGrammarService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GrammarQueryRequest req)
    {
        var data = await _service.GetAllAsync(req.Level);
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lesson = await _service.GetByIdAsync(id);
        return lesson is null
            ? NotFound(new { error = "Not found" })
            : Ok(lesson);
    }
}
