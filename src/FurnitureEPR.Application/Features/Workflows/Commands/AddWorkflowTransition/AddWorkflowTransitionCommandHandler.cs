using FurnitureEPR.Application.Features.Workflows;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowTransition;

public sealed class AddWorkflowTransitionCommandHandler
    : IRequestHandler<AddWorkflowTransitionCommand, Guid>
{
    private readonly IWorkflowRepository _repository;

    public AddWorkflowTransitionCommandHandler(IWorkflowRepository repository)
        => _repository = repository;

    public Task<Guid> Handle(
        AddWorkflowTransitionCommand request,
        CancellationToken cancellationToken)
        => _repository.AddTransitionAsync(
            request.WorkflowVersionId,
            request.FromStageId,
            request.ToStageId,
            request.Name,
            cancellationToken);
}
