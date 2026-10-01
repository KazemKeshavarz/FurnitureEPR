using FluentValidation;

namespace FurnitureEPR.Application.Features.Components.Commands.CreateComponent;

public sealed class CreateComponentCommandValidator : AbstractValidator<CreateComponentCommand>
{
    public CreateComponentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
