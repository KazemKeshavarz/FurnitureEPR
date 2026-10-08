using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderItem(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    IReadOnlyCollection<CreateOrderItemComponent> Components);

public sealed record CreateOrderItemComponent(
    Guid ComponentId,
    decimal Quantity);

public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid? CreatedByUserId,
    decimal DiscountAmount,
    IReadOnlyCollection<CreateOrderItem> Items) : IRequest<Guid>;
