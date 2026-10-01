using MediatR;

namespace FurnitureEPR.Application.Features.Products.Queries;

public sealed record GetProductQuery(Guid Id) : IRequest<ProductDto?>;
