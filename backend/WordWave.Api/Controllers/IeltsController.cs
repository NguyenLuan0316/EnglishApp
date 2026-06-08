using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Models.Requests;
using WordWave.Application.Contracts.Ielts;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/ielts")]
public class IeltsController : ApiControllerBase
{
    private readonly IIeltsService _service;

    public IeltsController(IIeltsService service) => _service = service;

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests([FromQuery] string learnerId = "default")
    {
        return Ok(await _service.GetTestsAsync(learnerId));
    }

    [HttpGet("tests/{id:int}")]
    public async Task<IActionResult> GetTestById(int id, [FromQuery] string learnerId = "default")
    {
        var test = await _service.GetTestByIdAsync(id, learnerId);
        return test is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(test);
    }

    [HttpGet("tests/{id:int}/attempt")]
    public async Task<IActionResult> GetAttempt(int id, [FromQuery] string learnerId = "default")
    {
        var attempt = await _service.GetAttemptAsync(id, learnerId);
        return attempt is null ? NotFoundProblem($"IELTS attempt for test {id} was not found.") : Ok(attempt);
    }

    [HttpPut("tests/{id:int}/attempt")]
    public async Task<IActionResult> SaveAttempt(int id, [FromBody] IeltsAttemptRequest request)
    {
        var attempt = await _service.SaveAttemptAsync(id, new IeltsSaveAttemptRequestDto(request.LearnerId, request.StateData));
        return attempt is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(attempt);
    }

    [HttpPost("tests/{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] IeltsAttemptRequest request)
    {
        var attempt = await _service.SubmitAsync(id, new IeltsSubmitRequestDto(request.LearnerId, request.StateData));
        return attempt is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(attempt);
    }
}
