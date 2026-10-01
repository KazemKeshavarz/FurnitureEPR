using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;

public sealed record GetCustomerQuery(Guid Id) : IRequest<CustomerDto?>;
