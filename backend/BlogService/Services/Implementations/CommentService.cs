using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;
using BlogService.Repositories.Interfaces;
using BlogService.Services.Interfaces;

namespace BlogService.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;
    private readonly IUserServiceClient _userServiceClient;

    public CommentService(
        ICommentRepository commentRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper,
        IUserServiceClient userServiceClient)
    {
        _commentRepository = commentRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
        _userServiceClient = userServiceClient;
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null) return null;
        
        var commentDto = _mapper.Map<CommentDto>(comment);
        await PopulateAuthorInfoAsync(commentDto);
        return commentDto;
    }

    public async Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId)
    {
        var comments = await _commentRepository.GetByBlogPostIdAsync(blogPostId);
        var commentDtos = _mapper.Map<IEnumerable<CommentDto>>(comments);
        
        // Populate author information for all comments and replies
        foreach (var commentDto in commentDtos)
        {
            await PopulateAuthorInfoRecursivelyAsync(commentDto);
        }
        
        return commentDtos;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto createDto)
    {
        // Verify blog post exists
        var blogPostExists = await _blogPostRepository.ExistsAsync(createDto.BlogPostId);
        if (!blogPostExists)
        {
            throw new ArgumentException($"Blog post with ID {createDto.BlogPostId} not found.");
        }

        // If this is a reply, verify the parent comment exists
        if (createDto.ParentCommentId.HasValue)
        {
            var parentCommentExists = await _commentRepository.ExistsAsync(createDto.ParentCommentId.Value);
            if (!parentCommentExists)
            {
                throw new ArgumentException($"Parent comment with ID {createDto.ParentCommentId} not found.");
            }
        }

        var comment = _mapper.Map<Comment>(createDto);
        var createdComment = await _commentRepository.CreateAsync(comment);
        var commentDto = _mapper.Map<CommentDto>(createdComment);
        
        // No need to populate author info from UserService since it's already set in the controller
        return commentDto;
    }

    public async Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto)
    {
        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Only allow updating content - preserve author information
        existingComment.Content = updateDto.Content;
        
        var updatedComment = await _commentRepository.UpdateAsync(existingComment);
        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        await PopulateAuthorInfoAsync(commentDto);
        return commentDto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetCommentByIdOrThrowAsync(id);
        await _commentRepository.DeleteAsync(id);
    }

    private async Task<Comment> GetCommentByIdOrThrowAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            throw new ArgumentException($"Comment with ID {id} not found.");
        }
        return comment;
    }
    
    private async Task PopulateAuthorInfoAsync(CommentDto commentDto)
    {
        if (string.IsNullOrEmpty(commentDto.AuthorId))
        {
            commentDto.AuthorName = "Anonymous";
            commentDto.AuthorImage = string.Empty;
            return;
        }
        
        try
        {
            var user = await _userServiceClient.GetUserByIdAsync(commentDto.AuthorId);
            if (user != null)
            {
                commentDto.AuthorName = user.Name;
                commentDto.AuthorImage = user.Image ?? string.Empty;
            }
            else
            {
                commentDto.AuthorName = "Unknown User";
                commentDto.AuthorImage = string.Empty;
            }
        }
        catch (Exception)
        {
            // If we can't get user info, keep the existing AuthorName or set a default
            if (string.IsNullOrEmpty(commentDto.AuthorName))
            {
                commentDto.AuthorName = "Unknown User";
            }
            commentDto.AuthorImage = string.Empty;
        }
    }
    
    private async Task PopulateAuthorInfoRecursivelyAsync(CommentDto commentDto)
    {
        // Populate author info for this comment
        await PopulateAuthorInfoAsync(commentDto);
        
        // Recursively populate author info for all replies
        foreach (var reply in commentDto.Replies)
        {
            await PopulateAuthorInfoRecursivelyAsync(reply);
        }
    }
} 