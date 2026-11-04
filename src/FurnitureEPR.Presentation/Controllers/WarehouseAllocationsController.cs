using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Model.Orders;
using FurnitureEPR.Model.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/orders/items/{orderItemId:guid}/warehouse-allocations")]
[Authorize(Policy = PermissionNames.WorkflowMove)]
public sealed class WarehouseAllocationsController(ApplicationDbContext db) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Replace(
        Guid orderItemId,
        ReplaceWarehouseAllocationsRequest request,
        CancellationToken cancellationToken)
    {
        var item = await db.OrderItems
            .Include(x => x.Order)
            .Include(x => x.Product)
            .Include(x => x.WarehouseAllocations)
            .SingleOrDefaultAsync(x => x.Id == orderItemId, cancellationToken);

        if (item is null) return NotFound();
        if (item.Order.Status is OrderStatus.Draft or OrderStatus.Cancelled)
            return Conflict("انبار محصول فقط پس از نهایی‌شدن سفارش قابل ثبت است.");

        // تخصیص فقط وقتی مجاز است که گردش‌کار دسته‌بندی به مرحله انبار رسیده باشد.
        var isAtWarehouseStage = await db.OrderWorkflowInstances
            .AnyAsync(x => x.OrderId == item.OrderId
                && x.CategoryId == item.Product.CategoryId
                && x.Status == FurnitureEPR.Model.Workflow.OrderWorkflowInstanceStatus.Active
                && x.CurrentStage.Code == "WAREHOUSE", cancellationToken);
        if (!isAtWarehouseStage)
            return Conflict("ابتدا محصول را در گردش‌کار به مرحله انبار با کد WAREHOUSE برسانید.");

        var allocationsToSave = request.Allocations ?? Array.Empty<WarehouseAllocationInput>();
        if (allocationsToSave.Any(x => x.Quantity <= 0))
            return BadRequest("تعداد هر تخصیص باید بیشتر از صفر باشد.");
        if (allocationsToSave.GroupBy(x => x.WarehouseId).Any(x => x.Count() > 1))
            return BadRequest("هر انبار را فقط یک بار انتخاب کنید.");

        var total = allocationsToSave.Sum(x => x.Quantity);
        if (total > item.Quantity)
            return BadRequest($"مجموع تعداد تخصیص‌یافته ({total}) نمی‌تواند از تعداد سفارش ({item.Quantity}) بیشتر باشد.");

        var warehouseIds = allocationsToSave.Select(x => x.WarehouseId).ToArray();
        var activeWarehouseIds = await db.Warehouses
            .Where(x => warehouseIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (activeWarehouseIds.Count != warehouseIds.Distinct().Count())
            return BadRequest("یکی از انبارهای انتخاب‌شده وجود ندارد یا غیرفعال است.");

        db.OrderItemWarehouseAllocations.RemoveRange(item.WarehouseAllocations);
        foreach (var allocation in allocationsToSave)
            db.OrderItemWarehouseAllocations.Add(new OrderItemWarehouseAllocation(orderItemId, allocation.WarehouseId, allocation.Quantity));

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record WarehouseAllocationInput(Guid WarehouseId, decimal Quantity);
public sealed record ReplaceWarehouseAllocationsRequest(IReadOnlyCollection<WarehouseAllocationInput> Allocations);
