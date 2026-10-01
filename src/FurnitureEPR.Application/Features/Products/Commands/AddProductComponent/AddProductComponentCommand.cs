using MediatR;

namespace FurnitureEPR.Application.Features.Products.Commands.AddProductComponent;

public sealed record AddProductComponentCommand(
    Guid ProductId,
    Guid ComponentId,
    decimal DefaultQuantity) : IRequest;
