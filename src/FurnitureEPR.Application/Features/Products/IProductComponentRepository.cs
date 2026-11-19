namespace FurnitureEPR.Application.Features.Products;
public interface IProductComponentRepository
{
    Task AddAsync(Guid productId, Guid componentId, decimal defaultQuantity, CancellationToken cancellationToken);
    Task UpdateDefaultQuantityAsync(Guid productId, Guid productComponentId, decimal defaultQuantity, CancellationToken cancellationToken);
}
