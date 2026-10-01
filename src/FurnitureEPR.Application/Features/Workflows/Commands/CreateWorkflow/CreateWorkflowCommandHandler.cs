using FurnitureEPR.Model.Workflow;
using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflow;

public sealed class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Guid>
{
    private readonly IWorkflowRepository _repository;

    public CreateWorkflowCommandHandler(IWorkflowRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = new WorkflowDefinition(request.Name, request.Code);
        await _repository.AddAsync(workflow, cancellationToken);
        return workflow.Id;
    }
}
