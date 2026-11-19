using MediatR;
namespace FurnitureEPR.Application.Features.Products.Commands.UpdateProduct;
public sealed record UpdateProductCommand(Guid Id, Guid CategoryId, string Name) : IRequest;
