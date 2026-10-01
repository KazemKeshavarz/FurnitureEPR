using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Application.Features.Workflows;

public interface IWorkflowRepository
{
    Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken);
    Task AddVersionAsync(WorkflowVersion version, CancellationToken cancellationToken);
    Task<Guid> AddStageAsync(Guid workflowVersionId, string name, string code, int sortOrder, bool requiresQualityControl, CancellationToken cancellationToken);
}
