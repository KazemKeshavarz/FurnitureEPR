using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;

public sealed record CompleteOrderWorkflowCommand(
    Guid WorkflowInstanceId) : IRequest;
