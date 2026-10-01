using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PagedResult<CustomerListItemDto>>;
