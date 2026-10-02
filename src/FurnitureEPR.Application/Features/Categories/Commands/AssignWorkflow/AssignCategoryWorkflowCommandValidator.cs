using FluentValidation;

namespace FurnitureEPR.Application.Features.Categories.Commands.AssignWorkflow;

public sealed class AssignCategoryWorkflowCommandValidator
    : AbstractValidator<AssignCategoryWorkflowCommand>
{
    public AssignCategoryWorkflowCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
    }
}
