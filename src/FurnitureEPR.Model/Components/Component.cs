namespace FurnitureEPR.Model.Components;

public sealed class Component
{
    private Component() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public ICollection<Products.ProductComponent> ProductComponents { get; private set; } = new List<Products.ProductComponent>();

    public Component(string name)
    {
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Component name is required.", nameof(name));

        Name = name.Trim();
    }
}
