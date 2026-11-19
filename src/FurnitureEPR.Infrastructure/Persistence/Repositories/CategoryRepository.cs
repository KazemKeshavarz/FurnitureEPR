using FurnitureEPR.Application.Features.Categories;
using FurnitureEPR.Model.Categories;
using Microsoft.EntityFrameworkCore;
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
    public async Task UpdateAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Category was not found.");
        category.SetName(name);
        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task AssignWorkflowAsync(Guid categoryId, Guid workflowVersionId, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.SingleOrDefaultAsync(x => x.Id == categoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Category was not found.");
        var workflowVersion = await _db.WorkflowVersions.SingleOrDefaultAsync(x => x.Id == workflowVersionId, cancellationToken)
            ?? throw new KeyNotFoundException("Workflow version was not found.");
        if (!workflowVersion.IsPublished)
            throw new InvalidOperationException("Only published workflow versions can be assigned to categories.");
        category.AssignWorkflow(workflowVersionId);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
