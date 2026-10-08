using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.UpdateDraftOrder;

public sealed class UpdateDraftOrderCommandValidator : AbstractValidator<UpdateDraftOrderCommand>
{
    public UpdateDraftOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);

            item.RuleForEach(x => x.Components).ChildRules(component =>
            {
                component.RuleFor(x => x.ComponentId).NotEmpty();
                component.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            });
        });
    }
}
