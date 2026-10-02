using FluentValidation;

namespace FurnitureEPR.Application.Features.Workflows.Commands.PublishWorkflowVersion;

public sealed class PublishWorkflowVersionCommandValidator
    : AbstractValidator<PublishWorkflowVersionCommand>
{
    public PublishWorkflowVersionCommandValidator()
    {
        RuleFor(x => x.WorkflowVersionId).NotEmpty();
    }
}
