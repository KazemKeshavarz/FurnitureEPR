using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.Components).NotNull();

            item.RuleForEach(x => x.Components).ChildRules(component =>
            {
                component.RuleFor(x => x.ComponentId).NotEmpty();
                component.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            });
        });
    }
}
