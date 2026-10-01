using FurnitureEPR.Application.Features.Components;
using FurnitureEPR.Application.Features.Components.Queries;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class ComponentReadRepository : IComponentReadRepository
{
    private readonly ApplicationDbContext _db;
    public ComponentReadRepository(ApplicationDbContext db) => _db = db;

    public Task<ComponentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.Components.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new ComponentDto(x.Id, x.Name))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<ComponentListItemDto>> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Components.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x => x.Name.Contains(value));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ComponentListItemDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);

        return new PagedResult<ComponentListItemDto>(items, page, pageSize, total);
    }
}
