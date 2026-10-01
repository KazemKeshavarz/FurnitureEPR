using FurnitureEPR.Model.Categories;

namespace FurnitureEPR.Application.Features.Categories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken);
}
