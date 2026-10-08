namespace FurnitureEPR.Model.Categories;

public sealed class Category
{
    private Category() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid? WorkflowVersionId { get; private set; }

    public Workflow.WorkflowVersion? WorkflowVersion { get; private set; }

    public ICollection<Products.Product> Products { get; private set; } = new List<Products.Product>();

    public Category(string name)
    {
        SetName(name);
    }

    public void AssignWorkflow(Guid workflowVersionId)
    {
        if (workflowVersionId == Guid.Empty)
            throw new ArgumentException("Workflow version is required.", nameof(workflowVersionId));

        WorkflowVersionId = workflowVersionId;
    }

    public void RemoveWorkflow() => WorkflowVersionId = null;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
    }
}
