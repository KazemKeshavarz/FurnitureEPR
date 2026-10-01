using FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;
using FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;

namespace FurnitureEPR.Application.Features.Customers;

public interface ICustomerReadRepository
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<CustomerListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);
}
