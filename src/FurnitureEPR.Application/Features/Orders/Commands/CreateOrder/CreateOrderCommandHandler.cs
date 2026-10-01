using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Model.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.CreateAsync(
            request.CustomerId,
            request.CreatedByUserId,
            request.Items,
            cancellationToken);

        return order.Id;
    }
}
