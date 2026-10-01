namespace FurnitureEPR.Model.Workflow;

public sealed class WorkflowStage
{
    private WorkflowStage() { }

    public Guid Id { get; private set; }
    public Guid WorkflowVersionId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool RequiresQualityControl { get; private set; }
    public bool IsActive { get; private set; }

    public WorkflowVersion WorkflowVersion { get; private set; } = null!;

    public WorkflowStage(
        Guid workflowVersionId,
        string name,
        string code,
        int sortOrder,
        bool requiresQualityControl = false)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));

        Name = Required(name, nameof(name));
        Code = Required(code, nameof(code));
        WorkflowVersionId = workflowVersionId;
        SortOrder = sortOrder;
        RequiresQualityControl = requiresQualityControl;
        IsActive = true;
    }

    public void SetActive(bool active) => IsActive = active;

    private static string Required(string value, string parameter)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", parameter)
            : value.Trim();
}
