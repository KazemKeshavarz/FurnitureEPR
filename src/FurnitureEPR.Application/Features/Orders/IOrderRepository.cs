using FurnitureEPR.Model.Orders;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
}
