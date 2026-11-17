using System.Security.Claims;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;
    public Guid Id => Guid.Parse(_accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
