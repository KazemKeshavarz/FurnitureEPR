using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;
using FurnitureEPR.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db) => _db = db;

    public async Task<Order> CreateAsync(
        Guid customerId,
        Guid? createdByUserId,
        IReadOnlyCollection<CreateOrderItem> items,
        CancellationToken cancellationToken)
    {
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

        var order = new Order(customerId, createdByUserId);

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            var orderItem = new OrderItem(product.Id, product.Name, item.Quantity);

            var requestedComponents = item.Components.ToDictionary(
                x => x.ComponentId,
                x => x.Quantity);

            foreach (var productComponent in product.Components)
            {
                var quantity = requestedComponents.TryGetValue(
                    productComponent.ComponentId,
                    out var requestedQuantity)
                    ? requestedQuantity
                    : productComponent.DefaultQuantity;

                orderItem.AddComponent(
                    productComponent.ComponentId,
                    productComponent.Component.Name,
                    quantity);
            }

            var unknownComponent = requestedComponents.Keys
                .Except(product.Components.Select(x => x.ComponentId))
                .FirstOrDefault();

            if (unknownComponent != Guid.Empty)
                throw new InvalidOperationException(
                    "An order item contains a component that is not assigned to the selected product.");

            order.AddItem(orderItem);
        }

        await _db.Orders.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return order;
    }
}
