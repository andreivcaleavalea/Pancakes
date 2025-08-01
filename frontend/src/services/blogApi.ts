import { authenticatedBlogRequest, publicBlogRequest } from "@/utils/blogApi";
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
  // Get paginated blog posts with filtering (PUBLIC - no auth required)
  getAll: async (
    params: BlogPostQueryParams = {}
  ): Promise<PaginatedResult<BlogPost>> => {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        // Handle arrays (like tags) by sending multiple parameters with the same name
        if (Array.isArray(value)) {
          value.forEach((item) => {
            searchParams.append(key, item.toString());
          });
        } else {
          searchParams.append(key, value.toString());
        }
      }
    });

    const queryString = searchParams.toString();
    const endpoint = queryString
      ? `${ENDPOINTS.BLOG_POSTS}?${queryString}`
      : ENDPOINTS.BLOG_POSTS;

    return publicBlogRequest<PaginatedResult<BlogPost>>(endpoint);
  },

  // Get blog post by ID (PUBLIC - no auth required)
  getById: async (id: string): Promise<BlogPost> => {
    return publicBlogRequest<BlogPost>(`${ENDPOINTS.BLOG_POSTS}/${id}`);
  },

  // Get blog post by slug (PUBLIC - no auth required)
  getBySlug: async (slug: string): Promise<BlogPost> => {
    return publicBlogRequest<BlogPost>(`${ENDPOINTS.BLOG_POSTS}/slug/${slug}`);
  },

  // Get featured posts (PUBLIC - no auth required)
  getFeatured: async (count: number = 5): Promise<BlogPost[]> => {
    return publicBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/featured?count=${count}`
    );
  },

  // Get popular posts (PUBLIC - no auth required)
  getPopular: async (count: number = 5): Promise<BlogPost[]> => {
    return publicBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/popular?count=${count}`
    );
  },

  // Get posts by author (PUBLIC - no auth required)
  getByAuthor: async (
    authorId: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<BlogPost[]> => {
    return publicBlogRequest<BlogPost[]>(
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

  // Increment view count (PUBLIC - no auth required)
  incrementViewCount: async (id: string): Promise<void> => {
    return publicBlogRequest<void>(`${ENDPOINTS.BLOG_POSTS}/${id}/view`, {
      method: "POST",
    });
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
