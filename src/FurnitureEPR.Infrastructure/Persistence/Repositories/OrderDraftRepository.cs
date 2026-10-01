using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;
using FurnitureEPR.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderDraftRepository : IOrderDraftRepository
{
    private readonly ApplicationDbContext _db;

    public OrderDraftRepository(ApplicationDbContext db) => _db = db;

    public async Task UpdateAsync(
        Guid orderId,
        Guid customerId,
        IReadOnlyCollection<UpdateDraftOrderItem> items,
        CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Components)
            .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        if (order.Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");

        var customerExists = await _db.Customers
            .AnyAsync(x => x.Id == customerId, cancellationToken);

        if (!customerExists)
            throw new KeyNotFoundException("Customer was not found.");

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Include(x => x.Components)
            .ThenInclude(x => x.Component)
            .Where(x => productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (products.Count != productIds.Count)
            throw new KeyNotFoundException("One or more products were not found.");

        var replacementItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            var requested = item.Components.ToDictionary(x => x.ComponentId, x => x.Quantity);

            var unknown = requested.Keys
                .Except(product.Components.Select(x => x.ComponentId))
                .FirstOrDefault();

            if (unknown != Guid.Empty)
                throw new InvalidOperationException(
                    "An order item contains a component that is not assigned to the selected product.");

            var orderItem = new OrderItem(product.Id, product.Name, item.Quantity);

            foreach (var productComponent in product.Components)
            {
                var quantity = requested.TryGetValue(
                    productComponent.ComponentId,
                    out var requestedQuantity)
                    ? requestedQuantity
                    : productComponent.DefaultQuantity;

                orderItem.AddComponent(
                    productComponent.ComponentId,
                    productComponent.Component.Name,
                    quantity);
            }

            replacementItems.Add(orderItem);
        }

        order.ChangeCustomer(customerId);
        order.ReplaceItems(replacementItems);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
