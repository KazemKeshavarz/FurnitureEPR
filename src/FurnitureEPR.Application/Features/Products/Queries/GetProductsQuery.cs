using MediatR;

namespace FurnitureEPR.Application.Features.Products.Queries;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null) : IRequest<PagedResult<ProductListItemDto>>;
