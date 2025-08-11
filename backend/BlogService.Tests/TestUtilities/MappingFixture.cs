using AutoMapper;

namespace BlogService.Tests.TestUtilities;

public sealed class MappingFixture
{
    public IMapper Mapper { get; }

    public MappingFixture()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<BlogService.Helpers.MappingProfile>();
        });
        Mapper = configuration.CreateMapper();
    }
}

