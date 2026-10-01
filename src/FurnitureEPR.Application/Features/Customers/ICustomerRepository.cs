using FurnitureEPR.Model.Customers;

namespace FurnitureEPR.Application.Features.Customers;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
}
