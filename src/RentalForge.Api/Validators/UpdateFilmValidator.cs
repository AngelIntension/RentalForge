using FluentValidation;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;

namespace RentalForge.Api.Validators;

public class UpdateFilmValidator : AbstractValidator<UpdateFilmRequest>
{
    public UpdateFilmValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.ReleaseYear)
            .InclusiveBetween(1888, DateTime.UtcNow.Year + 5)
            .When(x => x.ReleaseYear.HasValue);

        RuleFor(x => x.LanguageId)
            .GreaterThan(0);

        RuleFor(x => x.OriginalLanguageId)
            .GreaterThan(0)
            .When(x => x.OriginalLanguageId.HasValue);

        RuleFor(x => x.RentalDuration)
            .GreaterThan((short)0);

        RuleFor(x => x.RentalRate)
            .GreaterThan(0m);

        RuleFor(x => x.Length)
            .GreaterThan((short)0)
            .When(x => x.Length.HasValue);

        RuleFor(x => x.ReplacementCost)
            .GreaterThan(0m);

        RuleFor(x => x.Rating)
            .IsInEnum()
            .When(x => x.Rating.HasValue);
    }
}
