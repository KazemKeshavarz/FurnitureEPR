namespace FurnitureEPR.Infrastructure.Identity;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "IdentitySeed";

    public bool Enabled { get; set; }
    public string AdminUserName { get; set; } = null!;
    public string AdminEmail { get; set; } = null!;
    public string AdminPassword { get; set; } = null!;
}
