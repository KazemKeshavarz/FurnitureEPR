using FurnitureEPR.Application.Features.Products;
using MediatR;

namespace FurnitureEPR.Application.Features.Products.Queries;

public sealed class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto?>
{
    private readonly IProductReadRepository _repository;
    public GetProductQueryHandler(IProductReadRepository repository) => _repository = repository;

    public Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    private readonly IProductReadRepository _repository;
    public GetProductsQueryHandler(IProductReadRepository repository) => _repository = repository;

    public Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        => _repository.GetPagedAsync(request.Page, request.PageSize, request.Search, request.CategoryId, cancellationToken);
}
