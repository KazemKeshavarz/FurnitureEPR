namespace FurnitureEPR.Application.Features.Products.Queries;

public sealed record ProductComponentDto(
    Guid Id,
    Guid ComponentId,
    string ComponentName,
    decimal DefaultQuantity);

public sealed record ProductDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyCollection<ProductComponentDto> Components);

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string CategoryName);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
