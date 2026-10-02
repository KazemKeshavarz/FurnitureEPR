using FurnitureEPR.Application.Features.Workflows;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.PublishWorkflowVersion;

public sealed class PublishWorkflowVersionCommandHandler
    : IRequestHandler<PublishWorkflowVersionCommand>
{
    private readonly IWorkflowRepository _repository;

    public PublishWorkflowVersionCommandHandler(IWorkflowRepository repository)
        => _repository = repository;

    public Task Handle(
        PublishWorkflowVersionCommand request,
        CancellationToken cancellationToken)
        => _repository.PublishVersionAsync(
            request.WorkflowVersionId,
            cancellationToken);
}
