using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;

public sealed record CompleteOrderWorkflowCommand(
    Guid OrderId,
    Guid CategoryId) : IRequest;
