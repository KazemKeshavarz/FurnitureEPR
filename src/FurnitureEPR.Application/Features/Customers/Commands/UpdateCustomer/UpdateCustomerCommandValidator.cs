using FluentValidation;

namespace FurnitureEPR.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(1000);
    }
}
