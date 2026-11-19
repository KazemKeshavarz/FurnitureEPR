using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? PhoneNumber,
    string? Address) : IRequest;
