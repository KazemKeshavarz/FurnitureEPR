using FluentValidation;
using FurnitureEPR.Presentation.Controllers;
using FurnitureEPR.Application.Security;

namespace FurnitureEPR.Presentation.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public sealed class PermissionRequestValidator : AbstractValidator<PermissionRequest>
{
    public PermissionRequestValidator()
    {
        RuleFor(x => x.Permission)
            .NotEmpty()
            .MaximumLength(150)
            .Matches("^[a-z0-9]+([._-][a-z0-9]+)*$")
            .WithMessage("Permission باید با فرمت معتبر ارسال شود.");
    }
}

public sealed class CreateIdentityUserRequestValidator : AbstractValidator<CreateIdentityUserRequest>
{
    public CreateIdentityUserRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(200);
    }
}
