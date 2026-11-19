using FurnitureEPR.Model.Categories;
namespace FurnitureEPR.Application.Features.Categories;
public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, string name, CancellationToken cancellationToken);
    Task AssignWorkflowAsync(Guid categoryId, Guid workflowVersionId, CancellationToken cancellationToken);
}
