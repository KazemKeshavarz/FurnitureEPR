using FurnitureEPR.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FurnitureEPR.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private const string AdministratorRole = "Administrator";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentitySeedOptions _options;

    public IdentitySeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentitySeedOptions> options)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        if (!_options.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(_options.AdminUserName)
            || string.IsNullOrWhiteSpace(_options.AdminEmail)
            || string.IsNullOrWhiteSpace(_options.AdminPassword))
        {
            throw new InvalidOperationException(
                "Identity seed configuration is incomplete.");
        }

        var role = await _roleManager.FindByNameAsync(AdministratorRole);
        if (role is null)
        {
            role = new ApplicationRole
            {
                Name = AdministratorRole
            };

            var roleResult = await _roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        await EnsureAdministratorPermissionsAsync(role);

        var user = await _userManager.FindByNameAsync(_options.AdminUserName);
        if (user is not null)
            return;

        user = new ApplicationUser
        {
            UserName = _options.AdminUserName,
            Email = _options.AdminEmail,
            EmailConfirmed = true
        };

        var userResult = await _userManager.CreateAsync(
            user,
            _options.AdminPassword);

        if (!userResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", userResult.Errors.Select(x => x.Description)));
        }

        var addRoleResult = await _userManager.AddToRoleAsync(
            user,
            AdministratorRole);

        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
        }
    }
}
