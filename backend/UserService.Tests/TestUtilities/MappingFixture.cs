using AutoMapper;
using UserService.Helpers;

namespace UserService.Tests.TestUtilities;

public class MappingFixture
{
    public IMapper Mapper { get; }

    public MappingFixture()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        Mapper = new Mapper(config);
    }
}


