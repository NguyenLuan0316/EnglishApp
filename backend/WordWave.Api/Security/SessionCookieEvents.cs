using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WordWave.Application.Contracts.Auth;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Security;

public class SessionCookieEvents : CookieAuthenticationEvents
{
    public const string SessionClaim = "wordwave_session";
    private readonly IAuthService _auth;
    public SessionCookieEvents(IAuthService auth) => _auth = auth;

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue(SessionClaim), out var sessionId) || await _auth.ValidateSessionAsync(sessionId) is not { } user)
        {
            context.RejectPrincipal();
            return;
        }
        context.ReplacePrincipal(CreatePrincipal(sessionId, user));
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public static ClaimsPrincipal CreatePrincipal(Guid sessionId, UserDto user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.DisplayName),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(SessionClaim, sessionId.ToString())
    }, CookieAuthenticationDefaults.AuthenticationScheme));
}
