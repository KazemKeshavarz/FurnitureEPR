using FurnitureEPR.Model.Workflow;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;

public sealed class AddWorkflowStageCommandHandler : IRequestHandler<AddWorkflowStageCommand, Guid>
{
    private readonly ApplicationDbContext _db;

    public AddWorkflowStageCommandHandler(ApplicationDbContext db) => _db = db;

    public async Task<Guid> Handle(AddWorkflowStageCommand request, CancellationToken cancellationToken)
    {
        var version = await _db.WorkflowVersions.FindAsync(
            new object[] { request.WorkflowVersionId },
            cancellationToken);

        if (version is null)
            throw new KeyNotFoundException("Workflow version was not found.");

        var stage = new WorkflowStage(
            request.WorkflowVersionId,
            request.Name,
            request.Code,
            request.SortOrder,
            request.RequiresQualityControl);

        version.AddStage(stage);
        await _db.SaveChangesAsync(cancellationToken);
        return stage.Id;
    }
}
