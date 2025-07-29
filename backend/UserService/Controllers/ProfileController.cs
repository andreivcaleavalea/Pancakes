using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models.DTOs;
using UserService.Services.Interfaces;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IEducationService _educationService;
    private readonly IJobService _jobService;
    private readonly IHobbyService _hobbyService;
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        IEducationService educationService,
        IJobService jobService,
        IHobbyService hobbyService,
        IProjectService projectService,
        ICurrentUserService currentUserService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _educationService = educationService;
        _jobService = jobService;
        _hobbyService = hobbyService;
        _projectService = projectService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // Profile endpoints
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var profileData = await _profileService.GetProfileDataAsync(currentUserId);
            if (profileData == null)
            {
                return NotFound("Profile not found");
            }

            return Ok(profileData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user");
            return StatusCode(500, "An error occurred while getting the profile");
        }
    }

    [HttpPut("user")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedProfile = await _profileService.UpdateUserProfileAsync(currentUserId, updateDto);
            return Ok(updatedProfile);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating user profile");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "An error occurred while updating the profile");
        }
    }

    [HttpPost("user/profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
    {
        try
        {
            if (profilePicture == null)
            {
                return BadRequest("No file provided");
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedProfile = await _profileService.UpdateProfilePictureAsync(currentUserId, profilePicture);
            return Ok(updatedProfile);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file provided for profile picture upload");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile picture");
            return StatusCode(500, "An error occurred while updating the profile picture");
        }
    }

    // Education endpoints
    [HttpGet("educations")]
    public async Task<IActionResult> GetEducations()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var educations = await _educationService.GetUserEducationsAsync(currentUserId);
            return Ok(educations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting educations for user");
            return StatusCode(500, "An error occurred while getting educations");
        }
    }

    [HttpPost("educations")]
    public async Task<IActionResult> AddEducation([FromBody] EducationDto educationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var createdEducation = await _educationService.CreateAsync(currentUserId, educationDto);
            return CreatedAtAction(nameof(GetEducations), createdEducation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating education");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating education");
            return StatusCode(500, "An error occurred while creating the education");
        }
    }

    [HttpPut("educations/{id}")]
    public async Task<IActionResult> UpdateEducation(string id, [FromBody] EducationDto educationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedEducation = await _educationService.UpdateAsync(currentUserId, id, educationDto);
            return Ok(updatedEducation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating education with ID {EducationId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating education with ID {EducationId}", id);
            return StatusCode(500, "An error occurred while updating the education");
        }
    }

    [HttpDelete("educations/{id}")]
    public async Task<IActionResult> DeleteEducation(string id)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            await _educationService.DeleteAsync(currentUserId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting education with ID {EducationId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting education with ID {EducationId}", id);
            return StatusCode(500, "An error occurred while deleting the education");
        }
    }

    // Job endpoints
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var jobs = await _jobService.GetUserJobsAsync(currentUserId);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs for user");
            return StatusCode(500, "An error occurred while getting jobs");
        }
    }

    [HttpPost("jobs")]
    public async Task<IActionResult> AddJob([FromBody] JobDto jobDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var createdJob = await _jobService.CreateAsync(currentUserId, jobDto);
            return CreatedAtAction(nameof(GetJobs), createdJob);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating job");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, "An error occurred while creating the job");
        }
    }

    [HttpPut("jobs/{id}")]
    public async Task<IActionResult> UpdateJob(string id, [FromBody] JobDto jobDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedJob = await _jobService.UpdateAsync(currentUserId, id, jobDto);
            return Ok(updatedJob);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating job with ID {JobId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job with ID {JobId}", id);
            return StatusCode(500, "An error occurred while updating the job");
        }
    }

    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(string id)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            await _jobService.DeleteAsync(currentUserId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting job with ID {JobId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job with ID {JobId}", id);
            return StatusCode(500, "An error occurred while deleting the job");
        }
    }

    // Hobby endpoints
    [HttpGet("hobbies")]
    public async Task<IActionResult> GetHobbies()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var hobbies = await _hobbyService.GetUserHobbiesAsync(currentUserId);
            return Ok(hobbies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hobbies for user");
            return StatusCode(500, "An error occurred while getting hobbies");
        }
    }

    [HttpPost("hobbies")]
    public async Task<IActionResult> AddHobby([FromBody] HobbyDto hobbyDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var createdHobby = await _hobbyService.CreateAsync(currentUserId, hobbyDto);
            return CreatedAtAction(nameof(GetHobbies), createdHobby);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating hobby");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hobby");
            return StatusCode(500, "An error occurred while creating the hobby");
        }
    }

    [HttpPut("hobbies/{id}")]
    public async Task<IActionResult> UpdateHobby(string id, [FromBody] HobbyDto hobbyDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedHobby = await _hobbyService.UpdateAsync(currentUserId, id, hobbyDto);
            return Ok(updatedHobby);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating hobby with ID {HobbyId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hobby with ID {HobbyId}", id);
            return StatusCode(500, "An error occurred while updating the hobby");
        }
    }

    [HttpDelete("hobbies/{id}")]
    public async Task<IActionResult> DeleteHobby(string id)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            await _hobbyService.DeleteAsync(currentUserId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting hobby with ID {HobbyId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hobby with ID {HobbyId}", id);
            return StatusCode(500, "An error occurred while deleting the hobby");
        }
    }

    // Project endpoints
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var projects = await _projectService.GetUserProjectsAsync(currentUserId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for user");
            return StatusCode(500, "An error occurred while getting projects");
        }
    }

    [HttpPost("projects")]
    public async Task<IActionResult> AddProject([FromBody] ProjectDto projectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var createdProject = await _projectService.CreateAsync(currentUserId, projectDto);
            return CreatedAtAction(nameof(GetProjects), createdProject);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating project");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, "An error occurred while creating the project");
        }
    }

    [HttpPut("projects/{id}")]
    public async Task<IActionResult> UpdateProject(string id, [FromBody] ProjectDto projectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedProject = await _projectService.UpdateAsync(currentUserId, id, projectDto);
            return Ok(updatedProject);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating project with ID {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project with ID {ProjectId}", id);
            return StatusCode(500, "An error occurred while updating the project");
        }
    }

    [HttpDelete("projects/{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            await _projectService.DeleteAsync(currentUserId, id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for deleting project with ID {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project with ID {ProjectId}", id);
            return StatusCode(500, "An error occurred while deleting the project");
        }
    }
} 