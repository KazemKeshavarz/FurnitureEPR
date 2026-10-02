namespace FurnitureEPR.Application.Security;

public interface IIdentityService
{
    Task<AuthenticationResult?> AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken);
}

public sealed record AuthenticationResult(
    string AccessToken,
    DateTime ExpiresAtUtc);
