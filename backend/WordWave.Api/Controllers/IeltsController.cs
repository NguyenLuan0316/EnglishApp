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
    private readonly ICurrentUser _currentUser;

    public IeltsController(IIeltsService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests([FromQuery] string learnerId = "default")
    {
        return Ok(await _service.GetTestsAsync(_currentUser.Id.ToString()));
    }

    [HttpGet("tests/{id:int}")]
    public async Task<IActionResult> GetTestById(int id, [FromQuery] string learnerId = "default")
    {
        var test = await _service.GetTestByIdAsync(id, _currentUser.Id.ToString());
        return test is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(test);
    }

    [HttpGet("tests/{id:int}/attempt")]
    public async Task<IActionResult> GetAttempt(int id, [FromQuery] string learnerId = "default")
    {
        var attempt = await _service.GetAttemptAsync(id, _currentUser.Id.ToString());
        return attempt is null ? NotFoundProblem($"IELTS attempt for test {id} was not found.") : Ok(attempt);
    }

    [HttpPut("tests/{id:int}/attempt")]
    public async Task<IActionResult> SaveAttempt(int id, [FromBody] IeltsAttemptRequest request)
    {
        var attempt = await _service.SaveAttemptAsync(id, new IeltsSaveAttemptRequestDto(_currentUser.Id.ToString(), request.StateData));
        return attempt is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(attempt);
    }

    [HttpPost("tests/{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] IeltsAttemptRequest request)
    {
        var attempt = await _service.SubmitAsync(id, new IeltsSubmitRequestDto(_currentUser.Id.ToString(), request.StateData));
        return attempt is null ? NotFoundProblem($"IELTS test {id} was not found.") : Ok(attempt);
    }
}
