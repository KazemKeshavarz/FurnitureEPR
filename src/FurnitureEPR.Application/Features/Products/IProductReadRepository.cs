using FurnitureEPR.Application.Features.Products.Queries;

namespace FurnitureEPR.Application.Features.Products;

public interface IProductReadRepository
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ProductListItemDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? categoryId, CancellationToken cancellationToken);
}
