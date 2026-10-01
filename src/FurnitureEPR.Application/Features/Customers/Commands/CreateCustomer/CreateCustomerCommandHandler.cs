using FurnitureEPR.Model.Customers;
using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Guid> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer(
            request.Name,
            request.PhoneNumber,
            request.Address);

        await _customerRepository.AddAsync(customer, cancellationToken);

        return customer.Id;
    }
}
