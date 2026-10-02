using FluentValidation;

namespace FurnitureEPR.Application.Features.Orders.Commands.RecordQualityControl;

public sealed class RecordQualityControlCommandValidator
    : AbstractValidator<RecordQualityControlCommand>
{
    public RecordQualityControlCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(2000);
        RuleFor(x => x.Result).IsInEnum();
    }
}
