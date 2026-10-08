using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;

public sealed record UpdateDraftOrderItem(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    IReadOnlyCollection<UpdateDraftOrderItemComponent> Components);

public sealed record UpdateDraftOrderItemComponent(
    Guid ComponentId,
    decimal Quantity);

public sealed record UpdateDraftOrderCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal DiscountAmount,
    IReadOnlyCollection<UpdateDraftOrderItem> Items) : IRequest;
