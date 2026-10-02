namespace FurnitureEPR.Model.Workflow;

public sealed class OrderWorkflowHistory
{
    private OrderWorkflowHistory() { }

    public Guid Id { get; private set; }
    public Guid OrderWorkflowInstanceId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public Guid TransitionId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    public OrderWorkflowInstance OrderWorkflowInstance { get; private set; } = null!;
    public WorkflowStage FromStage { get; private set; } = null!;
    public WorkflowStage ToStage { get; private set; } = null!;
    public WorkflowTransition Transition { get; private set; } = null!;

    // این رکورد هر بار که سفارش از یک مرحله به مرحله دیگر می‌رود، مسیر طی‌شده را ثبت می‌کند.
    internal OrderWorkflowHistory(
        Guid orderWorkflowInstanceId,
        Guid fromStageId,
        Guid toStageId,
        Guid transitionId)
    {
        OrderWorkflowInstanceId = orderWorkflowInstanceId;
        FromStageId = fromStageId;
        ToStageId = toStageId;
        TransitionId = transitionId;
        OccurredAtUtc = DateTime.UtcNow;
    }
}
