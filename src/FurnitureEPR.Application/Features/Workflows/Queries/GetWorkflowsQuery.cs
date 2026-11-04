using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Queries;

public sealed record GetWorkflowsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<WorkflowPagedResult<WorkflowListItemDto>>;

public sealed record WorkflowPagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
