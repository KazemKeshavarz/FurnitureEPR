namespace FurnitureEPR.Model.Warehouses;

public sealed class Warehouse
{
    private Warehouse() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Warehouse(string name, string code)
    {
        Id = Guid.NewGuid();
        Update(name, code);
        IsActive = true;
    }

    public void Update(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code is required.", nameof(code));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
