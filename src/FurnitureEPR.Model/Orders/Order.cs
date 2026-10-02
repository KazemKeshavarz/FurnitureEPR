using FurnitureEPR.Model.Customers;

namespace FurnitureEPR.Model.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    private Order() { }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? FinalizedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(Guid customerId, Guid? createdByUserId = null)
    {
        CustomerId = customerId;
        CreatedByUserId = createdByUserId;
        Status = OrderStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // شماره سفارش قبل از Finalize شدن باید تعیین شود تا سفارش وارد چرخه عملیاتی شود.
    public void SetOrderNumber(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required.", nameof(orderNumber));

        OrderNumber = orderNumber.Trim();
    }

    // آیتم‌های سفارش فقط تا زمانی قابل تغییر هستند که سفارش Draft باشد.
    public void ReplaceItems(IEnumerable<OrderItem> items)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        _items.Clear();
        foreach (var item in items)
            AddItem(item);
    }

    // مشتری سفارش نیز فقط در مرحله Draft قابل تغییر است.
    public void ChangeCustomer(Guid customerId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        CustomerId = customerId;
    }

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        _items.Add(item);
    }

    // Finalize مرز بین سفارش قابل ویرایش و سفارش عملیاتی است.
    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be finalized.");

        if (_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item.");

        if (string.IsNullOrWhiteSpace(OrderNumber))
            throw new InvalidOperationException("Order number must be assigned before finalization.");

        Status = OrderStatus.Active;
        FinalizedAtUtc = DateTime.UtcNow;
    }

    // سفارش تکمیل‌شده قابل لغو نیست، اما سایر وضعیت‌های غیرتکمیل‌شده می‌توانند لغو شوند.
    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
