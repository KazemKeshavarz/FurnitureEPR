using System.Data;
using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Model.Orders;
using FurnitureEPR.Model.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Presentation.Controllers;

/// <summary>
/// ثبت درخواست انتقال کالای آماده و بررسی آن توسط مدیر.
/// </summary>
[ApiController]
[Route("api/order-transfers")]
public sealed class OrderTransfersController(ApplicationDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionNames.WorkflowManage)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var transfers = await db.OrderTransfers
            .AsNoTracking()
            .OrderByDescending(x => x.RequestedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.SourceOrderId,
                SourceOrderNumber = x.SourceOrder.OrderNumber,
                SourceCustomerName = x.SourceOrder.Customer.Name,
                x.TargetOrderId,
                TargetOrderNumber = x.TargetOrder.OrderNumber,
                TargetCustomerName = x.TargetOrder.Customer.Name,
                x.Reason,
                Status = x.Status.ToString(),
                x.RequestedByUserId,
                x.ReviewedByUserId,
                x.RequestedAtUtc,
                x.ReviewedAtUtc,
                Lines = x.Lines.Select(line => new
                {
                    line.Id,
                    line.SourceOrderItemId,
                    SourceProductName = line.SourceOrderItem.ProductName,
                    line.TargetOrderItemId,
                    TargetProductName = line.TargetOrderItem.ProductName,
                    line.WarehouseId,
                    WarehouseName = line.Warehouse.Name,
                    line.Quantity
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(transfers);
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.WorkflowMove)]
    public async Task<IActionResult> Create(
        CreateOrderTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Unauthorized();
        if (request.Lines is null || request.Lines.Count == 0)
            return BadRequest("حداقل یک قلم برای انتقال انتخاب کنید.");

        var sourceOrder = await db.Orders
            .SingleOrDefaultAsync(x => x.Id == request.SourceOrderId, cancellationToken);
        var targetOrder = await db.Orders
            .SingleOrDefaultAsync(x => x.Id == request.TargetOrderId, cancellationToken);

        if (sourceOrder is null || targetOrder is null)
            return NotFound("سفارش مبدأ یا مقصد پیدا نشد.");
        if (sourceOrder.Id == targetOrder.Id)
            return BadRequest("سفارش مبدأ و مقصد نمی‌توانند یکسان باشند.");
        if (sourceOrder.Status is not (OrderStatus.Completed or OrderStatus.HasShortage))
            return Conflict("فقط از سفارش تکمیل‌شده می‌توان کالای آماده انتقال داد.");
        if (targetOrder.Status != OrderStatus.Active)
            return Conflict("سفارش مقصد باید در وضعیت در حال انجام باشد.");

        var sourceIds = request.Lines.Select(x => x.SourceOrderItemId).Distinct().ToArray();
        var targetIds = request.Lines.Select(x => x.TargetOrderItemId).Distinct().ToArray();
        var sourceItems = await db.OrderItems
            .Where(x => sourceIds.Contains(x.Id) && x.OrderId == sourceOrder.Id)
            .Include(x => x.Components)
            .Include(x => x.WarehouseAllocations)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var targetItems = await db.OrderItems
            .Where(x => targetIds.Contains(x.Id) && x.OrderId == targetOrder.Id)
            .Include(x => x.Components)
            .Include(x => x.WarehouseAllocations)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (sourceItems.Count != sourceIds.Length || targetItems.Count != targetIds.Length)
            return BadRequest("یکی از اقلام انتخاب‌شده متعلق به سفارش مبدأ یا مقصد نیست.");

        var transfer = new OrderTransfer(sourceOrder.Id, targetOrder.Id, userId, request.Reason);
        foreach (var line in request.Lines)
        {
            if (!sourceItems.TryGetValue(line.SourceOrderItemId, out var source)
                || !targetItems.TryGetValue(line.TargetOrderItemId, out var target))
                return BadRequest("قلم انتخاب‌شده معتبر نیست.");

            if (!HasSameSpecification(source, target))
                return Conflict("مشخصات محصول مبدأ و مقصد یکسان نیست.");

            var allocation = source.WarehouseAllocations.SingleOrDefault(x => x.WarehouseId == line.WarehouseId);
            if (allocation is null || allocation.Quantity < line.Quantity)
                return Conflict("تعداد انتخاب‌شده در انبار مبدأ موجود نیست.");

            var targetAllocated = target.WarehouseAllocations.Sum(x => x.Quantity);
            if (targetAllocated + line.Quantity > target.Quantity)
                return Conflict("انتقال باعث می‌شود تعداد تأمین‌شده سفارش مقصد از تعداد درخواستی آن بیشتر شود.");

            transfer.AddLine(line.SourceOrderItemId, line.TargetOrderItemId, line.WarehouseId, line.Quantity);
        }

        // درخواست هنوز موجودی را جابه‌جا نمی‌کند؛ جابه‌جایی فقط پس از تأیید مدیر انجام می‌شود.
        db.OrderTransfers.Add(transfer);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = transfer.Id }, new { transfer.Id, Status = transfer.Status.ToString() });
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = PermissionNames.WorkflowManage)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid reviewerId)
            return Unauthorized();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var transfer = await db.OrderTransfers
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (transfer is null) return NotFound();
        if (transfer.Status != OrderTransferStatus.PendingApproval)
            return Conflict("این درخواست قبلاً بررسی شده است.");

        var sourceOrder = await db.Orders.SingleAsync(x => x.Id == transfer.SourceOrderId, cancellationToken);
        var targetOrder = await db.Orders.SingleAsync(x => x.Id == transfer.TargetOrderId, cancellationToken);
        if (sourceOrder.Status is not (OrderStatus.Completed or OrderStatus.HasShortage) || targetOrder.Status != OrderStatus.Active)
            return Conflict("وضعیت سفارش مبدأ یا مقصد پس از ثبت درخواست تغییر کرده است.");

        var sourceItemIds = transfer.Lines.Select(x => x.SourceOrderItemId).Distinct().ToArray();
        var targetItemIds = transfer.Lines.Select(x => x.TargetOrderItemId).Distinct().ToArray();
        var items = await db.OrderItems
            .Where(x => sourceItemIds.Contains(x.Id) || targetItemIds.Contains(x.Id))
            .Include(x => x.Components)
            .Include(x => x.WarehouseAllocations)
            .ToListAsync(cancellationToken);
        var itemMap = items.ToDictionary(x => x.Id);

        foreach (var line in transfer.Lines)
        {
            if (!itemMap.TryGetValue(line.SourceOrderItemId, out var source)
                || !itemMap.TryGetValue(line.TargetOrderItemId, out var target)
                || !HasSameSpecification(source, target))
                return Conflict("مشخصات یا اقلام سفارش پس از ثبت درخواست تغییر کرده‌اند.");

            var sourceAllocation = source.WarehouseAllocations.SingleOrDefault(x => x.WarehouseId == line.WarehouseId);
            if (sourceAllocation is null || sourceAllocation.Quantity < line.Quantity)
                return Conflict("موجودی انبار مبدأ برای این انتقال کافی نیست.");

            var targetAllocated = target.WarehouseAllocations.Sum(x => x.Quantity);
            var incomingForTarget = transfer.Lines
                .Where(x => x.TargetOrderItemId == line.TargetOrderItemId)
                .Sum(x => x.Quantity);
            if (targetAllocated + incomingForTarget > target.Quantity)
                return Conflict("ظرفیت تعداد درخواستی سفارش مقصد برای این انتقال کافی نیست.");
        }

        foreach (var line in transfer.Lines)
        {
            var source = itemMap[line.SourceOrderItemId];
            var target = itemMap[line.TargetOrderItemId];
            var sourceAllocation = source.WarehouseAllocations.Single(x => x.WarehouseId == line.WarehouseId);
            var targetAllocation = target.WarehouseAllocations.SingleOrDefault(x => x.WarehouseId == line.WarehouseId);

            if (sourceAllocation.Quantity == line.Quantity)
                db.OrderItemWarehouseAllocations.Remove(sourceAllocation);
            else
                sourceAllocation.DecreaseQuantity(line.Quantity);

            if (targetAllocation is null)
                db.OrderItemWarehouseAllocations.Add(new OrderItemWarehouseAllocation(target.Id, line.WarehouseId, line.Quantity));
            else
                targetAllocation.IncreaseQuantity(line.Quantity);
        }

        transfer.Approve(reviewerId);
        sourceOrder.MarkHasShortage();
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = PermissionNames.WorkflowManage)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid reviewerId)
            return Unauthorized();

        var transfer = await db.OrderTransfers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (transfer is null) return NotFound();
        if (transfer.Status != OrderTransferStatus.PendingApproval)
            return Conflict("این درخواست قبلاً بررسی شده است.");

        transfer.Reject(reviewerId);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static bool HasSameSpecification(OrderItem source, OrderItem target)
    {
        if (source.ProductId != target.ProductId || source.Components.Count != target.Components.Count)
            return false;

        var sourceComponents = source.Components.OrderBy(x => x.ComponentId)
            .Select(x => (x.ComponentId, x.Quantity)).ToArray();
        var targetComponents = target.Components.OrderBy(x => x.ComponentId)
            .Select(x => (x.ComponentId, x.Quantity)).ToArray();
        return sourceComponents.SequenceEqual(targetComponents);
    }
}

public sealed record CreateOrderTransferRequest(
    Guid SourceOrderId,
    Guid TargetOrderId,
    string Reason,
    IReadOnlyCollection<CreateOrderTransferLineRequest> Lines);

public sealed record CreateOrderTransferLineRequest(
    Guid SourceOrderItemId,
    Guid TargetOrderItemId,
    Guid WarehouseId,
    decimal Quantity);
