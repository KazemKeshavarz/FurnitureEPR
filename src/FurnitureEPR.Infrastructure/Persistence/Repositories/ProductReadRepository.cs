using FurnitureEPR.Application.Features.Products;
using FurnitureEPR.Application.Features.Products.Queries;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class ProductReadRepository : IProductReadRepository
{
    private readonly ApplicationDbContext _db;
    public ProductReadRepository(ApplicationDbContext db) => _db = db;

    public Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.Products.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new ProductDto(
                x.Id, x.Name, x.CategoryId, x.Category.Name,
                x.Components.Select(c => new ProductComponentDto(
                    c.Id, c.ComponentId, c.Component.Name, c.DefaultQuantity)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<ProductListItemDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? categoryId, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x => x.Name.Contains(value));
        }

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ProductListItemDto(x.Id, x.Name, x.CategoryId, x.Category.Name))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(items, page, pageSize, total);
    }
}
