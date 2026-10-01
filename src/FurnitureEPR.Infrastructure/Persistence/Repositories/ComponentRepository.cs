using FurnitureEPR.Application.Features.Components;
using FurnitureEPR.Model.Components;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class ComponentRepository : IComponentRepository
{
    private readonly ApplicationDbContext _db;
    public ComponentRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(Component component, CancellationToken cancellationToken)
    {
        await _db.Components.AddAsync(component, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
