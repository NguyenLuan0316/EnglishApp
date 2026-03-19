// wordwave/backend/WordWave.Api/Controllers/PatternsController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Data;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatternsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PatternsController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll() => Ok(_db.SentencePatterns.AsQueryable());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var query = _db.SentencePatterns.AsQueryable();

        var p = query.FirstOrDefault(x => x.Id == id);
        return p is null ? NotFound(new { error = "Not found" }) : Ok(p);
    }
}
