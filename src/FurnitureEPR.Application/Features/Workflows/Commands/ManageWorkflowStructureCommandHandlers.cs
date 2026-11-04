using FurnitureEPR.Application.Features.Workflows;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands;

public sealed record UpdateWorkflowStageCommand(Guid WorkflowVersionId, Guid StageId, string Name, string Code, int SortOrder, bool RequiresQualityControl, Guid? ResponsibleRoleId) : IRequest;
public sealed record DeleteWorkflowStageCommand(Guid WorkflowVersionId, Guid StageId) : IRequest;
public sealed record UpdateWorkflowTransitionCommand(Guid WorkflowVersionId, Guid TransitionId, Guid FromStageId, Guid ToStageId, string Name) : IRequest;
public sealed record DeleteWorkflowTransitionCommand(Guid WorkflowVersionId, Guid TransitionId) : IRequest;

public sealed class UpdateWorkflowStageCommandHandler(IWorkflowRepository repository) : IRequestHandler<UpdateWorkflowStageCommand>
{
    public Task Handle(UpdateWorkflowStageCommand request, CancellationToken cancellationToken) =>
        repository.UpdateStageAsync(request.WorkflowVersionId, request.StageId, request.Name, request.Code, request.SortOrder, request.RequiresQualityControl, request.ResponsibleRoleId, cancellationToken);
}

public sealed class DeleteWorkflowStageCommandHandler(IWorkflowRepository repository) : IRequestHandler<DeleteWorkflowStageCommand>
{
    public Task Handle(DeleteWorkflowStageCommand request, CancellationToken cancellationToken) =>
        repository.DeleteStageAsync(request.WorkflowVersionId, request.StageId, cancellationToken);
}

public sealed class UpdateWorkflowTransitionCommandHandler(IWorkflowRepository repository) : IRequestHandler<UpdateWorkflowTransitionCommand>
{
    public Task Handle(UpdateWorkflowTransitionCommand request, CancellationToken cancellationToken) =>
        repository.UpdateTransitionAsync(request.WorkflowVersionId, request.TransitionId, request.FromStageId, request.ToStageId, request.Name, cancellationToken);
}

public sealed class DeleteWorkflowTransitionCommandHandler(IWorkflowRepository repository) : IRequestHandler<DeleteWorkflowTransitionCommand>
{
    public Task Handle(DeleteWorkflowTransitionCommand request, CancellationToken cancellationToken) =>
        repository.DeleteTransitionAsync(request.WorkflowVersionId, request.TransitionId, cancellationToken);
}
