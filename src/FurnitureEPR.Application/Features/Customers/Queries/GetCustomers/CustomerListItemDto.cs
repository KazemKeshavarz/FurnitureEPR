namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;

public sealed record CustomerListItemDto(
    Guid Id,
    string Name,
    string? PhoneNumber);
