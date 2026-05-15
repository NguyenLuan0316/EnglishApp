using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Models.Requests;
using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/toeic")]
public class ToeicController : ApiControllerBase
{
    private readonly IToeicService _service;

    public ToeicController(IToeicService service) => _service = service;

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests()
    {
        return Ok(await _service.GetTestsAsync());
    }

    [HttpGet("tests/{id:int}")]
    public async Task<IActionResult> GetTestById(int id)
    {
        var test = await _service.GetTestByIdAsync(id);
        return test is null ? NotFoundProblem($"TOEIC test {id} was not found.") : Ok(test);
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions([FromQuery] ToeicQuestionQueryRequest request)
    {
        return Ok(await _service.GetQuestionsAsync(request.Part));
    }

    [HttpPost("tests/{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] ToeicSubmitRequest request)
    {
        var result = await _service.SubmitAsync(id, new ToeicSubmitRequestDto(request.Answers));
        return result is null ? NotFoundProblem($"TOEIC test {id} was not found.") : Ok(result);
    }
}
