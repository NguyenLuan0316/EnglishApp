// wordwave/backend/WordWave.Api/Controllers/PatternsController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatternsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(LessonData.Patterns);

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var p = LessonData.Patterns.FirstOrDefault(x => x.Id == id);
        return p is null ? NotFound(new { error = "Not found" }) : Ok(p);
    }
}
