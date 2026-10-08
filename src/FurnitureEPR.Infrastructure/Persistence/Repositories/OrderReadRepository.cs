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
                x.TotalAmount,
                x.DiscountAmount,
                x.FinalAmount,
                x.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice,
                    i.Components.Select(c => new OrderItemComponentDto(
                        c.Id,
                        c.ComponentId,
                        c.ComponentName,
                        c.Quantity)).ToList()))
                    .ToList(),
                x.Workflows.Select(w => new OrderWorkflowInstanceDto(
                    w.Id,
                    w.CategoryId,
                    w.Category.Name,
                    w.WorkflowVersionId,
                    w.WorkflowVersion.VersionNumber,
                    w.CurrentStageId,
                    w.CurrentStage.Name,
                    w.Status.ToString(),
                    w.StartedAtUtc,
                    w.CompletedAtUtc,
                    w.History
                        .OrderBy(h => h.OccurredAtUtc)
                        .Select(h => new OrderWorkflowHistoryDto(
                            h.Id,
                            h.FromStageId,
                            h.ToStageId,
                            h.TransitionId,
                            h.OccurredAtUtc))
                        .ToList(),
                    w.QualityChecks
                        .OrderBy(q => q.CheckedAtUtc)
                        .Select(q => new OrderWorkflowQualityCheckDto(
                            q.Id,
                            q.StageId,
                            q.Result.ToString(),
                            q.Comment,
                            q.CheckedAtUtc,
                            q.CheckedByUserId))
                        .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
}