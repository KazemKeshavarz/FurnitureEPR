namespace FurnitureEPR.Model.Orders;

/// <summary>
/// درخواست انتقال کالای آماده بین دو سفارش؛ تأیید آن باعث جابه‌جایی تخصیص انبار می‌شود.
/// </summary>
public sealed class OrderTransfer
{
    private readonly List<OrderTransferLine> _lines = new();

    private OrderTransfer() { }

    public Guid Id { get; private set; }
    public Guid SourceOrderId { get; private set; }
    public Guid TargetOrderId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string Reason { get; private set; } = null!;
    public OrderTransferStatus Status { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    public Order SourceOrder { get; private set; } = null!;
    public Order TargetOrder { get; private set; } = null!;
    public IReadOnlyCollection<OrderTransferLine> Lines => _lines.AsReadOnly();

    public OrderTransfer(Guid sourceOrderId, Guid targetOrderId, Guid requestedByUserId, string reason)
    {
        if (sourceOrderId == Guid.Empty || targetOrderId == Guid.Empty || sourceOrderId == targetOrderId)
            throw new ArgumentException("سفارش مبدأ و مقصد باید معتبر و متفاوت باشند.");
        if (requestedByUserId == Guid.Empty)
            throw new ArgumentException("ثبت‌کننده درخواست مشخص نیست.", nameof(requestedByUserId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("دلیل انتقال الزامی است.", nameof(reason));

        Id = Guid.NewGuid();
        SourceOrderId = sourceOrderId;
        TargetOrderId = targetOrderId;
        RequestedByUserId = requestedByUserId;
        Reason = reason.Trim();
        Status = OrderTransferStatus.PendingApproval;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public void AddLine(Guid sourceOrderItemId, Guid targetOrderItemId, Guid warehouseId, decimal quantity)
    {
        if (Status != OrderTransferStatus.PendingApproval)
            throw new InvalidOperationException("درخواست بررسی‌شده قابل ویرایش نیست.");
        if (sourceOrderItemId == Guid.Empty || targetOrderItemId == Guid.Empty || warehouseId == Guid.Empty || quantity <= 0)
            throw new ArgumentException("قلم، انبار و تعداد انتقال باید معتبر باشند.");
        if (_lines.Any(x => x.SourceOrderItemId == sourceOrderItemId && x.TargetOrderItemId == targetOrderItemId && x.WarehouseId == warehouseId))
            throw new InvalidOperationException("قلم انتقال تکراری است.");

        _lines.Add(new OrderTransferLine(Id, sourceOrderItemId, targetOrderItemId, warehouseId, quantity));
    }

    public void Approve(Guid reviewerUserId)
    {
        EnsurePending();
        ReviewedByUserId = reviewerUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        Status = OrderTransferStatus.Approved;
    }

    public void Reject(Guid reviewerUserId)
    {
        EnsurePending();
        ReviewedByUserId = reviewerUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        Status = OrderTransferStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != OrderTransferStatus.PendingApproval)
            throw new InvalidOperationException("این درخواست قبلاً بررسی شده است.");
    }
}
