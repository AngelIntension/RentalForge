using AutoFixture;
using FluentAssertions;
using FluentValidation.TestHelper;
using RentalForge.Api.Models.Auth;
using RentalForge.Api.Validators;

namespace RentalForge.Api.Tests.Unit;

public class RefreshRequestValidatorTests
{
    private readonly RefreshRequestValidator _sut = new();
    private readonly Fixture _fixture = new();

    public RefreshRequestValidatorTests()
    {
        _fixture.Customize<RefreshRequest>(c => c
            .With(x => x.RefreshToken, "valid-refresh-token"));
    }

    [Fact]
    public void Valid_Request_Passes_Validation()
    {
        var request = _fixture.Create<RefreshRequest>();
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RefreshToken_Required(string? refreshToken)
    {
        var request = _fixture.Create<RefreshRequest>() with { RefreshToken = refreshToken! };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
