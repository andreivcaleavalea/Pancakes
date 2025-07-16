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
        CreateMap<UpdateBlogPostDto, BlogPost>();
    }
}
