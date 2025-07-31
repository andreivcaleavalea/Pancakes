namespace AdminService.Models.DTOs
{
    public class UserOverviewDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public int TotalBlogPosts { get; set; }
        public int TotalComments { get; set; }
        public int ReportsCount { get; set; }
        public bool IsBanned { get; set; }
    }

    public class UserDetailDto : UserOverviewDto
    {
        public string Bio { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Image { get; set; } = string.Empty;
        public List<UserEducationDto> Education { get; set; } = new List<UserEducationDto>();
        public List<UserJobDto> Jobs { get; set; } = new List<UserJobDto>();
        public List<UserProjectDto> Projects { get; set; } = new List<UserProjectDto>();
        public List<UserHobbyDto> Hobbies { get; set; } = new List<UserHobbyDto>();
    }

    public class UserEducationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
    }

    public class UserJobDto
    {
        public string Id { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UserProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Technologies { get; set; } = string.Empty;
        public string ProjectUrl { get; set; } = string.Empty;
        public string GithubUrl { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
    }

    public class UserHobbyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SkillLevel { get; set; }
    }
}