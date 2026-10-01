using MediatR;

namespace FurnitureEPR.Application.Features.Components.Queries;

public sealed record GetComponentsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<ComponentListItemDto>>;
