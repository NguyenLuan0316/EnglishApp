using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordWave.Api.Models.Requests;
using WordWave.Api.Security;
using WordWave.Application.Interfaces;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _currentUser;
    public AuthController(IAuthService auth, ICurrentUser currentUser)
    {
        _auth = auth;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult GetCsrf([FromServices] IAntiforgery antiforgery) => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request.Email, request.DisplayName, request.Password);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : StatusCode(result.Status, result.Value);
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request.Email, request.Password);
        if (result.Value is not { } session)
        {
            return Problem(detail: result.Error, statusCode: result.Status);
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, SessionCookieEvents.CreatePrincipal(session.SessionId, session.User), new AuthenticationProperties { ExpiresUtc = session.ExpiresAt, IsPersistent = false, AllowRefresh = false });
        return Ok(session.User);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me() => Ok(await _auth.ValidateSessionAsync(Guid.Parse(User.FindFirstValue(SessionCookieEvents.SessionClaim)!)));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _auth.LogoutAsync(Guid.Parse(User.FindFirstValue(SessionCookieEvents.SessionClaim)!));
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var result = await _auth.ChangePasswordAsync(_currentUser.Id, request.CurrentPassword, request.NewPassword);
        if (result.Value is null)
        {
            return Problem(detail: result.Error, statusCode: result.Status);
        }
        await HttpContext.SignOutAsync();
        return NoContent();
    }
}
