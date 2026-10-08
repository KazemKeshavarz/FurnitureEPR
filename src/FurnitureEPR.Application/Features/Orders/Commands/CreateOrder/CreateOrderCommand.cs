using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItem(
    Guid ProductId,
    decimal Quantity,
    IReadOnlyCollection<CreateOrderItemComponent> Components,
    decimal UnitPrice = 0);

public sealed record CreateOrderItemComponent(
    Guid ComponentId,
    decimal Quantity);

public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid? CreatedByUserId,
    IReadOnlyCollection<CreateOrderItem> Items,
    decimal DiscountAmount = 0) : IRequest<Guid>;
