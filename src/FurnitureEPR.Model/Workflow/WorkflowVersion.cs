namespace FurnitureEPR.Model.Workflow;

public sealed class WorkflowVersion
{
    private readonly List<WorkflowStage> _stages = new();
    private readonly List<WorkflowTransition> _transitions = new();

    private WorkflowVersion() { }

    public Guid Id { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int VersionNumber { get; private set; }
    public bool IsPublished { get; private set; }

    public WorkflowDefinition WorkflowDefinition { get; private set; } = null!;
    public IReadOnlyCollection<WorkflowStage> Stages => _stages.AsReadOnly();
    public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    public WorkflowVersion(Guid workflowDefinitionId, int versionNumber)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        VersionNumber = versionNumber;
    }

    public void Publish()
    {
        if (_stages.Count == 0)
            throw new InvalidOperationException("A workflow version must contain at least one stage.");

        IsPublished = true;
    }

    public void AddStage(WorkflowStage stage)
    {
        if (IsPublished)
            throw new InvalidOperationException("Published workflow versions cannot be modified.");

        ArgumentNullException.ThrowIfNull(stage);
        _stages.Add(stage);
    }

    public void AddTransition(WorkflowTransition transition)
    {
        if (IsPublished)
            throw new InvalidOperationException("Published workflow versions cannot be modified.");

        ArgumentNullException.ThrowIfNull(transition);
        _transitions.Add(transition);
    }
}
