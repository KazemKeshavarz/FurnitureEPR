using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.FinalizeOrder;

public sealed class FinalizeOrderCommandHandler : IRequestHandler<FinalizeOrderCommand>
{
    private readonly IOrderFinalizationRepository _repository;

    public FinalizeOrderCommandHandler(IOrderFinalizationRepository repository)
        => _repository = repository;

    public Task Handle(FinalizeOrderCommand request, CancellationToken cancellationToken)
        => _repository.FinalizeAsync(request.OrderId, cancellationToken);
}
