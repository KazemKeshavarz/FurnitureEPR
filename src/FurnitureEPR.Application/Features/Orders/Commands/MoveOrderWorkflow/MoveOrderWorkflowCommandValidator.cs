using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.MoveOrderWorkflow;

public sealed class MoveOrderWorkflowCommandValidator
    : AbstractValidator<MoveOrderWorkflowCommand>
{
    public MoveOrderWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId).NotEmpty();
        RuleFor(x => x.TransitionId).NotEmpty();
    }
}
