using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;

public sealed record MoveOrderWorkflowCommand(
    Guid WorkflowInstanceId,
    Guid TransitionId) : IRequest;
