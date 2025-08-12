using BlogService.Helpers;
using BlogService.Services.Implementations;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace BlogService.Tests.Helpers;

public class HelpersTests
{
    [Fact]
    public void PaginationHelper_Computes_Pagination_Correctly()
    {
        var data = Enumerable.Range(1, 7).ToList();
        var page = 2;
        var pageSize = 3;
        var total = 10;

        var result = PaginationHelper.CreatePaginatedResult(data, page, pageSize, total);

        result.Pagination.CurrentPage.Should().Be(2);
        result.Pagination.PageSize.Should().Be(3);
        result.Pagination.TotalItems.Should().Be(10);
        result.Pagination.TotalPages.Should().Be(4); // ceil(10/3)
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.HasNextPage.Should().BeTrue(); // page 2 of 4
        result.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void ModelValidationService_Valid_Then_Invalid_Aggregates_Errors()
    {
        var logger = new Mock<ILogger<ModelValidationService>>();
        var svc = new ModelValidationService(logger.Object);

        var validState = new ModelStateDictionary();
        validState.AddModelError("irrelevant", string.Empty);
        validState.Clear(); // ensure IsValid
        var valid = svc.ValidateModel(validState);
        valid.IsValid.Should().BeTrue();

        var invalid = new ModelStateDictionary();
        invalid.AddModelError("Title", "Required");
        invalid.AddModelError("Content", "Too short");

        var inval = svc.ValidateModel(invalid);
        inval.IsValid.Should().BeFalse();
        inval.ErrorMessage.Should().Contain("Title").And.Contain("Content");
    }
}


