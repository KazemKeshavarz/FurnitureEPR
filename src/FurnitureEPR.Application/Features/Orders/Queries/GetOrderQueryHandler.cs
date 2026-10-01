using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderReadRepository _repository;

    public GetOrderQueryHandler(IOrderReadRepository repository)
        => _repository = repository;

    public Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}
