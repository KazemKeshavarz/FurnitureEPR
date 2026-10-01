namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderFinalizationRepository
{
    Task FinalizeAsync(Guid orderId, CancellationToken cancellationToken);
}
