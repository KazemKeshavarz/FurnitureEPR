namespace FurnitureEPR.Model.Warehouses;

public sealed class OrderItemWarehouseAllocation
{
    private OrderItemWarehouseAllocation() { }

    public Guid Id { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    public Orders.OrderItem OrderItem { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    public OrderItemWarehouseAllocation(Guid orderItemId, Guid warehouseId, decimal quantity)
    {
        if (orderItemId == Guid.Empty) throw new ArgumentException("Order item is required.", nameof(orderItemId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("Warehouse is required.", nameof(warehouseId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Id = Guid.NewGuid();
        OrderItemId = orderItemId;
        WarehouseId = warehouseId;
        Quantity = quantity;
        AssignedAtUtc = DateTime.UtcNow;
    }
}
