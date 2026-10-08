using FurnitureEPR.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/identity")]
[Authorize(Policy = PermissionNames.IdentityManageUsers)]
public sealed class IdentityController : ControllerBase
{
    private readonly IIdentityManagementService _identityService;

    public IdentityController(IIdentityManagementService identityService)
        => _identityService = identityService;

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<IdentityUserDto>>> GetUsers(
        CancellationToken cancellationToken)
        => Ok(await _identityService.GetUsersAsync(cancellationToken));

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<IdentityUserDto>> GetUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("users")]
    public async Task<ActionResult<IdentityUserDto>> CreateUser(
        CreateIdentityUserRequest request,
        CancellationToken cancellationToken)
        => Ok(await _identityService.CreateUserAsync(request, cancellationToken));

    [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _identityService.AssignRoleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _identityService.RemoveRoleAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/permissions")]
    public async Task<IActionResult> AddUserPermission(
        Guid userId,
        PermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _identityService.AddUserPermissionAsync(
            userId,
            request.Permission,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/permissions")]
    public async Task<IActionResult> RemoveUserPermission(
        Guid userId,
        PermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _identityService.RemoveUserPermissionAsync(
            userId,
            request.Permission,
            cancellationToken);

        return NoContent();
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyCollection<IdentityRoleDto>>> GetRoles(
        CancellationToken cancellationToken)
        => Ok(await _identityService.GetRolesAsync(cancellationToken));

    [HttpPost("roles")]
    [Authorize(Policy = PermissionNames.IdentityManageRoles)]
    public async Task<ActionResult<IdentityRoleDto>> CreateRole(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
        => Ok(await _identityService.CreateRoleAsync(request.Name, cancellationToken));

    [HttpPost("roles/{roleId:guid}/permissions")]
    [Authorize(Policy = PermissionNames.IdentityManageClaims)]
    public async Task<IActionResult> AddRolePermission(
        Guid roleId,
        PermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _identityService.AddRolePermissionAsync(
            roleId,
            request.Permission,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("roles/{roleId:guid}/permissions")]
    [Authorize(Policy = PermissionNames.IdentityManageClaims)]
    public async Task<IActionResult> RemoveRolePermission(
        Guid roleId,
        PermissionRequest request,
        CancellationToken cancellationToken)
    {
        await _identityService.RemoveRolePermissionAsync(
            roleId,
            request.Permission,
            cancellationToken);

        return NoContent();
    }
}

public sealed record CreateRoleRequest(string Name);

public sealed record PermissionRequest(string Permission);
