using FurnitureEPR.Application.Features.Workflows;
using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Guid> AddTransitionAsync(
        Guid workflowVersionId,
        Guid fromStageId,
        Guid toStageId,
        string name,
        CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions.FindAsync(
            new object[] { workflowVersionId },
            cancellationToken);

        if (version is null)
            throw new KeyNotFoundException("Workflow version was not found.");

        // هر دو مرحله باید متعلق به همان نسخه گردشکار باشند.
        var stages = await _db.WorkflowStages
            .Where(x => x.WorkflowVersionId == workflowVersionId)
            .Where(x => x.Id == fromStageId || x.Id == toStageId)
            .ToListAsync(cancellationToken);

        if (stages.Count != 2)
            throw new InvalidOperationException(
                "Both transition stages must belong to the workflow version.");

        var transition = new WorkflowTransition(
            workflowVersionId,
            fromStageId,
            toStageId,
            name);

        version.AddTransition(transition);
        await _db.SaveChangesAsync(cancellationToken);

        return transition.Id;
    }

    public async Task AddVersionAsync(
        WorkflowVersion version,
        CancellationToken cancellationToken)
    {
        var exists = await _db.WorkflowDefinitions.FindAsync(
            new object[] { version.WorkflowDefinitionId },
            cancellationToken);

        if (exists is null)
            throw new KeyNotFoundException("Workflow was not found.");

        await _db.WorkflowVersions.AddAsync(version, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishVersionAsync(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions
            .Include(x => x.Stages)
            .Include(x => x.Transitions)
            .SingleOrDefaultAsync(x => x.Id == workflowVersionId, cancellationToken);

        if (version is null)
            throw new KeyNotFoundException("Workflow version was not found.");

        version.Publish();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
