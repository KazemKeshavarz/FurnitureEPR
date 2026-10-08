using FurnitureEPR.Application.Features.Orders;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedOrderDto>
{
    private readonly IOrderReadRepository _repository;

    public GetOrdersQueryHandler(IOrderReadRepository repository)
        => _repository = repository;

    public Task<PagedOrderDto> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
        => _repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Status,
            cancellationToken);
}