using FurnitureEPR.Application.Features.Categories;
using FurnitureEPR.Model.Categories;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        await _db.Categories.AddAsync(category, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
