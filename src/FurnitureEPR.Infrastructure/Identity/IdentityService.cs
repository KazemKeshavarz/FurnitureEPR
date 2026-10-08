using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FurnitureEPR.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FurnitureEPR.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtOptions _options;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtOptions> options)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _options = options.Value;
    }

    public async Task<AuthenticationResult?> AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
            return null;

        // استفاده از SignInManager باعث می‌شود Lockout و شمارش ورودهای ناموفق Identity رعایت شود.
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString())
        };

        // Claimهای مستقیم کاربر بخشی از Token می‌شوند.
        foreach (var userClaim in userClaims)
            claims.Add(userClaim);

        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            claims.Add(new Claim(IdentityClaimTypes.RoleId, role.Id.ToString()));

            // Claimهای تعریف‌شده روی Role نیز به کاربر منتقل می‌شوند.
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            claims.AddRange(roleClaims);
        }

        // از ایجاد Claim تکراری در Token جلوگیری می‌کنیم.
        claims = claims
            .GroupBy(x => new { x.Type, x.Value })
            .Select(x => x.First())
            .ToList();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthenticationResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }
}
