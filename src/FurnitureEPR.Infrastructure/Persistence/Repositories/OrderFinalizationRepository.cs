using FurnitureEPR.Application.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderFinalizationRepository : IOrderFinalizationRepository
{
    private readonly ApplicationDbContext _db;

    public OrderFinalizationRepository(ApplicationDbContext db) => _db = db;

    public async Task FinalizeAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        order.SetOrderNumber($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}");
        order.FinalizeOrder();

        await _db.SaveChangesAsync(cancellationToken);
    }
}
