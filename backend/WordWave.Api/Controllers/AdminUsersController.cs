using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWave.Api.Models.Requests;
using WordWave.Application.Interfaces;
using WordWave.Domain.Models;

namespace WordWave.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminUsersController : ApiControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _currentUser;
    public AdminUsersController(IAuthService auth, ICurrentUser currentUser)
    {
        _auth = auth;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery, StringLength(100)] string? search, [FromQuery, Range(1, 100000)] int page = 1, [FromQuery, Range(1, 100)] int limit = 20) => Ok(await _auth.GetUsersAsync(search, page, limit));

    [HttpPut("{id:guid}/access")]
    public async Task<IActionResult> SetAccess(Guid id, UserAccessRequest request)
    {
        var result = await _auth.SetAccessAsync(_currentUser.Id, id, request.Role, request.IsActive!.Value);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminCreateUserRequest request)
    {
        var result = await _auth.CreateUserAsync(request.Email, request.DisplayName, request.Password, request.Role, request.IsActive);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : StatusCode(201, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AdminUpdateUserRequest request)
    {
        var result = await _auth.UpdateUserAsync(_currentUser.Id, id, request.Email, request.DisplayName, request.Role, request.IsActive!.Value);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _auth.DeleteUserAsync(_currentUser.Id, id);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : NoContent();
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ResetPassword(Guid id, AdminResetPasswordRequest request)
    {
        var result = await _auth.ResetPasswordAsync(_currentUser.Id, id, request.NewPassword);
        return result.Value is null ? Problem(detail: result.Error, statusCode: result.Status) : NoContent();
    }
}
