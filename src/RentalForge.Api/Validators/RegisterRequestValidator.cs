using FluentValidation;
using RentalForge.Api.Models.Auth;

namespace RentalForge.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("'Password' must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("'Password' must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("'Password' must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("'Password' must contain at least one special character.");

        RuleFor(x => x.Role)
            .Must(r => r is null or "Admin" or "Staff" or "Customer")
            .WithMessage("'Role' must be 'Admin', 'Staff', or 'Customer'.")
            .When(x => x.Role is not null);
    }
}
