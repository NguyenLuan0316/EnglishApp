// wordwave/backend/WordWave.Api/Controllers/GrammarController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrammarController : ControllerBase
{
    // GET /api/grammar?level=B1
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? level)
    {
        var data = LessonData.Grammar.AsEnumerable();
        if (!string.IsNullOrEmpty(level))
            data = data.Where(g => g.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        return Ok(data);
    }

    // GET /api/grammar/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var lesson = LessonData.Grammar.FirstOrDefault(g => g.Id == id);
        return lesson is null ? NotFound(new { error = "Not found" }) : Ok(lesson);
    }
}
