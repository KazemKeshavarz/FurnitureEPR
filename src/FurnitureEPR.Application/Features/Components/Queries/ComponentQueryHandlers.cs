using FurnitureEPR.Application.Features.Components;
using MediatR;

namespace FurnitureEPR.Application.Features.Components.Queries;

public sealed class GetComponentQueryHandler : IRequestHandler<GetComponentQuery, ComponentDto?>
{
    private readonly IComponentReadRepository _repository;
    public GetComponentQueryHandler(IComponentReadRepository repository) => _repository = repository;

    public Task<ComponentDto?> Handle(GetComponentQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class GetComponentsQueryHandler : IRequestHandler<GetComponentsQuery, PagedResult<ComponentListItemDto>>
{
    private readonly IComponentReadRepository _repository;
    public GetComponentsQueryHandler(IComponentReadRepository repository) => _repository = repository;

    public Task<PagedResult<ComponentListItemDto>> Handle(GetComponentsQuery request, CancellationToken cancellationToken)
        => _repository.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
}
