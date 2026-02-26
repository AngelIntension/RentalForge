using FluentValidation;
using RentalForge.Api.Models;

namespace RentalForge.Api.Validators;

public class ReturnRentalValidator : AbstractValidator<ReturnRentalRequest>
{
    public ReturnRentalValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.StaffId)
            .NotNull().WithMessage("'Staff Id' is required when amount is provided.")
            .GreaterThan(0)
            .When(x => x.Amount.HasValue);
    }
}
