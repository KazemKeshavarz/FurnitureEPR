namespace FurnitureEPR.Application.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    IReadOnlyCollection<Guid> RoleIds { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }

    bool HasPermission(string permission);
}
