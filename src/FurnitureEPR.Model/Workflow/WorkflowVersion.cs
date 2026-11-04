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
        Id = Guid.NewGuid();
        WorkflowDefinitionId = workflowDefinitionId;
        VersionNumber = versionNumber;
    }

    // انتشار یعنی این نسخه برای استفاده Runtime آماده است و دیگر نباید ساختارش تغییر کند.
    public void Publish()
    {
        if (_stages.Count == 0)
            throw new InvalidOperationException("A workflow version must contain at least one stage.");

        IsPublished = true;
    }

    // Stage فقط قبل از Publish شدن نسخه قابل اضافه شدن است.
    public void AddStage(WorkflowStage stage)
    {
        if (IsPublished)
            throw new InvalidOperationException("Published workflow versions cannot be modified.");

        ArgumentNullException.ThrowIfNull(stage);
        _stages.Add(stage);
    }

    // مرحله فقط در نسخه پیش‌نویس قابل ویرایش یا حذف است.
    public void UpdateStage(Guid stageId, string name, string code, int sortOrder, bool requiresQualityControl, Guid? responsibleRoleId)
    {
        EnsureDraft();
        var stage = _stages.SingleOrDefault(x => x.Id == stageId)
            ?? throw new KeyNotFoundException("Workflow stage was not found.");
        if (_stages.Any(x => x.Id != stageId && (x.Code == code || x.SortOrder == sortOrder)))
            throw new InvalidOperationException("Stage code and sort order must be unique within a version.");
        stage.Update(name, code, sortOrder, requiresQualityControl, responsibleRoleId);
    }

    public void RemoveStage(Guid stageId)
    {
        EnsureDraft();
        if (_transitions.Any(x => x.FromStageId == stageId || x.ToStageId == stageId))
            throw new InvalidOperationException("Remove transitions referencing this stage first.");
        var stage = _stages.SingleOrDefault(x => x.Id == stageId)
            ?? throw new KeyNotFoundException("Workflow stage was not found.");
        _stages.Remove(stage);
    }

    public void UpdateTransition(Guid transitionId, Guid fromStageId, Guid toStageId, string name)
    {
        EnsureDraft();
        if (!_stages.Any(x => x.Id == fromStageId) || !_stages.Any(x => x.Id == toStageId))
            throw new InvalidOperationException("Both transition stages must belong to this version.");
        if (_transitions.Any(x => x.Id != transitionId && x.FromStageId == fromStageId && x.ToStageId == toStageId))
            throw new InvalidOperationException("This transition already exists.");
        var transition = _transitions.SingleOrDefault(x => x.Id == transitionId)
            ?? throw new KeyNotFoundException("Workflow transition was not found.");
        transition.Update(fromStageId, toStageId, name);
    }

    public void RemoveTransition(Guid transitionId)
    {
        EnsureDraft();
        var transition = _transitions.SingleOrDefault(x => x.Id == transitionId)
            ?? throw new KeyNotFoundException("Workflow transition was not found.");
        _transitions.Remove(transition);
    }

    private void EnsureDraft()
    {
        if (IsPublished)
            throw new InvalidOperationException("Published workflow versions cannot be modified.");
    }

    // Transition نیز بخشی از ساختار ثابت نسخه Workflow است.
    public void AddTransition(WorkflowTransition transition)
    {
        if (IsPublished)
            throw new InvalidOperationException("Published workflow versions cannot be modified.");

        ArgumentNullException.ThrowIfNull(transition);
        _transitions.Add(transition);
    }
}
