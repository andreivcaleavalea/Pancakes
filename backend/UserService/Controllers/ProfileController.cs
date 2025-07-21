using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UserService.Data;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize] // Temporarily disabled for testing
    public class ProfileController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly CurrentUserService _currentUserService;

        public ProfileController(UserDbContext context, IMapper mapper, CurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        // Get complete profile data
        [HttpGet]
        public async Task<ActionResult<ProfileDataDto>> GetProfile()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Educations)
                .Include(u => u.Jobs)
                .Include(u => u.Hobbies)
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                return NotFound();
            }

            var profileData = new ProfileDataDto
            {
                User = _mapper.Map<UserProfileDto>(user),
                Educations = _mapper.Map<List<EducationDto>>(user.Educations),
                Jobs = _mapper.Map<List<JobDto>>(user.Jobs),
                Hobbies = _mapper.Map<List<HobbyDto>>(user.Hobbies),
                Projects = _mapper.Map<List<ProjectDto>>(user.Projects)
            };

            return Ok(profileData);
        }

        // Update user profile
        [HttpPut("user")]
        public async Task<ActionResult<UserProfileDto>> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
            {
                return NotFound();
            }

            // Update user fields (email is never updated for security)
            user.Name = updateUserProfileDto.Name;
            user.Bio = updateUserProfileDto.Bio;
            user.PhoneNumber = updateUserProfileDto.PhoneNumber;
            if (!string.IsNullOrEmpty(updateUserProfileDto.DateOfBirth))
            {
                if (DateTime.TryParse(updateUserProfileDto.DateOfBirth, out DateTime parsedDate))
                {
                    // Convert to UTC for PostgreSQL compatibility
                    user.DateOfBirth = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }
            }
            else
            {
                user.DateOfBirth = null; 
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<UserProfileDto>(user));
        }

        // Education endpoints
        [HttpGet("educations")]
        public async Task<ActionResult<List<EducationDto>>> GetEducations()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var educations = await _context.Educations
                .Where(e => e.UserId == currentUserId)
                .ToListAsync();

            return Ok(_mapper.Map<List<EducationDto>>(educations));
        }

        [HttpPost("educations")]
        public async Task<ActionResult<EducationDto>> AddEducation([FromBody] EducationDto educationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var education = _mapper.Map<Education>(educationDto);
            education.UserId = currentUserId;
            education.Id = Guid.NewGuid().ToString();

            _context.Educations.Add(education);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEducations), _mapper.Map<EducationDto>(education));
        }

        [HttpPut("educations/{id}")]
        public async Task<ActionResult<EducationDto>> UpdateEducation(string id, [FromBody] EducationDto educationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == currentUserId);

            if (education == null)
            {
                return NotFound();
            }

            _mapper.Map(educationDto, education);
            education.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<EducationDto>(education));
        }

        [HttpDelete("educations/{id}")]
        public async Task<IActionResult> DeleteEducation(string id)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == currentUserId);

            if (education == null)
            {
                return NotFound();
            }

            _context.Educations.Remove(education);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Job endpoints
        [HttpGet("jobs")]
        public async Task<ActionResult<List<JobDto>>> GetJobs()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var jobs = await _context.Jobs
                .Where(j => j.UserId == currentUserId)
                .ToListAsync();

            return Ok(_mapper.Map<List<JobDto>>(jobs));
        }

        [HttpPost("jobs")]
        public async Task<ActionResult<JobDto>> AddJob([FromBody] JobDto jobDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var job = _mapper.Map<Job>(jobDto);
            job.UserId = currentUserId;
            job.Id = Guid.NewGuid().ToString();

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobs), _mapper.Map<JobDto>(job));
        }

        [HttpPut("jobs/{id}")]
        public async Task<ActionResult<JobDto>> UpdateJob(string id, [FromBody] JobDto jobDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == currentUserId);

            if (job == null)
            {
                return NotFound();
            }

            _mapper.Map(jobDto, job);
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<JobDto>(job));
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(string id)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == currentUserId);

            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hobby endpoints
        [HttpGet("hobbies")]
        public async Task<ActionResult<List<HobbyDto>>> GetHobbies()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var hobbies = await _context.Hobbies
                .Where(h => h.UserId == currentUserId)
                .ToListAsync();

            return Ok(_mapper.Map<List<HobbyDto>>(hobbies));
        }

        [HttpPost("hobbies")]
        public async Task<ActionResult<HobbyDto>> AddHobby([FromBody] HobbyDto hobbyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var hobby = _mapper.Map<Hobby>(hobbyDto);
            hobby.UserId = currentUserId;
            hobby.Id = Guid.NewGuid().ToString();

            _context.Hobbies.Add(hobby);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHobbies), _mapper.Map<HobbyDto>(hobby));
        }

        [HttpPut("hobbies/{id}")]
        public async Task<ActionResult<HobbyDto>> UpdateHobby(string id, [FromBody] HobbyDto hobbyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var hobby = await _context.Hobbies
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);

            if (hobby == null)
            {
                return NotFound();
            }

            _mapper.Map(hobbyDto, hobby);
            hobby.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<HobbyDto>(hobby));
        }

        [HttpDelete("hobbies/{id}")]
        public async Task<IActionResult> DeleteHobby(string id)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var hobby = await _context.Hobbies
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == currentUserId);

            if (hobby == null)
            {
                return NotFound();
            }

            _context.Hobbies.Remove(hobby);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Project endpoints
        [HttpGet("projects")]
        public async Task<ActionResult<List<ProjectDto>>> GetProjects()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var projects = await _context.Projects
                .Where(p => p.UserId == currentUserId)
                .ToListAsync();

            return Ok(_mapper.Map<List<ProjectDto>>(projects));
        }

        [HttpPost("projects")]
        public async Task<ActionResult<ProjectDto>> AddProject([FromBody] ProjectDto projectDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var project = _mapper.Map<Project>(projectDto);
            project.UserId = currentUserId;
            project.Id = Guid.NewGuid().ToString();

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), _mapper.Map<ProjectDto>(project));
        }

        [HttpPut("projects/{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(string id, [FromBody] ProjectDto projectDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (project == null)
            {
                return NotFound();
            }

            _mapper.Map(projectDto, project);
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ProjectDto>(project));
        }

        [HttpDelete("projects/{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
} 