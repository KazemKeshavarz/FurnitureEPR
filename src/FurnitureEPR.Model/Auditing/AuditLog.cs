namespace FurnitureEPR.Model.Auditing;

public sealed class AuditLog
{
    private AuditLog() { }

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Method { get; private set; } = null!;
    public string Path { get; private set; } = null!;
    public int StatusCode { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    public AuditLog(
        Guid? userId,
        string method,
        string path,
        int statusCode)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Method = Required(method, nameof(method), 20);
        Path = Required(path, nameof(path), 500);
        StatusCode = statusCode;
        OccurredAtUtc = DateTime.UtcNow;
    }

    private static string Required(string value, string parameter, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", parameter);

        value = value.Trim();

        if (value.Length > maxLength)
            throw new ArgumentException("Value is too long.", parameter);

        return value;
    }
}
