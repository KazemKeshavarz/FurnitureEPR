using FurnitureEPR.Application.Security;
using FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;
using FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;
using FurnitureEPR.Application.Features.Orders.Commands.FinalizeOrder;
using FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;
using FurnitureEPR.Application.Features.Orders.Commands.RecordQualityControl;
using FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;
using FurnitureEPR.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}/draft")]
    public async Task<IActionResult> UpdateDraft(
        Guid id,
        UpdateDraftOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.OrderId)
            return BadRequest("Route id and OrderId must match.");

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/finalize")]
    public async Task<IActionResult> Finalize(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new FinalizeOrderCommand(id),
            cancellationToken);

        return NoContent();
    }

    // اجرای یک Transition مشخص روی گردشکار Runtime سفارش.
    [Authorize(Policy = PermissionNames.WorkflowMove)]
    [HttpPost("{orderId:guid}/workflow/{categoryId:guid}/transitions/{transitionId:guid}")]
    public async Task<IActionResult> MoveWorkflow(
        Guid orderId,
        Guid categoryId,
        Guid transitionId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new MoveOrderWorkflowCommand(
                orderId,
                categoryId,
                transitionId),
            cancellationToken);

        return NoContent();
    }

    // پایان دادن به Workflow یک Category مشخص؛ خود Order فقط پس از پایان همه Workflowها Completed می‌شود.
    [Authorize(Policy = PermissionNames.WorkflowComplete)]
    [HttpPost("{orderId:guid}/workflow/{categoryId:guid}/complete")]
    public async Task<IActionResult> CompleteWorkflow(
        Guid orderId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new CompleteOrderWorkflowCommand(orderId, categoryId),
            cancellationToken);

        return NoContent();
    }

    // ثبت نتیجه QC برای Stage فعلی؛ Reject باعث خروج از Stage نمی‌شود و امکان QC مجدد وجود دارد.
    [Authorize(Policy = PermissionNames.WorkflowQualityControl)]
    [HttpPost("{orderId:guid}/workflow/{categoryId:guid}/quality-control")]
    public async Task<IActionResult> RecordQualityControl(
        Guid orderId,
        Guid categoryId,
        RecordQualityControlCommand command,
        CancellationToken cancellationToken)
    {
        if (orderId != command.OrderId || categoryId != command.CategoryId)
            return BadRequest("Route ids and command ids must match.");

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("production/tasks")]
    public async Task<ActionResult<PagedProductionTaskDto>> GetProductionTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetProductionTasksQuery(page, pageSize, search),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedOrderDto>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] FurnitureEPR.Model.Orders.OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetOrdersQuery(page, pageSize, search, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _sender.Send(
            new GetOrderQuery(id),
            cancellationToken);

        return order is null ? NotFound() : Ok(order);
    }
}
