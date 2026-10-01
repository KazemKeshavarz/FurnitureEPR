using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Features.Orders.Queries;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderReadRepository : IOrderReadRepository
{
    private readonly ApplicationDbContext _db;

    public OrderReadRepository(ApplicationDbContext db) => _db = db;

    public Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _db.Orders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new OrderDto(
                x.Id,
                x.OrderNumber,
                x.CustomerId,
                x.Customer.Name,
                x.Status.ToString(),
                x.CreatedAtUtc,
                x.FinalizedAtUtc,
                x.CreatedByUserId,
                x.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Components.Select(c => new OrderItemComponentDto(
                        c.Id,
                        c.ComponentId,
                        c.ComponentName,
                        c.Quantity)).ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
}
