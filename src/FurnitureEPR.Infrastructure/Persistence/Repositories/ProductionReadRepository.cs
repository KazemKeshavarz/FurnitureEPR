using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Features.Orders.Queries;
using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class ProductionReadRepository : IProductionReadRepository
{
    private readonly ApplicationDbContext _db;

    public ProductionReadRepository(ApplicationDbContext db) => _db = db;

    public async Task<PagedProductionTaskDto> GetTasksAsync(
        int page,
        int pageSize,
        string? search,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.OrderWorkflowInstances
            .AsNoTracking()
            .Where(x => x.Status == OrderWorkflowInstanceStatus.Active)
            .Where(x => x.Order.Status == FurnitureEPR.Model.Orders.OrderStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Order.OrderNumber.Contains(term) ||
                x.Order.Customer.Name.Contains(term) ||
                x.Category.Name.Contains(term) ||
                x.CurrentStage.Name.Contains(term));
        }

        // فقط Stageهایی نمایش داده می‌شوند که مسئول مشخصی ندارند یا Role کاربر فعلی مسئول آن‌هاست.
        query = query.Where(x =>
            x.CurrentStage.ResponsibleRoleId == null ||
            roleIds.Contains(x.CurrentStage.ResponsibleRoleId.Value));

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(x => x.CurrentStage.SortOrder)
            .ThenBy(x => x.StartedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.OrderId,
                x.Order.OrderNumber,
                CustomerName = x.Order.Customer.Name,
                x.CategoryId,
                CategoryName = x.Category.Name,
                x.CurrentStageId,
                CurrentStageName = x.CurrentStage.Name,
                CurrentStageCode = x.CurrentStage.Code,
                x.CurrentStage.RequiresQualityControl,
                x.StartedAtUtc,
                ResponsibleRoleId = x.CurrentStage.ResponsibleRoleId,
                QualityChecks = x.QualityChecks
                    .Where(q => q.StageId == x.CurrentStageId)
                    .OrderByDescending(q => q.CheckedAtUtc)
                    .Select(q => (QualityControlResult?)q.Result)
                    .FirstOrDefault(),
                Transitions = x.WorkflowVersion.Transitions
                    .Where(t => t.FromStageId == x.CurrentStageId && t.ToStage.IsActive)
                    .OrderBy(t => t.ToStage.SortOrder)
                    .Select(t => new ProductionTaskTransitionDto(
                        t.Id,
                        t.Name,
                        t.ToStageId,
                        t.ToStage.Name))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x =>
        {
            var qualityApproved =
                !x.RequiresQualityControl ||
                x.QualityChecks == QualityControlResult.Approved;

            var canMove =
                qualityApproved &&
                (x.ResponsibleRoleId is null || roleIds.Contains(x.ResponsibleRoleId.Value));

            var canComplete =
                canMove &&
                x.Transitions.Count == 0;

            return new ProductionTaskDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.CustomerName,
                x.CategoryId,
                x.CategoryName,
                x.CurrentStageId,
                x.CurrentStageName,
                x.CurrentStageCode,
                x.RequiresQualityControl,
                x.QualityChecks == QualityControlResult.Approved,
                canMove,
                canComplete,
                x.Transitions,
                x.StartedAtUtc);
        }).ToList();

        return new PagedProductionTaskDto(items, page, pageSize, totalCount);
    }
}