using FurnitureEPR.Application.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence.Repositories;

public sealed class OrderWorkflowRuntimeRepository
    : IOrderWorkflowRuntimeRepository
{
    private readonly ApplicationDbContext _db;

    public OrderWorkflowRuntimeRepository(ApplicationDbContext db)
        => _db = db;

    public async Task MoveAsync(
        Guid orderId,
        Guid categoryId,
        Guid transitionId,
        CancellationToken cancellationToken)
    {
        var instance = await _db.OrderWorkflowInstances
            .SingleOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.CategoryId == categoryId,
                cancellationToken);

        if (instance is null)
            throw new KeyNotFoundException(
                "Order workflow instance was not found.");

        var transition = await _db.WorkflowTransitions
            .SingleOrDefaultAsync(
                x => x.Id == transitionId
                    && x.WorkflowVersionId == instance.WorkflowVersionId,
                cancellationToken);

        if (transition is null)
            throw new InvalidOperationException(
                "The transition does not belong to the workflow version of this order.");

        if (transition.FromStageId != instance.CurrentStageId)
            throw new InvalidOperationException(
                "The transition is not valid for the current stage.");

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
