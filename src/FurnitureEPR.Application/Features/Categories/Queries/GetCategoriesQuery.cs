using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Queries;

public sealed record GetCategoriesQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<CategoryListItemDto>>;
