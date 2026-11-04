using FurnitureEPR.Application.Features.Workflows;
using FurnitureEPR.Application.Features.Workflows.Queries;
using FurnitureEPR.Model.Categories;
using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class WorkflowRepository : IWorkflowRepository
{
    private readonly ApplicationDbContext _db;

    public WorkflowRepository(ApplicationDbContext db) => _db = db;


    public async Task<WorkflowPagedResult<WorkflowListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.WorkflowDefinitions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var definitions = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                VersionCount = x.Versions.Count,
                PublishedVersionCount = x.Versions.Count(v => v.IsPublished)
            })
            .ToListAsync(cancellationToken);

        var ids = definitions.Select(x => x.Id).ToArray();
        var assignedCategories = await _db.Categories
            .AsNoTracking()
            .Where(x => x.WorkflowVersion != null && ids.Contains(x.WorkflowVersion.WorkflowDefinitionId))
            .Select(x => new { x.WorkflowVersion!.WorkflowDefinitionId, x.Name })
            .ToListAsync(cancellationToken);

        var items = definitions.Select(x => new WorkflowListItemDto(
            x.Id,
            x.Name,
            x.Code,
            x.VersionCount,
            x.PublishedVersionCount,
            assignedCategories
                .Where(c => c.WorkflowDefinitionId == x.Id)
                .Select(c => c.Name)
                .OrderBy(name => name)
                .ToArray()))
            .ToArray();

        return new WorkflowPagedResult<WorkflowListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<WorkflowDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var definition = await _db.WorkflowDefinitions
            .AsNoTracking()
            .Include(x => x.Versions)
                .ThenInclude(x => x.Stages)
            .Include(x => x.Versions)
                .ThenInclude(x => x.Transitions)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return null;

        var assignedCategoryIds = await _db.Categories
            .AsNoTracking()
            .Where(x => x.WorkflowVersion != null && x.WorkflowVersion.WorkflowDefinitionId == id)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        var versions = definition.Versions
            .OrderByDescending(x => x.VersionNumber)
            .Select(version => new WorkflowVersionDto(
                version.Id,
                version.VersionNumber,
                version.IsPublished,
                version.Stages
                    .OrderBy(stage => stage.SortOrder)
                    .Select(stage => new WorkflowStageDto(
                        stage.Id,
                        stage.Name,
                        stage.Code,
                        stage.SortOrder,
                        stage.RequiresQualityControl,
                        stage.ResponsibleRoleId,
                        stage.IsActive))
                    .ToArray(),
                version.Transitions
                    .OrderBy(transition => transition.Name)
                    .Select(transition => new WorkflowTransitionDto(
                        transition.Id,
                        transition.FromStageId,
                        transition.ToStageId,
                        transition.Name))
                    .ToArray()))
            .ToArray();

        return new WorkflowDetailDto(
            definition.Id,
            definition.Name,
            definition.Code,
            assignedCategoryIds,
            versions);
    }

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
