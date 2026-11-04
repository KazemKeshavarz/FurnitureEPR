namespace FurnitureEPR.Model.Orders;

public sealed class OrderTransferLine
{
    private OrderTransferLine() { }

    public Guid Id { get; private set; }
    public Guid OrderTransferId { get; private set; }
    public Guid SourceOrderItemId { get; private set; }
    public Guid TargetOrderItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }

    public OrderTransfer Transfer { get; private set; } = null!;
    public OrderItem SourceOrderItem { get; private set; } = null!;
    public OrderItem TargetOrderItem { get; private set; } = null!;
    public Warehouses.Warehouse Warehouse { get; private set; } = null!;

    internal OrderTransferLine(Guid transferId, Guid sourceItemId, Guid targetItemId, Guid warehouseId, decimal quantity)
    {
        Id = Guid.NewGuid();
        OrderTransferId = transferId;
        SourceOrderItemId = sourceItemId;
        TargetOrderItemId = targetItemId;
        WarehouseId = warehouseId;
        Quantity = quantity;
    }
}
