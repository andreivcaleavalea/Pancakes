using AutoMapper;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implementations;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public JobService(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JobDto>> GetUserJobsAsync(string userId)
    {
        await EnsureUserExistsAsync(userId);
        var jobs = await _jobRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<JobDto> CreateAsync(string userId, JobDto jobDto)
    {
        await EnsureUserExistsAsync(userId);
        
        var job = _mapper.Map<Job>(jobDto);
        job.UserId = userId;
        
        var createdJob = await _jobRepository.CreateAsync(job);
        return _mapper.Map<JobDto>(createdJob);
    }

    public async Task<JobDto> UpdateAsync(string userId, string jobId, JobDto jobDto)
    {
        await EnsureUserExistsAsync(userId);
        var existingJob = await GetJobByIdOrThrowAsync(jobId, userId);
        
        _mapper.Map(jobDto, existingJob);
        var updatedJob = await _jobRepository.UpdateAsync(existingJob);
        return _mapper.Map<JobDto>(updatedJob);
    }

    public async Task DeleteAsync(string userId, string jobId)
    {
        await EnsureUserExistsAsync(userId);
        await GetJobByIdOrThrowAsync(jobId, userId);
        await _jobRepository.DeleteAsync(jobId);
    }

    private async Task<Job> GetJobByIdOrThrowAsync(string jobId, string userId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null || job.UserId != userId)
        {
            throw new ArgumentException($"Job with ID {jobId} not found for user {userId}.");
        }
        return job;
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        if (!await _userRepository.ExistsAsync(userId))
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
    }
} 