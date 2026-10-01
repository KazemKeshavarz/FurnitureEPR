using FurnitureEPR.Application.Features.Customers.Commands.CreateCustomer;
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
