using AdminService.Models.Requests;
using AdminService.Validations;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Validations;

public class BlogManagementRequestValidatorTests
{
    [Fact]
    public void ValidateDeleteBlogPostRequest_WhenValid()
    {
        var request = new DeleteBlogPostRequest
        {
            Reason = "Inappropriate content violating community guidelines"
        };

        var result = BlogManagementRequestValidator.ValidateDeleteBlogPostRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateDeleteBlogPostRequest_WhenEmptyReason(string reason)
    {
        var request = new DeleteBlogPostRequest
        {
            Reason = reason
        };

        var result = BlogManagementRequestValidator.ValidateDeleteBlogPostRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reason", StringComparison.OrdinalIgnoreCase));
    }
    
    [Fact]
    public void ValidateDeleteBlogPostRequest_WhenReasonTooShort()
    {
        var request = new DeleteBlogPostRequest
        {
            Reason = "Bad" // Too short
        };

        var result = BlogManagementRequestValidator.ValidateDeleteBlogPostRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reason", StringComparison.OrdinalIgnoreCase));
    }
    
    [Fact]
    public void ValidateUpdateBlogPostStatusRequest_WhenValid()
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = Guid.NewGuid().ToString(),
            Status = 2, // Suspended  
            Reason = "Under review for policy compliance"
        };

        var result = BlogManagementRequestValidator.ValidateUpdateBlogPostStatusRequest(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void ValidateUpdateBlogPostStatusRequest_WhenInvalidStatus(int status)
    {
        var request = new UpdateBlogPostStatusRequest
        {
            BlogPostId = Guid.NewGuid().ToString(),
            Status = status,
            Reason = "Valid reason"
        };

        var result = BlogManagementRequestValidator.ValidateUpdateBlogPostStatusRequest(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("status", StringComparison.OrdinalIgnoreCase));
    }
}