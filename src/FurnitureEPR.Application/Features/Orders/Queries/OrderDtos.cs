namespace FurnitureEPR.Application.Features.Orders.Queries;

public sealed record OrderItemComponentDto(
    Guid Id,
    Guid ComponentId,
    string ComponentName,
    decimal Quantity);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    IReadOnlyCollection<OrderItemComponentDto> Components);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? FinalizedAtUtc,
    Guid? CreatedByUserId,
    IReadOnlyCollection<OrderItemDto> Items);
