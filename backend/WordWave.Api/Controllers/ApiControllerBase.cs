using Microsoft.AspNetCore.Mvc;

namespace WordWave.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult NotFoundProblem(string detail)
    {
        return NotFound(new ProblemDetails
        {
            Type = "https://httpstatuses.com/404",
            Title = "Resource Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = detail,
            Instance = HttpContext.Request.Path
        });
    }
}
