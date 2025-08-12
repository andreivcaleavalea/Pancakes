using AdminService.Models.Requests;
using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class BanRequestValidatorTests
{
    [Fact]
    public void ValidateBanRequest_WhenValid()
    {
        var request = new BanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "Inappropriate behavior and harassment",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var result = BanRequestValidator.ValidateBanRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateBanRequest_WhenEmptyReason(string reason)
    {
        var request = new BanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = reason,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var result = BanRequestValidator.ValidateBanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reason", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateBanRequest_WhenInvalidExpiryDate()
    {
        var request = new BanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "Valid reason",
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Past date
        };

        var result = BanRequestValidator.ValidateBanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("future", StringComparison.OrdinalIgnoreCase));
    }
}