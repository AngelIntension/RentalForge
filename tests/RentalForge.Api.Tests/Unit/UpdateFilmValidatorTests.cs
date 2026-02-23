using AutoFixture;
using FluentValidation.TestHelper;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Models;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class UpdateFilmValidatorTests
{
    private readonly UpdateFilmValidator _sut = new();
    private readonly Fixture _fixture = new();

    public UpdateFilmValidatorTests()
    {
        _fixture.Customize<UpdateFilmRequest>(c => c
            .With(x => x.Title, () => _fixture.Create<string>()[..10])
            .With(x => x.Description, "A test film description")
            .With(x => x.ReleaseYear, 2020)
            .With(x => x.LanguageId, 1)
            .With(x => x.OriginalLanguageId, (int?)null)
            .With(x => x.RentalDuration, (short)5)
            .With(x => x.RentalRate, 3.99m)
            .With(x => x.Length, (short?)120)
            .With(x => x.ReplacementCost, 24.99m)
            .With(x => x.Rating, MpaaRating.PG)
            .With(x => x.SpecialFeatures, (string[]?)null));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<UpdateFilmRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Title validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_Required(string? title)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Title = title! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_MaxLength_255()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Title = new string('A', 256) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_At_MaxLength_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Title = new string('A', 255) };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // Description validation

    [Fact]
    public void Description_MaxLength_1000()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Description = new string('A', 1001) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_At_MaxLength_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Description = new string('A', 1000) };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Null_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Description = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ReleaseYear validation

    [Fact]
    public void ReleaseYear_Below_1888_Fails()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReleaseYear = 1887 };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Fact]
    public void ReleaseYear_Above_CurrentYearPlus5_Fails()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReleaseYear = DateTime.UtcNow.Year + 6 };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Fact]
    public void ReleaseYear_At_1888_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReleaseYear = 1888 };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Fact]
    public void ReleaseYear_At_CurrentYearPlus5_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReleaseYear = DateTime.UtcNow.Year + 5 };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Fact]
    public void ReleaseYear_Null_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReleaseYear = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseYear);
    }

    // LanguageId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LanguageId_Must_Be_Greater_Than_Zero(int languageId)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { LanguageId = languageId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LanguageId);
    }

    // OriginalLanguageId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void OriginalLanguageId_Must_Be_Greater_Than_Zero_When_Set(int originalLanguageId)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { OriginalLanguageId = originalLanguageId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OriginalLanguageId);
    }

    [Fact]
    public void OriginalLanguageId_Null_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { OriginalLanguageId = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalLanguageId);
    }

    // RentalDuration validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RentalDuration_Must_Be_Greater_Than_Zero(short rentalDuration)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { RentalDuration = rentalDuration };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RentalDuration);
    }

    // RentalRate validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RentalRate_Must_Be_Greater_Than_Zero(double rentalRate)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { RentalRate = (decimal)rentalRate };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RentalRate);
    }

    // Length validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Length_Must_Be_Greater_Than_Zero_When_Set(short length)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Length = length };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Length);
    }

    [Fact]
    public void Length_Null_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Length = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Length);
    }

    // ReplacementCost validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReplacementCost_Must_Be_Greater_Than_Zero(double replacementCost)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { ReplacementCost = (decimal)replacementCost };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ReplacementCost);
    }

    // Rating validation

    [Fact]
    public void Rating_Invalid_Value_Fails()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Rating = (MpaaRating)99 };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Rating_Null_Passes()
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Rating = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Theory]
    [InlineData(MpaaRating.G)]
    [InlineData(MpaaRating.PG)]
    [InlineData(MpaaRating.Pg13)]
    [InlineData(MpaaRating.R)]
    [InlineData(MpaaRating.Nc17)]
    public void Rating_Valid_Values_Pass(MpaaRating rating)
    {
        var request = _fixture.Create<UpdateFilmRequest>() with { Rating = rating };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }
}
