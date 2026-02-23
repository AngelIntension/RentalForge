using AutoFixture;
using FluentValidation.TestHelper;
using RentalForge.Api.Models;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class CreateRentalValidatorTests
{
    private readonly CreateRentalValidator _sut = new();
    private readonly Fixture _fixture = new();

    public CreateRentalValidatorTests()
    {
        _fixture.Customize<CreateRentalRequest>(c => c
            .With(x => x.FilmId, 1)
            .With(x => x.StoreId, 1)
            .With(x => x.CustomerId, 1)
            .With(x => x.StaffId, 1));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<CreateRentalRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // FilmId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FilmId_Must_Be_Greater_Than_Zero(int filmId)
    {
        var request = _fixture.Create<CreateRentalRequest>() with { FilmId = filmId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FilmId);
    }

    // StoreId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StoreId_Must_Be_Greater_Than_Zero(int storeId)
    {
        var request = _fixture.Create<CreateRentalRequest>() with { StoreId = storeId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StoreId);
    }

    // CustomerId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CustomerId_Must_Be_Greater_Than_Zero(int customerId)
    {
        var request = _fixture.Create<CreateRentalRequest>() with { CustomerId = customerId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    // StaffId validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StaffId_Must_Be_Greater_Than_Zero(int staffId)
    {
        var request = _fixture.Create<CreateRentalRequest>() with { StaffId = staffId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StaffId);
    }

    // Multiple validation errors aggregated

    [Fact]
    public void All_Fields_At_Zero_Produces_Four_Errors()
    {
        var request = new CreateRentalRequest
        {
            FilmId = 0,
            StoreId = 0,
            CustomerId = 0,
            StaffId = 0
        };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FilmId);
        result.ShouldHaveValidationErrorFor(x => x.StoreId);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        result.ShouldHaveValidationErrorFor(x => x.StaffId);
    }
}
