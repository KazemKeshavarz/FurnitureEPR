using FluentValidation;

namespace FurnitureEPR.Application.Features.Workflows.Commands.CreateWorkflowVersion;

public sealed class CreateWorkflowVersionCommandValidator : AbstractValidator<CreateWorkflowVersionCommand>
{
    public CreateWorkflowVersionCommandValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.VersionNumber).GreaterThan(0);
    }
}
