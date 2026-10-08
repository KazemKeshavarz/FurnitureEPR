namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    int ItemsCount);

public sealed record PagedOrderDto(
    IReadOnlyCollection<OrderListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);