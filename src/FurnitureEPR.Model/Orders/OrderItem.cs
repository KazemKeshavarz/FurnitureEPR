using FurnitureEPR.Model.Products;

namespace FurnitureEPR.Model.Orders;

public sealed class OrderItem
{
    private readonly List<OrderItemComponent> _components = new();

    private OrderItem() { }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public decimal Quantity { get; private set; }

    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public IReadOnlyCollection<OrderItemComponent> Components => _components.AsReadOnly();

    public OrderItem(Guid productId, string productName, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductId = productId;
        ProductName = NormalizeRequired(productName, nameof(productName));
        Quantity = quantity;
    }

    public void AddComponent(Guid componentId, string componentName, decimal quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        _components.Add(new OrderItemComponent(
            componentId,
            componentName,
            quantity));
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", paramName);

        return value.Trim();
    }
}
