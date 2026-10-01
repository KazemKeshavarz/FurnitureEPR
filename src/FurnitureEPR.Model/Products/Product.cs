using FurnitureEPR.Model.Categories;

namespace FurnitureEPR.Model.Products;

public sealed class Product
{
    private Product() { }

    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;

    public Category Category { get; private set; } = null!;
    public ICollection<ProductComponent> Components { get; private set; } = new List<ProductComponent>();

    public Product(Guid categoryId, string name)
    {
        CategoryId = categoryId;
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        Name = name.Trim();
    }
}
