using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string? PhoneNumber,
    string? Address) : IRequest<Guid>;
