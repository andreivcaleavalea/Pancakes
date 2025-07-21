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

    public CommentService(
        ICommentRepository commentRepository,
        IBlogPostRepository blogPostRepository,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        return comment != null ? _mapper.Map<CommentDto>(comment) : null;
    }

    public async Task<IEnumerable<CommentDto>> GetByBlogPostIdAsync(Guid blogPostId)
    {
        var comments = await _commentRepository.GetByBlogPostIdAsync(blogPostId);
        return _mapper.Map<IEnumerable<CommentDto>>(comments);
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
        return _mapper.Map<CommentDto>(createdComment);
    }

    public async Task<CommentDto> UpdateAsync(Guid id, CreateCommentDto updateDto)
    {
        var existingComment = await GetCommentByIdOrThrowAsync(id);
        
        // Only allow updating content and author name
        existingComment.Content = updateDto.Content;
        existingComment.AuthorName = updateDto.AuthorName;
        
        var updatedComment = await _commentRepository.UpdateAsync(existingComment);
        return _mapper.Map<CommentDto>(updatedComment);
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
} 