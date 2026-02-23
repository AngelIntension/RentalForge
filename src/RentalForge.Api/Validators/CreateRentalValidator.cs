using FluentValidation;
using RentalForge.Api.Models;

namespace RentalForge.Api.Validators;

public class CreateRentalValidator : AbstractValidator<CreateRentalRequest>
{
    public CreateRentalValidator()
    {
        RuleFor(x => x.FilmId)
            .GreaterThan(0);

        RuleFor(x => x.StoreId)
            .GreaterThan(0);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.StaffId)
            .GreaterThan(0);
    }
}
