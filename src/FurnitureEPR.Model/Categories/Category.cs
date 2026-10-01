namespace FurnitureEPR.Model.Categories;

public sealed class Category
{
    private Category() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public ICollection<Products.Product> Products { get; private set; } = new List<Products.Product>();

    public Category(string name)
    {
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
    }
}
