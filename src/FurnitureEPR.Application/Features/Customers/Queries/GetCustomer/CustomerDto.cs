namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? PhoneNumber,
    string? Address);
