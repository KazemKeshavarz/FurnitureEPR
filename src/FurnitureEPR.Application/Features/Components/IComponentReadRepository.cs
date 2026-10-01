using FurnitureEPR.Application.Features.Components.Queries;

namespace FurnitureEPR.Application.Features.Components;

public interface IComponentReadRepository
{
    Task<ComponentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ComponentListItemDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
}
