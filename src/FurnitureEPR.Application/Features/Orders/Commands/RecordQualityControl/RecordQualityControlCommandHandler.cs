using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Model.Workflow;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.RecordQualityControl;

public sealed class RecordQualityControlCommandHandler
    : IRequestHandler<RecordQualityControlCommand>
{
    private readonly IOrderWorkflowRuntimeRepository _repository;

    public RecordQualityControlCommandHandler(
        IOrderWorkflowRuntimeRepository repository)
        => _repository = repository;

    public Task Handle(
        RecordQualityControlCommand request,
        CancellationToken cancellationToken)
        => _repository.RecordQualityControlAsync(
            request.OrderId,
            request.CategoryId,
            request.Result,
            request.Comment,
            null,
            cancellationToken);
}
