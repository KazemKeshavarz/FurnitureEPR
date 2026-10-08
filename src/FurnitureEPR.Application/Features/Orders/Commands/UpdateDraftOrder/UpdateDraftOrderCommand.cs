using MediatR;

namespace FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;

public sealed record UpdateDraftOrderItem(
    Guid ProductId,
    decimal Quantity,
    IReadOnlyCollection<UpdateDraftOrderItemComponent> Components,
    decimal UnitPrice = 0);

public sealed record UpdateDraftOrderItemComponent(
    Guid ComponentId,
    decimal Quantity);

public sealed record UpdateDraftOrderCommand(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyCollection<UpdateDraftOrderItem> Items,
    decimal DiscountAmount = 0) : IRequest;
