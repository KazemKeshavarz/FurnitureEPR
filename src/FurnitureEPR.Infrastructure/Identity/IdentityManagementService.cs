using System.Security.Claims;
using FurnitureEPR.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace FurnitureEPR.Infrastructure.Identity;

public sealed class IdentityManagementService : IIdentityManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<IdentityUserDto>> GetUsersAsync(
        CancellationToken cancellationToken)
    {
        var users = _userManager.Users
            .OrderBy(x => x.UserName)
            .ToList();

        var result = new List<IdentityUserDto>(users.Count);

        foreach (var user in users)
            result.Add(await MapUserAsync(user));

        return result;
    }

    public async Task<IdentityUserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await MapUserAsync(user);
    }

    public async Task<IdentityRoleDto> CreateRoleAsync(
        string name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("نام نقش الزامی است.", nameof(name));

        name = name.Trim();

        if (await _roleManager.RoleExistsAsync(name))
            throw new InvalidOperationException("این نقش قبلاً ایجاد شده است.");

        var role = new ApplicationRole { Name = name };
        var result = await _roleManager.CreateAsync(role);

        EnsureSucceeded(result, "ایجاد نقش");

        return new IdentityRoleDto(role.Id, role.Name!, Array.Empty<string>());
    }

    public async Task<IReadOnlyCollection<IdentityRoleDto>> GetRolesAsync(
        CancellationToken cancellationToken)
    {
        var roles = _roleManager.Roles
            .OrderBy(x => x.Name)
            .ToList();

        var result = new List<IdentityRoleDto>(roles.Count);

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);

            result.Add(new IdentityRoleDto(
                role.Id,
                role.Name!,
                claims
                    .Where(x => x.Type == IdentityClaimTypes.Permission)
                    .Select(x => x.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToArray()));
        }

        return result;
    }

    public async Task<IdentityUserDto> CreateUserAsync(
        CreateIdentityUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ArgumentException("نام کاربری الزامی است.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("رمز عبور الزامی است.", nameof(request));

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        EnsureSucceeded(result, "ایجاد کاربر");

        return await MapUserAsync(user);
    }

    public async Task AssignRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserOrThrowAsync(userId);
        var role = await GetRoleOrThrowAsync(roleId);

        if (await _userManager.IsInRoleAsync(user, role.Name!))
            return;

        EnsureSucceeded(
            await _userManager.AddToRoleAsync(user, role.Name!),
            "اختصاص نقش به کاربر");
    }

    public async Task RemoveRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserOrThrowAsync(userId);
        var role = await GetRoleOrThrowAsync(roleId);

        if (!await _userManager.IsInRoleAsync(user, role.Name!))
            return;

        EnsureSucceeded(
            await _userManager.RemoveFromRoleAsync(user, role.Name!),
            "حذف نقش از کاربر");
    }

    public async Task AddRolePermissionAsync(
        Guid roleId,
        string permission,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleOrThrowAsync(roleId);
        permission = NormalizePermission(permission);

        var claims = await _roleManager.GetClaimsAsync(role);
        if (claims.Any(x =>
            x.Type == IdentityClaimTypes.Permission
            && x.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        EnsureSucceeded(
            await _roleManager.AddClaimAsync(
                role,
                new Claim(IdentityClaimTypes.Permission, permission)),
            "اختصاص Permission به نقش");
    }

    public async Task RemoveRolePermissionAsync(
        Guid roleId,
        string permission,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleOrThrowAsync(roleId);
        permission = NormalizePermission(permission);

        var claims = await _roleManager.GetClaimsAsync(role);

        foreach (var claim in claims.Where(x =>
                     x.Type == IdentityClaimTypes.Permission
                     && x.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)))
        {
            EnsureSucceeded(
                await _roleManager.RemoveClaimAsync(role, claim),
                "حذف Permission از نقش");
        }
    }

    public async Task AddUserPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken)
    {
        var user = await GetUserOrThrowAsync(userId);
        permission = NormalizePermission(permission);

        var claims = await _userManager.GetClaimsAsync(user);
        if (claims.Any(x =>
            x.Type == IdentityClaimTypes.Permission
            && x.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        EnsureSucceeded(
            await _userManager.AddClaimAsync(
                user,
                new Claim(IdentityClaimTypes.Permission, permission)),
            "اختصاص Permission مستقیم به کاربر");
    }

    public async Task RemoveUserPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken)
    {
        var user = await GetUserOrThrowAsync(userId);
        permission = NormalizePermission(permission);

        var claims = await _userManager.GetClaimsAsync(user);

        foreach (var claim in claims.Where(x =>
                     x.Type == IdentityClaimTypes.Permission
                     && x.Value.Equals(permission, StringComparison.OrdinalIgnoreCase)))
        {
            EnsureSucceeded(
                await _userManager.RemoveClaimAsync(user, claim),
                "حذف Permission مستقیم از کاربر");
        }
    }

    private async Task<IdentityUserDto> MapUserAsync(ApplicationUser user)
    {
        var roleNames = await _userManager.GetRolesAsync(user);
        var directClaims = await _userManager.GetClaimsAsync(user);

        var roles = new List<IdentityRoleDto>(roleNames.Count);

        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            var claims = await _roleManager.GetClaimsAsync(role);

            roles.Add(new IdentityRoleDto(
                role.Id,
                role.Name!,
                claims
                    .Where(x => x.Type == IdentityClaimTypes.Permission)
                    .Select(x => x.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToArray()));
        }

        return new IdentityUserDto(
            user.Id,
            user.UserName!,
            user.Email,
            user.EmailConfirmed,
            await _userManager.IsLockedOutAsync(user),
            roles,
            directClaims
                .Where(x => x.Type == IdentityClaimTypes.Permission)
                .Select(x => x.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray());
    }

    private async Task<ApplicationUser> GetUserOrThrowAsync(Guid userId)
        => await _userManager.FindByIdAsync(userId.ToString())
           ?? throw new KeyNotFoundException("کاربر پیدا نشد.");

    private async Task<ApplicationRole> GetRoleOrThrowAsync(Guid roleId)
        => await _roleManager.FindByIdAsync(roleId.ToString())
           ?? throw new KeyNotFoundException("نقش پیدا نشد.");

    private static string NormalizePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission الزامی است.", nameof(permission));

        return permission.Trim().ToLowerInvariant();
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
            return;

        throw new InvalidOperationException(
            $"{operation} انجام نشد: " +
            string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
