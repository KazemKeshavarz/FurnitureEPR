using FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderDraftRepository
{
    Task UpdateAsync(
        Guid orderId,
        Guid customerId,
        IReadOnlyCollection<UpdateDraftOrderItem> items,
        CancellationToken cancellationToken);
}
