using FurnitureEPR.Application.Features.Customers;
using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    private readonly ICustomerReadRepository _repository;

    public GetCustomersQueryHandler(ICustomerReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<CustomerListItemDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
        => _repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);
}
