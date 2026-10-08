using FurnitureEPR.Application.Features.Products.Commands.AddProductComponent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/components")]
public sealed class ProductComponentsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductComponentsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid productId,
        AddProductComponentBody body,
        CancellationToken ct)
    {
        await _sender.Send(
            new AddProductComponentCommand(productId, body.ComponentId, body.DefaultQuantity),
            ct);

        return NoContent();
    }
}

public sealed record AddProductComponentBody(
    Guid ComponentId,
    decimal DefaultQuantity);
