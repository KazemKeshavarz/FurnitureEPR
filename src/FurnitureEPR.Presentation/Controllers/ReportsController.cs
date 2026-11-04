using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Model.Orders;
using FurnitureEPR.Model.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Presentation.Controllers;

// گزارش‌ها فقط داده‌های عملیاتی سفارش و تولید را نمایش می‌دهند؛ این API جایگزین حسابداری یا انبارداری نیست.
[ApiController]
[Route("api/reports")]
[Authorize(Policy = PermissionNames.WorkflowManage)]
public sealed class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(from, to);
        var orders = _db.Orders.AsNoTracking().Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc < end);

        var statusCounts = await orders
            .GroupBy(x => x.Status)
            .Select(g => new { status = g.Key.ToString(), count = g.Count() })
            .ToListAsync(cancellationToken);

        var totals = await orders
            .GroupBy(_ => 1)
            .Select(g => new
            {
                count = g.Count(),
                totalAmount = g.Sum(x => x.TotalAmount),
                discountAmount = g.Sum(x => x.DiscountAmount),
                finalAmount = g.Sum(x => x.FinalAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var activeStages = await _db.OrderWorkflowInstances.AsNoTracking()
            .Where(x => x.Status == OrderWorkflowInstanceStatus.Active)
            .GroupBy(x => x.CurrentStage.Name)
            .Select(g => new { stageName = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToListAsync(cancellationToken);

        var recentOrders = await orders
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .Select(x => new
            {
                x.Id,
                x.OrderNumber,
                customerName = x.Customer.Name,
                status = x.Status.ToString(),
                x.CreatedAtUtc,
                x.FinalAmount
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            from = start,
            to = end,
            totals = totals ?? new { count = 0, totalAmount = 0m, discountAmount = 0m, finalAmount = 0m },
            statuses = statusCounts,
            activeStages,
            recentOrders
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrdersReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(from, to);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Orders.AsNoTracking()
            .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc < end);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.OrderNumber,
                customerName = x.Customer.Name,
                status = x.Status.ToString(),
                x.CreatedAtUtc,
                x.FinalizedAtUtc,
                x.TotalAmount,
                x.DiscountAmount,
                x.FinalAmount,
                itemsCount = x.Items.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(new { from = start, to = end, page, pageSize, totalCount, items });
    }

    [HttpGet("production")]
    public async Task<IActionResult> GetProductionReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(from, to);
        var current = await _db.OrderWorkflowInstances.AsNoTracking()
            .Where(x => x.Status == OrderWorkflowInstanceStatus.Active && x.StartedAtUtc < end)
            .GroupBy(x => new { x.CurrentStageId, x.CurrentStage.Name, x.CurrentStage.Code, x.Status })
            .Select(g => new
            {
                stageId = g.Key.CurrentStageId,
                stageName = g.Key.Name,
                stageCode = g.Key.Code,
                workflowStatus = g.Key.Status.ToString(),
                activeCount = g.Count()
            })
            .OrderByDescending(x => x.activeCount)
            .ToListAsync(cancellationToken);

        var completedByStage = await _db.OrderWorkflowHistories.AsNoTracking()
            .Where(x => x.OccurredAtUtc >= start && x.OccurredAtUtc < end)
            .GroupBy(x => new { x.ToStageId, x.ToStage.Name, x.ToStage.Code })
            .Select(g => new
            {
                stageId = g.Key.ToStageId,
                stageName = g.Key.Name,
                stageCode = g.Key.Code,
                entries = g.Count(),
                lastActivityAtUtc = g.Max(x => x.OccurredAtUtc)
            })
            .OrderByDescending(x => x.entries)
            .ToListAsync(cancellationToken);

        var quality = await _db.OrderWorkflowQualityChecks.AsNoTracking()
            .Where(x => x.CheckedAtUtc >= start && x.CheckedAtUtc < end)
            .GroupBy(x => x.Result)
            .Select(g => new { result = g.Key.ToString(), count = g.Count() })
            .ToListAsync(cancellationToken);

        return Ok(new { from = start, to = end, current, completedByStage, quality });
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProductsReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(from, to);
        var products = await _db.OrderItems.AsNoTracking()
            .Where(x => x.Order.CreatedAtUtc >= start && x.Order.CreatedAtUtc < end)
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                productName = g.Key.ProductName,
                orderLines = g.Count(),
                orderCount = g.Select(x => x.OrderId).Distinct().Count(),
                quantity = g.Sum(x => x.Quantity),
                amount = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.quantity)
            .ToListAsync(cancellationToken);

        return Ok(new { from = start, to = end, products });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsersActivityReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(from, to);
        var activity = await (
            from log in _db.AuditLogs.AsNoTracking()
            join user in _db.Users.AsNoTracking() on log.UserId equals user.Id into userGroup
            from user in userGroup.DefaultIfEmpty()
            where log.OccurredAtUtc >= start && log.OccurredAtUtc < end
            group new { log, user } by new
            {
                log.UserId,
                UserName = user == null ? "کاربر حذف‌شده/ناشناس" : user.UserName
            } into g
            select new
            {
                userId = g.Key.UserId,
                userName = g.Key.UserName,
                requestCount = g.Count(),
                successfulRequests = g.Count(x => x.log.StatusCode >= 200 && x.log.StatusCode < 400),
                failedRequests = g.Count(x => x.log.StatusCode >= 400)
            })
            .OrderByDescending(x => x.requestCount)
            .ToListAsync(cancellationToken);

        return Ok(new { from = start, to = end, activity });
    }

    private static (DateTime Start, DateTime End) NormalizeRange(DateTime? from, DateTime? to)
    {
        var end = (to?.Date.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1));
        var start = from?.Date ?? end.AddDays(-30);
        if (start >= end)
            throw new ArgumentException("تاریخ شروع باید قبل از تاریخ پایان باشد.");
        return (DateTime.SpecifyKind(start, DateTimeKind.Utc), DateTime.SpecifyKind(end, DateTimeKind.Utc));
    }
}
