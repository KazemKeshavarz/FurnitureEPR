using FurnitureEPR.Application.Security;
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
