using FurnitureEPR.Application.Features.Customers.Commands.CreateCustomer;
using FurnitureEPR.Application.Features.Customers.Commands.UpdateCustomer;
using FurnitureEPR.Application.Features.Customers.Queries.GetCustomer;
using FurnitureEPR.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customerId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = customerId },
            customerId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id and command id must match.");

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerListItemDto>>> GetList(
        [FromQuery] GetCustomersQuery query,
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(
            new GetCustomerQuery(id),
            cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }
}
