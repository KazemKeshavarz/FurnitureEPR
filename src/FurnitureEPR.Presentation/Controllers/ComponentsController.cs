using Microsoft.AspNetCore.Authorization;
using FurnitureEPR.Application.Features.Components.Commands.CreateComponent;
using FurnitureEPR.Application.Features.Components;
using FurnitureEPR.Application.Features.Components.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FurnitureEPR.Application.Security;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/components")]
public sealed class ComponentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IComponentRepository _repository;
    public ComponentsController(ISender sender, IComponentRepository repository) { _sender = sender; _repository = repository; }

    [Authorize(Policy = PermissionNames.IdentityManageMasterData)]
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateComponentCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [Authorize(Policy = PermissionNames.IdentityManageMasterData)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateComponentBody body, CancellationToken ct)
    {
        await _repository.UpdateAsync(id, body.Name, ct);
        return NoContent();
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

public sealed record UpdateComponentBody(string Name);
