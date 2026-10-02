using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
        => _identityService = identityService;

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        return Ok(new CurrentUserResponse(
            User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier),
            User.FindFirstValue(System.Security.Claims.ClaimTypes.Name),
            User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(x => x.Value).Distinct().ToArray(),
            User.FindAll(IdentityClaimTypes.RoleId).Select(x => x.Value).Distinct().ToArray()));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.AuthenticateAsync(
            request.UserName,
            request.Password,
            cancellationToken);

        if (result is null)
            return Unauthorized("Invalid username or password.");

        return Ok(new LoginResponse(
            result.AccessToken,
            result.ExpiresAtUtc));
    }
}

public sealed record LoginRequest(
    string UserName,
    string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);

public sealed record CurrentUserResponse(
    string? UserId,
    string? UserName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> RoleIds);
