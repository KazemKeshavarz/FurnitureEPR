using FurnitureEPR.Model.Products;

namespace FurnitureEPR.Application.Features.Products;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);
}
