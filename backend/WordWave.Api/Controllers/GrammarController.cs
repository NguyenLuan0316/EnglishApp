// wordwave/backend/WordWave.Api/Controllers/GrammarController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrammarController : ControllerBase
{
    private readonly AppDbContext _db;
    public GrammarController(AppDbContext db) => _db = db;

    // GET /api/grammar?level=B1
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? level)
    {
        var query = _db.GrammarLessons.AsQueryable();

        var data = query.AsEnumerable();
        if (!string.IsNullOrEmpty(level))
            data = data.Where(g => g.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        return Ok(data);
    }

    // GET /api/grammar/{id}
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var query = _db.GrammarLessons.AsQueryable();
        var lesson = query.FirstOrDefault(g => g.Id == id);
        return lesson is null ? NotFound(new { error = "Not found" }) : Ok(lesson);
    }
}
