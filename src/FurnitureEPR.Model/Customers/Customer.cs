namespace FurnitureEPR.Model.Customers;

public sealed class Customer
{
    private Customer() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }

    public Customer(string name, string? phoneNumber = null, string? address = null)
    {
        SetName(name);
        PhoneNumber = Normalize(phoneNumber);
        Address = Normalize(address);
    }

    public void UpdateContact(string? phoneNumber, string? address)
    {
        PhoneNumber = Normalize(phoneNumber);
        Address = Normalize(address);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));

        Name = name.Trim();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
