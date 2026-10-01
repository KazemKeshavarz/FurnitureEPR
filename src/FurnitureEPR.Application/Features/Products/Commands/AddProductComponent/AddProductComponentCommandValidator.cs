using FluentValidation;

namespace FurnitureEPR.Application.Features.Products.Commands.AddProductComponent;

public sealed class AddProductComponentCommandValidator : AbstractValidator<AddProductComponentCommand>
{
    public AddProductComponentCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ComponentId).NotEmpty();
        RuleFor(x => x.DefaultQuantity).GreaterThanOrEqualTo(0);
    }
}
