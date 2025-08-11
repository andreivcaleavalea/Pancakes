using BlogService.Controllers;
using BlogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Tests.Controllers;

public class TagsControllerTests
{
    private static TagsController CreateController(out Mock<ITagService> svc)
    {
        svc = new Mock<ITagService>(MockBehavior.Strict);
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TagsController>>();
        return new TagsController(svc.Object, logger.Object);
    }

    [Fact]
    public async Task GetPopularTags_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        svc.Setup(s => s.GetPopularTagsAsync(5)).ReturnsAsync(new List<string> { "tag" });

        var action = await controller.GetPopularTags(5) as OkObjectResult;
        action.Should().NotBeNull();
        (action!.Value as IEnumerable<string>)!.Should().Contain("tag");
    }

    [Fact]
    public async Task SearchTags_Returns_Ok()
    {
        var controller = CreateController(out var svc);
        svc.Setup(s => s.SearchTagsAsync("q", 10)).ReturnsAsync(new List<string>());

        var action = await controller.SearchTags("q", 10) as OkObjectResult;
        action.Should().NotBeNull();
    }
}



