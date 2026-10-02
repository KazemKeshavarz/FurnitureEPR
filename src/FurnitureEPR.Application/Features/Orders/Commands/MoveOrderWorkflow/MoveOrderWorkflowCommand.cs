using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;

public sealed record MoveOrderWorkflowCommand(
    Guid OrderId,
    Guid CategoryId,
    Guid TransitionId) : IRequest;
