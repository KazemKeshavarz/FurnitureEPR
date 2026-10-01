using FurnitureEPR.Application.Features.Workflows;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflowVersion;

public sealed class CreateWorkflowVersionCommandHandler : IRequestHandler<CreateWorkflowVersionCommand, Guid>
{
    private readonly IWorkflowRepository _repository;

    public CreateWorkflowVersionCommandHandler(IWorkflowRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var version = new Model.Workflow.WorkflowVersion(request.WorkflowId, request.VersionNumber);
        await _repository.AddVersionAsync(version, cancellationToken);
        return version.Id;
    }
}
