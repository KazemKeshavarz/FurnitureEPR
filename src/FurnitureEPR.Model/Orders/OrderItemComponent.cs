namespace FurnitureEPR.Model.Orders;

public sealed class OrderItemComponent
{
    private OrderItemComponent() { }

    public Guid Id { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ComponentId { get; private set; }
    public string ComponentName { get; private set; } = null!;
    public decimal Quantity { get; private set; }

    public OrderItem OrderItem { get; private set; } = null!;

    internal OrderItemComponent(Guid componentId, string componentName, decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ComponentId = componentId;
        ComponentName = string.IsNullOrWhiteSpace(componentName)
            ? throw new ArgumentException("Component name is required.", nameof(componentName))
            : componentName.Trim();
        Quantity = quantity;
    }
}
