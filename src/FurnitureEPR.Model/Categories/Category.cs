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

    public void AssignWorkflow(Guid workflowVersionId)\n    {\n        if (workflowVersionId == Guid.Empty)\n            throw new ArgumentException("Workflow version is required.", nameof(workflowVersionId));\n\n        WorkflowVersionId = workflowVersionId;\n    }\n\n    public void RemoveWorkflow() => WorkflowVersionId = null;\n\n    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
    }
}
