namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
