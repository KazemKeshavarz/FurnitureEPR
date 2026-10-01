namespace FurnitureEPR.Model.Workflow;

public sealed class WorkflowTransition
{
    private WorkflowTransition() { }

    public Guid Id { get; private set; }
    public Guid WorkflowVersionId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public string Name { get; private set; } = null!;

    public WorkflowVersion WorkflowVersion { get; private set; } = null!;
    public WorkflowStage FromStage { get; private set; } = null!;
    public WorkflowStage ToStage { get; private set; } = null!;

    public WorkflowTransition(
        Guid workflowVersionId,
        Guid fromStageId,
        Guid toStageId,
        string name)
    {
        if (fromStageId == toStageId)
            throw new ArgumentException("A transition cannot point to the same stage.");

        WorkflowVersionId = workflowVersionId;
        FromStageId = fromStageId;
        ToStageId = toStageId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Transition name is required.", nameof(name))
            : name.Trim();
    }
}
