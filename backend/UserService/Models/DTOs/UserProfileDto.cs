namespace UserService.Models.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
    }
    
    public class EducationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    
    public class JobDto
    {
        public string Id { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    
    public class HobbyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
    
    public class ProjectDto
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
    
    public class ProfileDataDto
    {
        public UserProfileDto User { get; set; } = new();
        public List<EducationDto> Educations { get; set; } = new();
        public List<JobDto> Jobs { get; set; } = new();
        public List<HobbyDto> Hobbies { get; set; } = new();
        public List<ProjectDto> Projects { get; set; } = new();
    }
} 