using FurnitureEPR.Application.Features.Categories.Queries;

namespace FurnitureEPR.Application.Features.Categories;

public interface ICategoryReadRepository
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<CategoryListItemDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
}
