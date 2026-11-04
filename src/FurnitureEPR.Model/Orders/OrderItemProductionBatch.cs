namespace FurnitureEPR.Model.Orders;

/// <summary>
/// سهم مستقلی از تعداد یک قلم سفارش؛ جزئیات فنی آن در رابط کاربری نمایش داده نمی‌شود.
/// </summary>
public sealed class OrderItemProductionBatch
{
    private OrderItemProductionBatch() { }

    public Guid Id { get; private set; }
    public Guid OrderItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public ProductionBatchOrigin Origin { get; private set; }
    public ProductionBatchStatus Status { get; private set; }
    public Guid? SourceTransferLineId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadyAtUtc { get; private set; }

    public OrderItem OrderItem { get; private set; } = null!;

    public OrderItemProductionBatch(Guid orderItemId, decimal quantity, ProductionBatchOrigin origin, ProductionBatchStatus status = ProductionBatchStatus.Planned, Guid? sourceTransferLineId = null)
    {
        if (orderItemId == Guid.Empty) throw new ArgumentException("قلم سفارش الزامی است.", nameof(orderItemId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (origin == ProductionBatchOrigin.Transfer && sourceTransferLineId is null)
            throw new ArgumentException("شناسه ردیف انتقال برای کالای منتقل‌شده الزامی است.", nameof(sourceTransferLineId));

        Id = Guid.NewGuid();
        OrderItemId = orderItemId;
        Quantity = quantity;
        Origin = origin;
        Status = status;
        SourceTransferLineId = sourceTransferLineId;
        CreatedAtUtc = DateTime.UtcNow;
        if (status == ProductionBatchStatus.Ready) ReadyAtUtc = DateTime.UtcNow;
    }

    public void ReduceQuantity(decimal quantity)
    {
        if (Status is ProductionBatchStatus.Ready or ProductionBatchStatus.InProgress or ProductionBatchStatus.Planned)
        {
            if (quantity <= 0 || quantity > Quantity) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity -= quantity;
            if (Quantity == 0) Status = ProductionBatchStatus.Cancelled;
        }
        else throw new InvalidOperationException("این بخش از سفارش قابل کاهش نیست.");
    }

    public void MarkInProgress()
    {
        if (Status != ProductionBatchStatus.Planned) throw new InvalidOperationException("فقط مقدار برنامه‌ریزی‌شده وارد تولید می‌شود.");
        Status = ProductionBatchStatus.InProgress;
    }

    public void MarkReady()
    {
        if (Status != ProductionBatchStatus.InProgress && Status != ProductionBatchStatus.Planned)
            throw new InvalidOperationException("وضعیت این بخش قابل آماده‌سازی نیست.");
        Status = ProductionBatchStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
    }
}
