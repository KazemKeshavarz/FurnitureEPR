namespace FurnitureEPR.Application.Features.Components.Queries;

public sealed record ComponentDto(Guid Id, string Name);

public sealed record ComponentListItemDto(Guid Id, string Name);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
