namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderWorkflowRuntimeRepository
{
    // یک Transition معتبر را روی Instance مربوط به سفارش و Category اجرا می‌کند.
    Task MoveAsync(
        Guid orderId,
        Guid categoryId,
        Guid transitionId,
        CancellationToken cancellationToken);
}
