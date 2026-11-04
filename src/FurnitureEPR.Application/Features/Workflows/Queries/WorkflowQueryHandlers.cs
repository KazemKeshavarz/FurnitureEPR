using MediatR;

namespace FurnitureEPR.Application.Features.Workflows.Queries;

public sealed class GetWorkflowsQueryHandler
    : IRequestHandler<GetWorkflowsQuery, WorkflowPagedResult<WorkflowListItemDto>>
{
    private readonly IWorkflowRepository _repository;

    public GetWorkflowsQueryHandler(IWorkflowRepository repository) => _repository = repository;

    public Task<WorkflowPagedResult<WorkflowListItemDto>> Handle(
        GetWorkflowsQuery request,
        CancellationToken cancellationToken)
        => _repository.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
}

public sealed class GetWorkflowQueryHandler : IRequestHandler<GetWorkflowQuery, WorkflowDetailDto?>
{
    private readonly IWorkflowRepository _repository;

    public GetWorkflowQueryHandler(IWorkflowRepository repository) => _repository = repository;

    public Task<WorkflowDetailDto?> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}
