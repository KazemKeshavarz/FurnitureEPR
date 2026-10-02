using System.Security.Claims;
using FurnitureEPR.Application.Security;
using Microsoft.AspNetCore.Http;

namespace FurnitureEPR.Infrastructure.Identity;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal Principal
        => _httpContextAccessor.HttpContext?.User
            ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated
        => Principal.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var value = Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public IReadOnlyCollection<Guid> RoleIds
        => Principal.FindAll(IdentityClaimTypes.RoleId)
            .Select(x => Guid.TryParse(x.Value, out var roleId) ? roleId : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

    public IReadOnlyCollection<string> Roles
        => Principal.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
