using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Application.Features.Workflows;

public interface IWorkflowRepository
{
    Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken);
    Task AddVersionAsync(WorkflowVersion version, CancellationToken cancellationToken);

    // نقش مسئول مرحله از Identity می‌آید، ولی خود Domain فقط شناسه نقش را نگه می‌دارد.
    Task<Guid> AddStageAsync(
        Guid workflowVersionId,
        string name,
        string code,
        int sortOrder,
        bool requiresQualityControl,
        Guid? responsibleRoleId,
        CancellationToken cancellationToken);
}
