using UserService.Helpers;
using UserService.Models.Entities;
using UserService.Models.DTOs;

namespace UserService.Tests.Mapping;

public class PersonalPageSettingsMappingTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return new Mapper(cfg);
    }

    [Fact]
    public void Maps_With_Valid_Json()
    {
        var mapper = CreateMapper();
        var settings = new PersonalPageSettings
        {
            UserId = "u1",
            PageSlug = "slug",
            SectionOrder = "[\"personal\",\"jobs\"]",
            SectionVisibility = "{\"personal\":true,\"jobs\":false}",
            SectionTemplates = "{\"personal\":\"card\"}",
            SectionColors = "{\"personal\":\"blue\"}",
            SectionAdvancedSettings = "{}"
        };
        var dto = mapper.Map<PersonalPageSettingsDto>(settings);
        dto.SectionOrder.Should().Contain(new[] { "personal", "jobs" });
        dto.SectionVisibility["personal"].Should().BeTrue();
        dto.SectionTemplates["personal"].Should().Be("card");
        dto.SectionColors["personal"].Should().Be("blue");
    }

    [Fact]
    public void Maps_With_Invalid_Json_Uses_Defaults()
    {
        var mapper = CreateMapper();
        var settings = new PersonalPageSettings
        {
            UserId = "u1",
            PageSlug = "slug",
            SectionOrder = "invalid",
            SectionVisibility = "invalid",
            SectionTemplates = "invalid",
            SectionColors = "invalid",
            SectionAdvancedSettings = "invalid"
        };
        var dto = mapper.Map<PersonalPageSettingsDto>(settings);
        dto.SectionOrder.Should().NotBeNull();
        dto.SectionVisibility.Should().NotBeNull();
        dto.SectionTemplates.Should().NotBeNull();
        dto.SectionColors.Should().NotBeNull();
    }
}


