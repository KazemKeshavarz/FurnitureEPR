using FurnitureEPR.Application.Features.Customers;
using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;

public sealed class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto?>
{
    private readonly ICustomerReadRepository _repository;

    public GetCustomerQueryHandler(ICustomerReadRepository repository)
    {
        _repository = repository;
    }

    public Task<CustomerDto?> Handle(
        GetCustomerQuery request,
        CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}
