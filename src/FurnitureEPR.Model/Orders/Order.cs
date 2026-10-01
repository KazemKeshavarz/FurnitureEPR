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

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
