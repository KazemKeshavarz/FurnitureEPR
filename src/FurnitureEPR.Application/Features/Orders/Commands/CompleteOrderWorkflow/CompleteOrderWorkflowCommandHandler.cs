using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;

public sealed class CompleteOrderWorkflowCommandHandler
    : IRequestHandler<CompleteOrderWorkflowCommand>
{
    private readonly IOrderWorkflowRuntimeRepository _repository;

    public CompleteOrderWorkflowCommandHandler(
        IOrderWorkflowRuntimeRepository repository)
        => _repository = repository;

    public Task Handle(
        CompleteOrderWorkflowCommand request,
        CancellationToken cancellationToken)
        => _repository.CompleteAsync(
            request.OrderId,
            request.CategoryId,
            cancellationToken);
}
