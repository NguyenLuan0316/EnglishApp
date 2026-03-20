// wordwave/backend/WordWave.Api/Controllers/ReviewController.cs
using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Interfaces;
using WordWave.Domain.Models;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _service;
    public ReviewController(IReviewService service) => _service = service;

    // GET /api/review/daily  — từ cần ôn hôm nay (tối đa 20)
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily() => Ok(await _service.GetDailyAsync());

    // POST /api/review/submit  body: { wordId, correct }
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitRequest req) => Ok(await _service.SubmitAsync(req));

    // GET /api/review/progress  — tổng tiến độ
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress() => Ok(await _service.GetProgressAsync());
}
