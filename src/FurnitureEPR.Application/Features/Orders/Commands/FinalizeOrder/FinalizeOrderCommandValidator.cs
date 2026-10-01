using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.FinalizeOrder;

public sealed class FinalizeOrderCommandValidator : AbstractValidator<FinalizeOrderCommand>
{
    public FinalizeOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
