using FurnitureEPR.Application.Features.Products.Commands.CreateProduct;
using FurnitureEPR.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;
    public ProductsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateProductCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetList([FromQuery] GetProductsQuery query, CancellationToken ct)
        => Ok(await _sender.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _sender.Send(new GetProductQuery(id), ct);
        return item is null ? NotFound() : Ok(item);
    }
}
