using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Models.Requests;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/admin/toeic")]
public class AdminToeicController : ApiControllerBase
{
    private readonly IToeicImportService _service;

    public AdminToeicController(IToeicImportService service) => _service = service;

    [HttpPost("import/json")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ImportJson(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "A non-empty JSON file is required." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportJsonAsync(stream, file.FileName);
        return Ok(result);
    }

    [HttpPost("import/csv")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "A non-empty CSV file is required." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportCsvAsync(stream, file.FileName);
        return Ok(result);
    }

    [HttpPost("crawl")]
    public async Task<IActionResult> Crawl([FromBody] ToeicCrawlRequest request)
    {
        var result = await _service.CrawlAsync(request.Keyword, request.SourceUrl);
        return Ok(result);
    }

    [HttpGet("import-logs")]
    public async Task<IActionResult> GetImportLogs([FromQuery] ToeicImportLogQueryRequest request)
    {
        return Ok(await _service.GetImportLogsAsync(request.Page, request.Limit));
    }
}
