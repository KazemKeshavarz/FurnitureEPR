namespace FurnitureEPR.Model.Workflow;

public sealed class WorkflowDefinition
{
    private readonly List<WorkflowVersion> _versions = new();

    private WorkflowDefinition() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;

    public IReadOnlyCollection<WorkflowVersion> Versions => _versions.AsReadOnly();

    public WorkflowDefinition(string name, string code)
    {
        SetName(name);
        SetCode(code);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name is required.", nameof(name));

        Name = name.Trim();
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Workflow code is required.", nameof(code));

        Code = code.Trim();
    }

    public WorkflowVersion CreateVersion(int versionNumber)
    {
        if (versionNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(versionNumber));

        if (_versions.Any(x => x.VersionNumber == versionNumber))
            throw new InvalidOperationException("Workflow version already exists.");

        var version = new WorkflowVersion(Id, versionNumber);
        _versions.Add(version);
        return version;
    }
}
