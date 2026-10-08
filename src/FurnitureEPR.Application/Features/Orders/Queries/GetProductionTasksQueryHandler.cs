using FurnitureEPR.Application.Security;
using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed class GetProductionTasksQueryHandler
    : IRequestHandler<GetProductionTasksQuery, PagedProductionTaskDto>
{
    private readonly IProductionReadRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetProductionTasksQueryHandler(
        IProductionReadRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public Task<PagedProductionTaskDto> Handle(
        GetProductionTasksQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("Authenticated user is required.");

        return _repository.GetTasksAsync(
            request.Page,
            request.PageSize,
            request.Search,
            _currentUser.RoleIds,
            cancellationToken);
    }
}