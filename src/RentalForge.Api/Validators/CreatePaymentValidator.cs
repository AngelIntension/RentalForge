using FluentValidation;
using RentalForge.Api.Models;

namespace RentalForge.Api.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.RentalId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.StaffId)
            .GreaterThan(0);
    }
}
