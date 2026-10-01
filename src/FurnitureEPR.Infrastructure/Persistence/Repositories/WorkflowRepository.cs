using FurnitureEPR.Application.Features.Workflows;
using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class WorkflowRepository : IWorkflowRepository
{
    private readonly ApplicationDbContext _db;

    public WorkflowRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken)
    {
        await _db.WorkflowDefinitions.AddAsync(workflow, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddVersionAsync(WorkflowVersion version, CancellationToken cancellationToken)
    {
        var exists = await _db.WorkflowDefinitions.FindAsync(
            new object[] { version.WorkflowDefinitionId },
            cancellationToken);

        if (exists is null)
            throw new KeyNotFoundException("Workflow was not found.");

        await _db.WorkflowVersions.AddAsync(version, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
