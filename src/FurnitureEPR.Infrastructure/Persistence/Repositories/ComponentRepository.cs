using FurnitureEPR.Application.Features.Components;
using FurnitureEPR.Model.Components;
using Microsoft.EntityFrameworkCore;

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

    public async Task UpdateAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var component = await _db.Components.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Component was not found.");
        component.SetName(name);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
