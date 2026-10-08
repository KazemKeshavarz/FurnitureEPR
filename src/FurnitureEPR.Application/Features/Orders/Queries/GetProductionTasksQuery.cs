using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record GetProductionTasksQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PagedProductionTaskDto>;