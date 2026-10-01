using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Application.Features.Workflows;

public interface IWorkflowRepository
{
    Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken);
    Task AddVersionAsync(WorkflowVersion version, CancellationToken cancellationToken);
}
