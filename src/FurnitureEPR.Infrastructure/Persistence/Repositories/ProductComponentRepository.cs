using FurnitureEPR.Application.Features.Products;
using FurnitureEPR.Model.Products;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class ProductComponentRepository : IProductComponentRepository
{
    private readonly ApplicationDbContext _db;

    public ProductComponentRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(
        Guid productId,
        Guid componentId,
        decimal defaultQuantity,
        CancellationToken cancellationToken)
    {
        var exists = await _db.Products
            .AnyAsync(x => x.Id == productId, cancellationToken);

        if (!exists)
            throw new KeyNotFoundException("Product was not found.");

        var componentExists = await _db.Components
            .AnyAsync(x => x.Id == componentId, cancellationToken);

        if (!componentExists)
            throw new KeyNotFoundException("Component was not found.");

        var duplicate = await _db.ProductComponents
            .AnyAsync(x => x.ProductId == productId && x.ComponentId == componentId, cancellationToken);

        if (duplicate)
            throw new InvalidOperationException("This component is already assigned to the product.");

        var productComponent = new ProductComponent(productId, componentId, defaultQuantity);
        await _db.ProductComponents.AddAsync(productComponent, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
