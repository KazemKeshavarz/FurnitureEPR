using FurnitureEPR.Application.Features.Customers;
using FurnitureEPR.Model.Customers;
using FurnitureEPR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Guid id, string name, string? phoneNumber, string? address, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");

        customer.SetName(name);
        customer.UpdateContact(phoneNumber, address);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
