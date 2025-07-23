import { authenticatedBlogRequest } from "@/utils/blogApi";
import type {
  BlogPost,
  PaginatedResult,
  BlogPostQueryParams,
  CreateBlogPostDto,
  UpdateBlogPostDto,
} from "@/types/blog";

const ENDPOINTS = {
  BLOG_POSTS: "/api/blogposts",
} as const;

// Blog Posts API
export const blogPostsApi = {
  // Get paginated blog posts with filtering
  getAll: async (
    params: BlogPostQueryParams = {}
  ): Promise<PaginatedResult<BlogPost>> => {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        searchParams.append(key, value.toString());
      }
    });

    const queryString = searchParams.toString();
    const endpoint = queryString
      ? `${ENDPOINTS.BLOG_POSTS}?${queryString}`
      : ENDPOINTS.BLOG_POSTS;

    return authenticatedBlogRequest<PaginatedResult<BlogPost>>(endpoint);
  },

  // Get blog post by ID
  getById: async (id: string): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(`${ENDPOINTS.BLOG_POSTS}/${id}`);
  },

  // Get blog post by slug
  getBySlug: async (slug: string): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(
      `${ENDPOINTS.BLOG_POSTS}/slug/${slug}`
    );
  },

  // Get featured posts
  getFeatured: async (count: number = 5): Promise<BlogPost[]> => {
    return authenticatedBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/featured?count=${count}`
    );
  },

  // Get popular posts
  getPopular: async (count: number = 5): Promise<BlogPost[]> => {
    return authenticatedBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/popular?count=${count}`
    );
  },

  // Get posts by author
  getByAuthor: async (
    authorId: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<BlogPost[]> => {
    return authenticatedBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/author/${authorId}?page=${page}&pageSize=${pageSize}`
    );
  },

  // Create new blog post
  create: async (blogPost: CreateBlogPostDto): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(ENDPOINTS.BLOG_POSTS, {
      method: "POST",
      body: JSON.stringify(blogPost),
    });
  },

  // Update blog post
  update: async (
    id: string,
    blogPost: UpdateBlogPostDto
  ): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(`${ENDPOINTS.BLOG_POSTS}/${id}`, {
      method: "PUT",
      body: JSON.stringify(blogPost),
    });
  },

  // Delete blog post
  delete: async (id: string): Promise<void> => {
    return authenticatedBlogRequest<void>(`${ENDPOINTS.BLOG_POSTS}/${id}`, {
      method: "DELETE",
    });
  },

  // Increment view count
  incrementViewCount: async (id: string): Promise<void> => {
    return authenticatedBlogRequest<void>(
      `${ENDPOINTS.BLOG_POSTS}/${id}/view`,
      {
        method: "POST",
      }
    );
  },

  // Toggle featured status
  toggleFeatured: async (id: string): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(
      `${ENDPOINTS.BLOG_POSTS}/${id}/featured`,
      {
        method: "PATCH",
      }
    );
  },
};
