using FurnitureEPR.Application.Features.Orders.Queries;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedOrderDto> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        FurnitureEPR.Model.Orders.OrderStatus? status,
        CancellationToken cancellationToken);
}
