using AutoFixture;
using FluentAssertions;
using FluentValidation.TestHelper;
using RentalForge.Api.Models.Auth;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();
    private readonly Fixture _fixture = new();

    public LoginRequestValidatorTests()
    {
        _fixture.Customize<LoginRequest>(c => c
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "SecureP@ss1"));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<LoginRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_Required(string? email)
    {
        var request = _fixture.Create<LoginRequest>() with { Email = email! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Invalid_Format_Fails()
    {
        var request = _fixture.Create<LoginRequest>() with { Email = "not-an-email" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_Format_Passes()
    {
        var request = _fixture.Create<LoginRequest>() with { Email = "user@example.com" };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Password_Required(string? password)
    {
        var request = _fixture.Create<LoginRequest>() with { Password = password! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Multiple_Errors_Aggregated()
    {
        var request = new LoginRequest { Email = "", Password = "" };
        var result = _sut.TestValidate(request);
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
