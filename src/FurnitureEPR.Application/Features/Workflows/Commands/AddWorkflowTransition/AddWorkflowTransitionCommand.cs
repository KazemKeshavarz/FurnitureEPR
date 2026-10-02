using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowTransition;

public sealed record AddWorkflowTransitionCommand(
    Guid WorkflowVersionId,
    Guid FromStageId,
    Guid ToStageId,
    string Name) : IRequest<Guid>;
