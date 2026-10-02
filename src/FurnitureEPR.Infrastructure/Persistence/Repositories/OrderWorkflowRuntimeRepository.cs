using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Model.Workflow;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderWorkflowRuntimeRepository
    : IOrderWorkflowRuntimeRepository
{
    private readonly ApplicationDbContext _db;

    public OrderWorkflowRuntimeRepository(ApplicationDbContext db)
        => _db = db;

    public async Task RecordQualityControlAsync(
        Guid orderId,
        Guid categoryId,
        QualityControlResult result,
        string? comment,
        Guid? checkedByUserId,
        CancellationToken cancellationToken)
    {
        // QC فقط روی Runtime Instance فعال همان سفارش و Category ثبت می‌شود.
        var instance = await _db.OrderWorkflowInstances
            .SingleOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.CategoryId == categoryId,
                cancellationToken);

        if (instance is null)
            throw new KeyNotFoundException("Order workflow instance was not found.");

        var currentStage = await _db.WorkflowStages
            .SingleOrDefaultAsync(
                x => x.Id == instance.CurrentStageId
                    && x.WorkflowVersionId == instance.WorkflowVersionId,
                cancellationToken);

        if (currentStage is null)
            throw new InvalidOperationException("Current workflow stage was not found.");

        if (!currentStage.RequiresQualityControl)
            throw new InvalidOperationException(
                "Quality control is not required for the current stage.");

        instance.RecordQualityControl(
            currentStage.Id,
            result,
            comment,
            checkedByUserId);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(
        Guid orderId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        // Instance مربوط به Category را پیدا می‌کنیم تا فقط همان بخش از Workflow کامل شود.
        var instance = await _db.OrderWorkflowInstances
            .SingleOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.CategoryId == categoryId,
                cancellationToken);

        if (instance is null)
            throw new KeyNotFoundException("Order workflow instance was not found.");

        instance.Complete();

        // فقط زمانی Order را Completed می‌کنیم که همه Workflowهای آن به پایان رسیده باشند.
        var hasIncompleteWorkflow = await _db.OrderWorkflowInstances
            .AnyAsync(
                x => x.OrderId == orderId
                    && x.Status == OrderWorkflowInstanceStatus.Active,
                cancellationToken);

        if (!hasIncompleteWorkflow)
        {
            var order = await _db.Orders
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("Order was not found.");

            order.MarkCompleted();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveAsync(
        Guid orderId,
        Guid categoryId,
        Guid transitionId,
        CancellationToken cancellationToken)
    {
        // ابتدا Runtime Instance مشخص سفارش و Category را پیدا می‌کنیم.
        var instance = await _db.OrderWorkflowInstances
            .SingleOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.CategoryId == categoryId,
                cancellationToken);

        if (instance is null)
            throw new KeyNotFoundException(
                "Order workflow instance was not found.");

        // Transition باید متعلق به همان نسخه Workflow باشد که سفارش هنگام Finalize گرفته است.
        var transition = await _db.WorkflowTransitions
            .SingleOrDefaultAsync(
                x => x.Id == transitionId
                    && x.WorkflowVersionId == instance.WorkflowVersionId,
                cancellationToken);

        if (transition is null)
            throw new InvalidOperationException(
                "The transition does not belong to the workflow version of this order.");

        // سفارش فقط می‌تواند از Stage فعلی خودش حرکت کند.
        if (transition.FromStageId != instance.CurrentStageId)
            throw new InvalidOperationException(
                "The transition is not valid for the current stage.");

        // Stage مقصد نیز باید در همان نسخه و فعال باشد.
        var currentStage = await _db.WorkflowStages
            .SingleAsync(
                x => x.Id == instance.CurrentStageId,
                cancellationToken);

        // اگر Stage فعلی نیاز به QC دارد، ابتدا باید آخرین QC آن Approved شده باشد.
        if (currentStage.RequiresQualityControl
            && !instance.IsQualityControlApproved(currentStage.Id))
        {
            throw new InvalidOperationException(
                "Quality control approval is required before moving from the current stage.");
        }

        var targetStageExists = await _db.WorkflowStages
            .AnyAsync(
                x => x.Id == transition.ToStageId
                    && x.WorkflowVersionId == instance.WorkflowVersionId
                    && x.IsActive,
                cancellationToken);

        if (!targetStageExists)
            throw new InvalidOperationException(
                "The target stage is not active in the workflow version.");

        instance.MoveTo(
            transition.ToStageId,
            transition.Id);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
