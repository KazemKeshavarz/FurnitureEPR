using FurnitureEPR.Model.Categories;

namespace FurnitureEPR.Application.Features.Categories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken);

    // فقط نسخه منتشرشده گردشکار می‌تواند به یک دسته‌بندی اختصاص داده شود.
    Task AssignWorkflowAsync(
        Guid categoryId,
        Guid workflowVersionId,
        CancellationToken cancellationToken);
}
