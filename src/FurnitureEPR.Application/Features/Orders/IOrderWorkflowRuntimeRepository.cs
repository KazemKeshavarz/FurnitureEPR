using FurnitureEPR.Model.Workflow;

namespace FurnitureEPR.Application.Features.Orders;

public interface IOrderWorkflowRuntimeRepository
{
    // یک Transition معتبر را روی Instance مربوط به سفارش و Category اجرا می‌کند.
    Task MoveAsync(
        Guid orderId,
        Guid categoryId,
        Guid transitionId,
        CancellationToken cancellationToken);

    // گردشکار یک Category را کامل می‌کند و در صورت پایان همه Workflowهای سفارش، خود Order را نیز Completed می‌کند.
    Task CompleteAsync(
        Guid orderId,
        Guid categoryId,
        CancellationToken cancellationToken);

    // نتیجه QC مرحله فعلی را ثبت می‌کند؛ Reject سفارش را در همان Stage نگه می‌دارد.
    Task RecordQualityControlAsync(
        Guid orderId,
        Guid categoryId,
        QualityControlResult result,
        string? comment,
        Guid? checkedByUserId,
        CancellationToken cancellationToken);
}
