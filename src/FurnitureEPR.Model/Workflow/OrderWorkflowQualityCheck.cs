namespace FurnitureEPR.Model.Workflow;

public sealed class OrderWorkflowQualityCheck
{
    private OrderWorkflowQualityCheck() { }

    public Guid Id { get; private set; }
    public Guid OrderWorkflowInstanceId { get; private set; }
    public Guid StageId { get; private set; }
    public QualityControlResult Result { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CheckedAtUtc { get; private set; }
    public Guid? CheckedByUserId { get; private set; }

    public OrderWorkflowInstance OrderWorkflowInstance { get; private set; } = null!;
    public WorkflowStage Stage { get; private set; } = null!;

    // هر بار انجام QC یک رکورد مستقل ایجاد می‌کند تا تمام دفعات تأیید یا رد قابل پیگیری باشند.
    public OrderWorkflowQualityCheck(
        Guid orderWorkflowInstanceId,
        Guid stageId,
        QualityControlResult result,
        string? comment = null,
        Guid? checkedByUserId = null)
    {
        if (orderWorkflowInstanceId == Guid.Empty)
            throw new ArgumentException("Workflow instance is required.", nameof(orderWorkflowInstanceId));

        if (stageId == Guid.Empty)
            throw new ArgumentException("Stage is required.", nameof(stageId));

        if (checkedByUserId == Guid.Empty)
            throw new ArgumentException("Checker user is invalid.", nameof(checkedByUserId));

        Id = Guid.NewGuid();
        OrderWorkflowInstanceId = orderWorkflowInstanceId;
        StageId = stageId;
        Result = result;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CheckedAtUtc = DateTime.UtcNow;
        CheckedByUserId = checkedByUserId;
    }
}
