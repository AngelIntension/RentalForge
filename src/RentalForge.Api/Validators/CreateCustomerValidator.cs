using FluentValidation;
using RentalForge.Api.Models;

namespace RentalForge.Api.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(45);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(45);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(50)
            .When(x => x.Email is not null);

        RuleFor(x => x.StoreId)
            .GreaterThan(0);

        RuleFor(x => x.AddressId)
            .GreaterThan(0);
    }
}
