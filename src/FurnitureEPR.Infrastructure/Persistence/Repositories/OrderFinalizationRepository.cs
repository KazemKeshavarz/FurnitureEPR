using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Model.Workflow;
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
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.Category)
                        .ThenInclude(x => x.WorkflowVersion)
                            .ThenInclude(x => x!.Stages)
            .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        // هر Category گردشکار مستقل خودش را دارد؛ بنابراین برای هر Category یک Runtime Instance می‌سازیم.
        var categories = order.Items
            .Select(x => x.Product.Category)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

        foreach (var category in categories)
        {
            var workflowVersion = category.WorkflowVersion;

            if (workflowVersion is null || !workflowVersion.IsPublished)
                throw new InvalidOperationException(
                    $"Category '{category.Name}' does not have a published workflow.");

            var initialStage = workflowVersion.Stages
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefault();

            if (initialStage is null)
                throw new InvalidOperationException(
                    $"Workflow version '{workflowVersion.VersionNumber}' has no active stage.");

            _db.OrderWorkflowInstances.Add(
                new OrderWorkflowInstance(
                    order.Id,
                    category.Id,
                    workflowVersion.Id,
                    initialStage.Id));
        }

        order.SetOrderNumber($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}");
        order.FinalizeOrder();

        await _db.SaveChangesAsync(cancellationToken);
    }
}
