using FurnitureEPR.Model.Customers;
using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Model.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderWorkflowInstance> _workflows = new();

    private Order() { }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? FinalizedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    // مبالغ سفارش به‌صورت Snapshot ذخیره می‌شوند تا تغییرات بعدی قیمت محصول، سفارش قبلی را تغییر ندهد.
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderWorkflowInstance> Workflows => _workflows.AsReadOnly();

    public Order(Guid customerId, Guid? createdByUserId = null)
    {
        CustomerId = customerId;
        CreatedByUserId = createdByUserId;
        Status = OrderStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetOrderNumber(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required.", nameof(orderNumber));

        OrderNumber = orderNumber.Trim();
    }

    public void ReplaceItems(IEnumerable<OrderItem> items)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        _items.Clear();
        foreach (var item in items)
            AddItem(item);
    }

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

    // تخفیف نباید از مبلغ کل سفارش بیشتر باشد و مبلغ نهایی در سمت سرور محاسبه می‌شود.
    public void RecalculateAmounts(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(discountAmount));

        TotalAmount = _items.Sum(x => x.TotalPrice);

        if (discountAmount > TotalAmount)
            throw new InvalidOperationException("Discount amount cannot be greater than total amount.");

        DiscountAmount = discountAmount;
        FinalAmount = TotalAmount - DiscountAmount;
    }

    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be finalized.");

        if (_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item.");

        if (string.IsNullOrWhiteSpace(OrderNumber))
            throw new InvalidOperationException("Order number must be assigned before finalization.");

        RecalculateAmounts(DiscountAmount);
        Status = OrderStatus.Active;
        FinalizedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status != OrderStatus.Active)
            throw new InvalidOperationException("Only active orders can be completed.");

        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
