using FurnitureEPR.Model.Products;
namespace FurnitureEPR.Application.Features.Products;
public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, Guid categoryId, string name, CancellationToken cancellationToken);
}
