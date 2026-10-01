using FurnitureEPR.Model.Components;

namespace FurnitureEPR.Model.Products;

public sealed class ProductComponent
{
    private ProductComponent() { }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ComponentId { get; private set; }
    public decimal DefaultQuantity { get; private set; }

    public Product Product { get; private set; } = null!;
    public Component Component { get; private set; } = null!;

    public ProductComponent(Guid productId, Guid componentId, decimal defaultQuantity)
    {
        if (defaultQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(defaultQuantity));

        ProductId = productId;
        ComponentId = componentId;
        DefaultQuantity = defaultQuantity;
    }

    public void SetDefaultQuantity(decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        DefaultQuantity = quantity;
    }
}
