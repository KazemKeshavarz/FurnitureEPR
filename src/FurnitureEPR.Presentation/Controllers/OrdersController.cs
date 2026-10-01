using FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;
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
