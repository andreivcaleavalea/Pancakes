using AutoMapper;
using BlogService.Models.DTOs;
using BlogService.Models.Entities;

namespace BlogService.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BlogPost, BlogPostDto>();
        CreateMap<CreateBlogPostDto, BlogPost>();
        CreateMap<UpdateBlogPostDto, BlogPost>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<Comment, CommentDto>();
        CreateMap<CreateCommentDto, Comment>();
        
        CreateMap<PostRating, PostRatingDto>();
        CreateMap<CreatePostRatingDto, PostRating>();
        
        CreateMap<CommentLike, CommentLikeDto>();
        CreateMap<CreateCommentLikeDto, CommentLike>();
        
        CreateMap<SavedBlog, SavedBlogDto>();
        CreateMap<CreateSavedBlogDto, SavedBlog>();
        
        CreateMap<Friendship, FriendshipDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
