using FurnitureEPR.Application.Features.Categories.Commands.AssignWorkflow;
using FurnitureEPR.Application.Features.Categories.Commands.CreateCategory;
using FurnitureEPR.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCategoryCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryListItemDto>>> GetList(
        [FromQuery] GetCategoriesQuery query,
        CancellationToken ct)
        => Ok(await _sender.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _sender.Send(new GetCategoryQuery(id), ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{categoryId:guid}/workflow")]
    public async Task<IActionResult> AssignWorkflow(
        Guid categoryId,
        AssignCategoryWorkflowCommand command,
        CancellationToken ct)
    {
        if (categoryId != command.CategoryId)
            return BadRequest("Route id and CategoryId must match.");

        await _sender.Send(command, ct);
        return NoContent();
    }
}
