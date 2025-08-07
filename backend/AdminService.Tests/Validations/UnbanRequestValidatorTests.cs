using AdminService.Models.Requests;
using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class UnbanRequestValidatorTests
{
    [Fact]
    public void ValidateUnbanRequest_WhenValid()
    {
        var request = new UnbanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "User has shown remorse and understanding of community guidelines"
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateUnbanRequest_WhenValidWithoutReason()
    {
        var request = new UnbanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = string.Empty
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateUnbanRequest_WhenEmptyUserId(string userId)
    {
        var request = new UnbanUserRequest
        {
            UserId = userId,
            Reason = "Valid reason for unbanning"
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("UserId is required");
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-valid-guid-format")]
    [InlineData("abc-def-ghi")]
    public void ValidateUnbanRequest_WhenInvalidUserIdFormat(string userId)
    {
        var request = new UnbanUserRequest
        {
            UserId = userId,
            Reason = "Valid reason for unbanning"
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid UserId format");
    }

    [Fact]
    public void ValidateUnbanRequest_WhenReasonTooLong()
    {
        var longReason = new string('a', 501); // > 500 characters
        var request = new UnbanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = longReason
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Unban reason too long (max 500 characters)");
    }

    [Fact]
    public void ValidateUnbanRequest_WhenReasonExactlyMaxLength()
    {
        var maxLengthReason = new string('a', 500); // Exactly 500 characters
        var request = new UnbanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = maxLengthReason
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Valid reason'; DROP TABLE users; --")]
    [InlineData("Reason with <script>alert('xss')</script>")]
    [InlineData("1' OR '1'='1")]
    public void ValidateUnbanRequest_WhenReasonContainsSqlInjection(string maliciousReason)
    {
        var request = new UnbanUserRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = maliciousReason
        };

        var result = UnbanRequestValidator.ValidateUnbanRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid characters detected");
    }

    [Fact]
    public void ValidateUnbanRequest_WhenNullRequest()
    {
        var result = UnbanRequestValidator.ValidateUnbanRequest(null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("UserId is required");
    }

    [Fact]
    public void ValidateUnbanRequest_WhenValidGuidFormats()
    {
        var validGuids = new[]
        {
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString("D"),
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("B"),
            Guid.NewGuid().ToString("P")
        };

        foreach (var validGuid in validGuids)
        {
            var request = new UnbanUserRequest
            {
                UserId = validGuid,
                Reason = "Valid reason"
            };

            var result = UnbanRequestValidator.ValidateUnbanRequest(request);

            result.IsValid.Should().BeTrue($"GUID format {validGuid} should be valid");
            result.Errors.Should().BeEmpty();
        }
    }
}
