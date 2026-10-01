using FurnitureEPR.Application.Features.Customers;
using FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;
using FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class CustomerReadRepository : ICustomerReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
        => await _dbContext.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CustomerDto(
                x.Id,
                x.Name,
                x.PhoneNumber,
                x.Address))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<CustomerListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(value) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(value)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerListItemDto(
                x.Id,
                x.Name,
                x.PhoneNumber))
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerListItemDto>(
            items,
            page,
            pageSize,
            totalCount);
    }
}
