using FurnitureEPR.Application.Features.Workflows;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;

public sealed class AddWorkflowStageCommandHandler : IRequestHandler<AddWorkflowStageCommand, Guid>
{
    private readonly IWorkflowRepository _repository;

    public AddWorkflowStageCommandHandler(IWorkflowRepository repository) => _repository = repository;

    public Task<Guid> Handle(AddWorkflowStageCommand request, CancellationToken cancellationToken)
        => _repository.AddStageAsync(
            request.WorkflowVersionId,
            request.Name,
            request.Code,
            request.SortOrder,
            request.RequiresQualityControl,
            cancellationToken);
}
