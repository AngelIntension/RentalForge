using AutoFixture;
using FluentAssertions;
using FluentValidation.TestHelper;
using RentalForge.Api.Models.Auth;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();
    private readonly Fixture _fixture = new();

    public RegisterRequestValidatorTests()
    {
        _fixture.Customize<RegisterRequest>(c => c
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "SecureP@ss1")
            .Without(x => x.Role));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<RegisterRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Email validation
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_Required(string? email)
    {
        var request = _fixture.Create<RegisterRequest>() with { Email = email! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Invalid_Format_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Email = "not-an-email" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_Format_Passes()
    {
        var request = _fixture.Create<RegisterRequest>() with { Email = "user@example.com" };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // Password validation
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Password_Required(string? password)
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = password! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_MinLength_8()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "Aa1@xyz" }; // 7 chars
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_At_MinLength_Passes()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "Aa1@xyzz" }; // 8 chars
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Missing_Uppercase_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "securep@ss1" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("'Password' must contain at least one uppercase letter.");
    }

    [Fact]
    public void Password_Missing_Lowercase_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "SECUREP@SS1" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("'Password' must contain at least one lowercase letter.");
    }

    [Fact]
    public void Password_Missing_Digit_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "SecureP@ss" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("'Password' must contain at least one digit.");
    }

    [Fact]
    public void Password_Missing_SpecialChar_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "SecurePass1" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("'Password' must contain at least one special character.");
    }

    [Fact]
    public void Password_All_Errors_Aggregated()
    {
        var request = _fixture.Create<RegisterRequest>() with { Password = "short" }; // missing upper, digit, special, too short
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.Errors.Where(e => e.PropertyName == "Password").Should().HaveCountGreaterThan(1);
    }

    // Role validation
    [Fact]
    public void Role_Null_Defaults_Passes()
    {
        var request = _fixture.Create<RegisterRequest>() with { Role = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Staff")]
    [InlineData("Customer")]
    public void Role_Valid_Values_Pass(string role)
    {
        var request = _fixture.Create<RegisterRequest>() with { Role = role };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Role_Invalid_Value_Fails()
    {
        var request = _fixture.Create<RegisterRequest>() with { Role = "SuperAdmin" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Multiple_Errors_Aggregated_Across_Fields()
    {
        var request = new RegisterRequest
        {
            Email = "",
            Password = "",
            Role = "Invalid"
        };
        var result = _sut.TestValidate(request);
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }
}
