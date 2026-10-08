using FurnitureEPR.Application.Features.Orders.Queries;

namespace FurnitureEPR.Application.Features.Orders;

public interface IProductionReadRepository
{
    Task<PagedProductionTaskDto> GetTasksAsync(
        int page,
        int pageSize,
        string? search,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken);
}