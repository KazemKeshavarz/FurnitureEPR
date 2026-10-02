using FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;
using FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;
using FurnitureEPR.Application.Features.Orders.Commands.FinalizeOrder;
using FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;
using FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;
using FurnitureEPR.Application.Features.Orders.Queries;
using MediatR;
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
