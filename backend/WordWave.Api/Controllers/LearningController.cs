using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWave.Application.Contracts.Review;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/learning")]
public class LearningController : ApiControllerBase
{
    private readonly ILearningService _service;
    public LearningController(ILearningService service) => _service = service;

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily() => Ok(await _service.GetDailyAsync());

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress() => Ok(await _service.GetProgressAsync());

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitLearningRequest request)
    {
        try
        {
            return Ok(await _service.SubmitAsync(request));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }
}
