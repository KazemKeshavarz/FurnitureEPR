using FurnitureEPR.Model.Categories;
using FurnitureEPR.Model.Orders;

namespace FurnitureEPR.Model.Workflow;

public sealed class OrderWorkflowInstance
{
    private readonly List<OrderWorkflowHistory> _history = new();

    private OrderWorkflowInstance() { }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid WorkflowVersionId { get; private set; }
    public Guid CurrentStageId { get; private set; }
    public OrderWorkflowInstanceStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public Order Order { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public WorkflowVersion WorkflowVersion { get; private set; } = null!;
    public WorkflowStage CurrentStage { get; private set; } = null!;
    public IReadOnlyCollection<OrderWorkflowHistory> History => _history.AsReadOnly();

    // این Instance اجرای یک نسخه مشخص از Workflow را برای یک Order و Category نگه می‌دارد.
    public OrderWorkflowInstance(
        Guid orderId,
        Guid categoryId,
        Guid workflowVersionId,
        Guid initialStageId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order is required.", nameof(orderId));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required.", nameof(categoryId));

        if (workflowVersionId == Guid.Empty)
            throw new ArgumentException("Workflow version is required.", nameof(workflowVersionId));

        if (initialStageId == Guid.Empty)
            throw new ArgumentException("Initial stage is required.", nameof(initialStageId));

        OrderId = orderId;
        CategoryId = categoryId;
        WorkflowVersionId = workflowVersionId;
        CurrentStageId = initialStageId;
        Status = OrderWorkflowInstanceStatus.Active;
        StartedAtUtc = DateTime.UtcNow;
    }

    // حرکت بین Stageها فقط از طریق Transition معتبر انجام می‌شود و هم‌زمان در History ثبت می‌گردد.
    public void MoveTo(Guid nextStageId, Guid transitionId)
    {
        if (Status != OrderWorkflowInstanceStatus.Active)
            throw new InvalidOperationException("Only active workflow instances can move between stages.");

        if (nextStageId == Guid.Empty)
            throw new ArgumentException("Next stage is required.", nameof(nextStageId));

        if (transitionId == Guid.Empty)
            throw new ArgumentException("Transition is required.", nameof(transitionId));

        var previousStageId = CurrentStageId;
        CurrentStageId = nextStageId;

        _history.Add(new OrderWorkflowHistory(
            Id,
            previousStageId,
            nextStageId,
            transitionId));
    }

    // زمانی استفاده می‌شود که Workflow مربوط به این Category به پایان رسیده باشد.
    public void Complete()
    {
        if (Status != OrderWorkflowInstanceStatus.Active)
            throw new InvalidOperationException("Only active workflow instances can be completed.");

        Status = OrderWorkflowInstanceStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderWorkflowInstanceStatus.Completed)
            throw new InvalidOperationException("Completed workflow instances cannot be cancelled.");

        Status = OrderWorkflowInstanceStatus.Cancelled;
    }
}
