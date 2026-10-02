using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Commands.AssignWorkflow;

public sealed record AssignCategoryWorkflowCommand(
    Guid CategoryId,
    Guid WorkflowVersionId) : IRequest;
