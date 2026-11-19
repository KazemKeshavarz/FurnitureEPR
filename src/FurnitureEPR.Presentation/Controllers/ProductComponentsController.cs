using Microsoft.AspNetCore.Authorization;
using FurnitureEPR.Application.Features.Products.Commands.AddProductComponent;
using FurnitureEPR.Application.Features.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FurnitureEPR.Application.Security;

namespace FurnitureEPR.Presentation.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/components")]
public sealed class ProductComponentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IProductComponentRepository _repository;

    public ProductComponentsController(ISender sender, IProductComponentRepository repository)
    {
        _sender = sender;
        _repository = repository;
    }

    [Authorize(Policy = PermissionNames.IdentityManageMasterData)]
    [HttpPut("{productComponentId:guid}")]
    public async Task<IActionResult> UpdateDefaultQuantity(
        Guid productId,
        Guid productComponentId,
        UpdateProductComponentBody body,
        CancellationToken ct)
    {
        await _repository.UpdateDefaultQuantityAsync(productId, productComponentId, body.DefaultQuantity, ct);
        return NoContent();
    }

    [Authorize(Policy = PermissionNames.IdentityManageMasterData)]
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

public sealed record UpdateProductComponentBody(decimal DefaultQuantity);
