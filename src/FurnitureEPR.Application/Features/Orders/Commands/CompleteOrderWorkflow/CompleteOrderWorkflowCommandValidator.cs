using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.CompleteOrderWorkflow;

public sealed class CompleteOrderWorkflowCommandValidator
    : AbstractValidator<CompleteOrderWorkflowCommand>
{
    public CompleteOrderWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId).NotEmpty();
    }
}
