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

    public async Task<Guid> AddStageAsync(
        Guid workflowVersionId,
        string name,
        string code,
        int sortOrder,
        bool requiresQualityControl,
        Guid? responsibleRoleId,
        CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions.FindAsync(
            new object[] { workflowVersionId },
            cancellationToken);

        if (version is null)
            throw new KeyNotFoundException("Workflow version was not found.");

        // نقش در Infrastructure به Identity متصل است؛ Domain فقط شناسه آن را می‌شناسد.
        if (responsibleRoleId.HasValue)
        {
            var roleExists = await _db.Roles.AnyAsync(
                x => x.Id == responsibleRoleId.Value,
                cancellationToken);

            if (!roleExists)
                throw new KeyNotFoundException("Responsible role was not found.");
        }

        var stage = new WorkflowStage(
            workflowVersionId,
            name,
            code,
            sortOrder,
            requiresQualityControl,
            responsibleRoleId);

        version.AddStage(stage);
        await _db.SaveChangesAsync(cancellationToken);
        return stage.Id;
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
