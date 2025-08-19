using AutoMapper;
using UserService.Helpers;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Implementations;

namespace UserService.Tests.Services;

public class PersonalPageServiceTests
{
	private static PersonalPageService Create(
		out Mock<IPersonalPageSettingsRepository> settings,
		out Mock<IUserRepository> users,
		out Mock<IEducationRepository> edu,
		out Mock<IJobRepository> jobs,
		out Mock<IHobbyRepository> hobbies,
		out Mock<IProjectRepository> projects)
	{
		settings = new Mock<IPersonalPageSettingsRepository>(MockBehavior.Strict);
		users = new Mock<IUserRepository>(MockBehavior.Strict);
		edu = new Mock<IEducationRepository>(MockBehavior.Strict);
		jobs = new Mock<IJobRepository>(MockBehavior.Strict);
		hobbies = new Mock<IHobbyRepository>(MockBehavior.Strict);
		projects = new Mock<IProjectRepository>(MockBehavior.Strict);
		var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<MappingProfile>()));
		return new PersonalPageService(settings.Object, users.Object, edu.Object, jobs.Object, hobbies.Object, projects.Object, mapper);
	}

	[Fact]
	public async Task GetSettings_Creates_Default_When_Missing()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		settings.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((PersonalPageSettings?)null);
		users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1", Name = "John Doe" });
		settings.Setup(r => r.PageSlugExistsAsync(It.IsAny<string>(), "u1")).ReturnsAsync(false);
		settings.Setup(r => r.CreateAsync(It.IsAny<PersonalPageSettings>())).ReturnsAsync((PersonalPageSettings s) => s);

		var dto = await svc.GetSettingsAsync("u1");
		dto.UserId.Should().Be("u1");
		dto.PageSlug.Should().Be("john-doe");
	}

	[Fact]
	public async Task UpdateSettings_Throws_When_Not_Found()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		settings.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync((PersonalPageSettings?)null);
		await FluentActions.Invoking(() => svc.UpdateSettingsAsync("u1", new UpdatePersonalPageSettingsDto()))
			.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task UpdateSettings_Slug_Taken_Throws()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		var entity = new PersonalPageSettings { UserId = "u1", PageSlug = "old" };
		settings.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(entity);
		settings.Setup(r => r.PageSlugExistsAsync("new", "u1")).ReturnsAsync(true);
		await FluentActions.Invoking(() => svc.UpdateSettingsAsync("u1", new UpdatePersonalPageSettingsDto { PageSlug = "new" }))
			.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task UpdateSettings_Updates_Fields_And_Maps_Back()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		var entity = new PersonalPageSettings { UserId = "u1", PageSlug = "old" };
		settings.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(entity);
		settings.Setup(r => r.PageSlugExistsAsync("new-slug", "u1")).ReturnsAsync(false);
		settings.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);

		var update = new UpdatePersonalPageSettingsDto
		{
			IsPublic = true,
			PageSlug = "new-slug",
			SectionOrder = new List<string> { "personal", "projects" },
			SectionVisibility = new Dictionary<string, bool> { ["personal"] = true, ["projects"] = false },
			SectionTemplates = new Dictionary<string, string> { ["personal"] = "card" },
			SectionColors = new Dictionary<string, string> { ["personal"] = "teal" },
			SectionAdvancedSettings = new Dictionary<string, AdvancedSectionSettings> { ["personal"] = new AdvancedSectionSettings() },
			Theme = "neo",
			ColorScheme = "green"
		};

		var dto = await svc.UpdateSettingsAsync("u1", update);
		dto.PageSlug.Should().Be("new-slug");
		dto.IsPublic.Should().BeTrue();
		dto.SectionOrder.Should().Equal("personal", "projects");
		dto.SectionVisibility.Should().ContainKey("projects").WhoseValue.Should().BeFalse();
		dto.SectionTemplates["personal"].Should().Be("card");
		dto.SectionColors["personal"].Should().Be("teal");
		dto.Theme.Should().Be("neo");
		dto.ColorScheme.Should().Be("green");
	}

	[Fact]
	public async Task GetPublicPage_Returns_Null_When_User_Missing()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		settings.Setup(r => r.GetByPageSlugAsync("slug")).ReturnsAsync(new PersonalPageSettings { UserId = "u1", User = null! });
		var res = await svc.GetPublicPageAsync("slug");
		res.Should().BeNull();
	}

	[Fact]
	public async Task GetPublicPage_Returns_Composed_Dto()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		var entity = new PersonalPageSettings { UserId = "u1", User = new User { Id = "u1", Name = "A" } };
		settings.Setup(r => r.GetByPageSlugAsync("slug")).ReturnsAsync(entity);
		edu.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Education> { new Education { Id = "e1", UserId = "u1" } });
		jobs.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Job> { new Job { Id = "j1", UserId = "u1" } });
		hobbies.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Hobby> { new Hobby { Id = "h1", UserId = "u1" } });
		projects.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(new List<Project> { new Project { Id = "p1", UserId = "u1" } });

		var page = await svc.GetPublicPageAsync("slug");
		page.Should().NotBeNull();
		page!.Educations.Should().HaveCount(1);
		page.Jobs.Should().HaveCount(1);
		page.Hobbies.Should().HaveCount(1);
		page.Projects.Should().HaveCount(1);
		page.User.Id.Should().Be("u1");
	}

	[Fact]
	public async Task GenerateUniqueSlug_Appends_Counter_When_Taken()
	{
		var svc = Create(out var settings, out var users, out var edu, out var jobs, out var hobbies, out var projects);
		users.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(new User { Id = "u1", Name = "John Doe" });
		settings.Setup(r => r.PageSlugExistsAsync(It.IsAny<string>(), "u1"))
			.ReturnsAsync((string slug, string userId) => slug == "john-doe" || slug == "john-doe-1");

		var slug = await svc.GenerateUniqueSlugAsync("John Doe", "u1");
		slug.Should().Be("john-doe-2");
	}
}


