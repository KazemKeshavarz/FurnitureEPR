using MediatR;

namespace FurnitureEPR.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(Guid CategoryId, string Name) : IRequest<Guid>;
