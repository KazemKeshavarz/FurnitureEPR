using FurnitureEPR.Application.Features.Categories;
using MediatR;

namespace FurnitureEPR.Application.Features.Categories.Queries;

public sealed class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, CategoryDto?>
{
    private readonly ICategoryReadRepository _repository;
    public GetCategoryQueryHandler(ICategoryReadRepository repository) => _repository = repository;

    public Task<CategoryDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryListItemDto>>
{
    private readonly ICategoryReadRepository _repository;
    public GetCategoriesQueryHandler(ICategoryReadRepository repository) => _repository = repository;

    public Task<PagedResult<CategoryListItemDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        => _repository.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
}
