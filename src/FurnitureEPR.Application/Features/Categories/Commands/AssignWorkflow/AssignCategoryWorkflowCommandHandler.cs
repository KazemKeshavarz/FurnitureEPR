using FurnitureEPR.Application.Features.Categories;
using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Commands.AssignWorkflow;

public sealed class AssignCategoryWorkflowCommandHandler
    : IRequestHandler<AssignCategoryWorkflowCommand>
{
    private readonly ICategoryRepository _repository;

    public AssignCategoryWorkflowCommandHandler(ICategoryRepository repository)
        => _repository = repository;

    public Task Handle(
        AssignCategoryWorkflowCommand request,
        CancellationToken cancellationToken)
        => _repository.AssignWorkflowAsync(
            request.CategoryId,
            request.WorkflowVersionId,
            cancellationToken);
}
