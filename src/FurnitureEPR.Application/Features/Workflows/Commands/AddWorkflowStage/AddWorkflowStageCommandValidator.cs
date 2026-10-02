using FluentValidation;

namespace FurnitureEPR.Application.Features.Workflows.Commands.AddWorkflowStage;

public sealed class AddWorkflowStageCommandValidator : AbstractValidator<AddWorkflowStageCommand>
{
    public AddWorkflowStageCommandValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ResponsibleRoleId)
            .Must(x => x is null || x != Guid.Empty)
            .WithMessage("Responsible role is invalid.");
    }
}
