using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;

public sealed class UpdateDraftOrderCommandHandler : IRequestHandler<UpdateDraftOrderCommand>
{
    private readonly IOrderDraftRepository _repository;

    public UpdateDraftOrderCommandHandler(IOrderDraftRepository repository)
        => _repository = repository;

    public Task Handle(UpdateDraftOrderCommand request, CancellationToken cancellationToken)
        => _repository.UpdateAsync(
            request.OrderId,
            request.CustomerId,
            request.DiscountAmount,
            request.Items,
            cancellationToken);
}
