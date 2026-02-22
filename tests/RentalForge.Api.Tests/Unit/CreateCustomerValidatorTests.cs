using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentValidation.TestHelper;
using RentalForge.Api.Models;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _sut = new();
    private readonly Fixture _fixture = new();

    public CreateCustomerValidatorTests()
    {
        _fixture.Customize<CreateCustomerRequest>(c => c
            .With(x => x.FirstName, () => _fixture.Create<string>()[..10])
            .With(x => x.LastName, () => _fixture.Create<string>()[..10])
            .With(x => x.Email, "test@example.com")
            .With(x => x.StoreId, 1)
            .With(x => x.AddressId, 1));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<CreateCustomerRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FirstName_Required(string? firstName)
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { FirstName = firstName! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void FirstName_MaxLength_45()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { FirstName = new string('A', 46) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void FirstName_At_MaxLength_Passes()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { FirstName = new string('A', 45) };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LastName_Required(string? lastName)
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { LastName = lastName! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void LastName_MaxLength_45()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { LastName = new string('B', 46) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void LastName_At_MaxLength_Passes()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { LastName = new string('B', 45) };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Email_Invalid_Format_Fails()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { Email = "not-an-email" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_Format_Passes()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { Email = "user@example.com" };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Null_Passes()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { Email = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_MaxLength_50()
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { Email = new string('a', 42) + "@test.com" }; // 51 chars
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StoreId_Must_Be_Greater_Than_Zero(int storeId)
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { StoreId = storeId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StoreId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddressId_Must_Be_Greater_Than_Zero(int addressId)
    {
        var request = _fixture.Create<CreateCustomerRequest>() with { AddressId = addressId };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AddressId);
    }
}
