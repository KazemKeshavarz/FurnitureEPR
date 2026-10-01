using FurnitureEPR.Application.Features.Components.Commands.CreateComponent;
using FurnitureEPR.Application.Features.Components.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/components")]
public sealed class ComponentsController : ControllerBase
{
    private readonly ISender _sender;
    public ComponentsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateComponentCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ComponentListItemDto>>> GetList(
        [FromQuery] GetComponentsQuery query, CancellationToken ct)
        => Ok(await _sender.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComponentDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _sender.Send(new GetComponentQuery(id), ct);
        return item is null ? NotFound() : Ok(item);
    }
}
