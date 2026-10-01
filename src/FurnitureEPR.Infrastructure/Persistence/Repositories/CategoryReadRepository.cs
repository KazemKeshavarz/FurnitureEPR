using FurnitureEPR.Application.Features.Categories;
using FurnitureEPR.Application.Features.Categories.Queries;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryReadRepository(ApplicationDbContext db) => _db = db;

    public Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.Categories.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CategoryDto(x.Id, x.Name))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<CategoryListItemDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x => x.Name.Contains(value));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new CategoryListItemDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);

        return new PagedResult<CategoryListItemDto>(items, page, pageSize, total);
    }
}
