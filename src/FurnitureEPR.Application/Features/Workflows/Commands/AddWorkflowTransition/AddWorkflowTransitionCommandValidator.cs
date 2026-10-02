using FluentValidation;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowTransition;

public sealed class AddWorkflowTransitionCommandValidator
    : AbstractValidator<AddWorkflowTransitionCommand>
{
    public AddWorkflowTransitionCommandValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
        RuleFor(x => x.FromStageId).NotEmpty();
        RuleFor(x => x.ToStageId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x)
            .Must(x => x.FromStageId != x.ToStageId)
            .WithMessage("FromStage and ToStage must be different.");
    }
}
