using FurnitureEPR.Model.Orders;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Guid customerId, Guid? createdByUserId, IReadOnlyCollection<Commands.CreateOrder.CreateOrderItem> items, CancellationToken cancellationToken);
}
