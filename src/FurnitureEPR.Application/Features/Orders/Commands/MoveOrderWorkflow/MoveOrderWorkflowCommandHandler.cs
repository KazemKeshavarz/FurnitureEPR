using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;

public sealed class MoveOrderWorkflowCommandHandler
    : IRequestHandler<MoveOrderWorkflowCommand>
{
    private readonly IOrderWorkflowRuntimeRepository _repository;

    public MoveOrderWorkflowCommandHandler(
        IOrderWorkflowRuntimeRepository repository)
    {
        _repository = repository;
    }

    public Task Handle(
        MoveOrderWorkflowCommand request,
        CancellationToken cancellationToken)
        => _repository.MoveAsync(
            request.OrderId,
            request.CategoryId,
            request.TransitionId,
            cancellationToken);
}
