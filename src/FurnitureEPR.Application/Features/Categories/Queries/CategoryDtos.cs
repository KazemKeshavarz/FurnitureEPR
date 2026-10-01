namespace FurnitureEPR.Application.Features.Categories.Queries;

public sealed record CategoryDto(Guid Id, string Name);

public sealed record CategoryListItemDto(Guid Id, string Name);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
