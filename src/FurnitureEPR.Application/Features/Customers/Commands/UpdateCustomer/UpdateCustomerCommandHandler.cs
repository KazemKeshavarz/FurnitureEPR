using FurnitureEPR.Application.Features.Customers;
using MediatR;

namespace FurnitureEPR.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _repository;

    public UpdateCustomerCommandHandler(ICustomerRepository repository) => _repository = repository;

    public Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        => _repository.UpdateAsync(request.Id, request.Name, request.PhoneNumber, request.Address, cancellationToken);
}
