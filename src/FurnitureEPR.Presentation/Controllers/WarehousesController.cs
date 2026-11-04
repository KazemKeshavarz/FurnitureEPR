using FurnitureEPR.Application.Security;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Model.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize(Policy = PermissionNames.WorkflowManage)]
public sealed class WarehousesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WarehouseDto>>> Get(CancellationToken cancellationToken)
    {
        var items = await db.Warehouses.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new WarehouseDto(x.Id, x.Name, x.Code, x.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(WarehouseRequest request, CancellationToken cancellationToken)
    {
        if (await db.Warehouses.AnyAsync(x => x.Code == request.Code.Trim().ToUpper(), cancellationToken))
            return Conflict("کد این انبار قبلاً ثبت شده است.");

        var warehouse = new Warehouse(request.Name, request.Code);
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = warehouse.Id }, warehouse.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, WarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (warehouse is null) return NotFound();
        if (await db.Warehouses.AnyAsync(x => x.Id != id && x.Code == request.Code.Trim().ToUpper(), cancellationToken))
            return Conflict("کد این انبار قبلاً ثبت شده است.");

        warehouse.Update(request.Name, request.Code);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] bool isActive, CancellationToken cancellationToken)
    {
        var warehouse = await db.Warehouses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (warehouse is null) return NotFound();
        warehouse.SetActive(isActive);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record WarehouseDto(Guid Id, string Name, string Code, bool IsActive);
public sealed record WarehouseRequest(string Name, string Code);
