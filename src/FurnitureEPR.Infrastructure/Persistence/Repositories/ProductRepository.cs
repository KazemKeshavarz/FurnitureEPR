using FurnitureEPR.Application.Features.Products;
using FurnitureEPR.Model.Products;
using Microsoft.EntityFrameworkCore;
namespace FurnitureEPR.Infrastructure.Persistence.Repositories;
public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;
    public ProductRepository(ApplicationDbContext db) => _db = db;
    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _db.Products.AddAsync(product, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateAsync(Guid id, Guid categoryId, string name, CancellationToken cancellationToken)
    {
        var product = await _db.Products.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Product was not found.");
        var categoryExists = await _db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);
        if (!categoryExists) throw new KeyNotFoundException("Category was not found.");
        product.SetName(name);
        // انتقال محصول به دسته‌بندی دیگر عمداً پشتیبانی نمی‌شود؛ مالکیت دسته‌بندی ثابت می‌ماند.
        if (product.CategoryId != categoryId)
            throw new InvalidOperationException("A product cannot be moved to another category after creation.");
        await _db.SaveChangesAsync(cancellationToken);
    }
}
