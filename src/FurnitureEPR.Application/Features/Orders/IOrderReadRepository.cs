using FurnitureEPR.Application.Features.Orders.Queries;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
