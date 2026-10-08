namespace FurnitureEPR.Application.Security;

public interface IIdentityManagementService
{
    Task<IReadOnlyCollection<IdentityUserDto>> GetUsersAsync(
        CancellationToken cancellationToken);

    Task<IdentityUserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IdentityRoleDto> CreateRoleAsync(
        string name,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<IdentityRoleDto>> GetRolesAsync(
        CancellationToken cancellationToken);

    Task<IdentityUserDto> CreateUserAsync(
        CreateIdentityUserRequest request,
        CancellationToken cancellationToken);

    Task AssignRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken);

    Task RemoveRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken);

    Task AddRolePermissionAsync(
        Guid roleId,
        string permission,
        CancellationToken cancellationToken);

    Task RemoveRolePermissionAsync(
        Guid roleId,
        string permission,
        CancellationToken cancellationToken);

    Task AddUserPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken);

    Task RemoveUserPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken);
}

public sealed record CreateIdentityUserRequest(
    string UserName,
    string Email,
    string Password);

public sealed record IdentityRoleDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Permissions);

public sealed record IdentityUserDto(
    Guid Id,
    string UserName,
    string? Email,
    bool EmailConfirmed,
    bool LockedOut,
    IReadOnlyCollection<IdentityRoleDto> Roles,
    IReadOnlyCollection<string> DirectPermissions);
