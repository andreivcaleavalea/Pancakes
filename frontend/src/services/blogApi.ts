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

// --- Helpers ---------------------------------------------------------------
type RawPagination = {
  currentPage?: number; CurrentPage?: number;
  pageSize?: number; PageSize?: number;
  totalPages?: number; TotalPages?: number;
  totalItems?: number; TotalItems?: number;
  hasNextPage?: boolean; HasNextPage?: boolean;
  hasPreviousPage?: boolean; HasPreviousPage?: boolean;
};
interface RawShape<T> { data?: T[]; Data?: T[]; pagination?: RawPagination; Pagination?: RawPagination; }
function normalizePaginated<T>(rawUnknown: unknown, fallbackPageSize = 0): PaginatedResult<T> {
  const raw = rawUnknown as RawShape<T> | null | undefined;
  const itemsSource = raw?.data || raw?.Data;
  const items: T[] = Array.isArray(itemsSource) ? itemsSource : [];
  const p: RawPagination = (raw?.pagination || raw?.Pagination || {}) as RawPagination;
  return {
    data: items,
    pagination: {
      currentPage: p.currentPage ?? p.CurrentPage ?? 1,
      pageSize: p.pageSize ?? p.PageSize ?? fallbackPageSize,
      totalPages: p.totalPages ?? p.TotalPages ?? 0,
      totalItems: p.totalItems ?? p.TotalItems ?? 0,
      hasNextPage: p.hasNextPage ?? p.HasNextPage ?? false,
      hasPreviousPage: p.hasPreviousPage ?? p.HasPreviousPage ?? false,
    },
  };
}

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

  const raw = await publicBlogRequest<unknown>(endpoint);
  return normalizePaginated<BlogPost>(raw, params.pageSize || 0);
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
    const result = await publicBlogRequest<BlogPost[]>(
      `${ENDPOINTS.BLOG_POSTS}/popular?count=${count}`
    );
    return result;
  },

  // Get personalized popular posts (AUTHENTICATED - personalized recommendations)
  getPersonalizedPopular: async (count: number = 5): Promise<BlogPost[]> => {
    // Check if user is authenticated
    const authSession = localStorage.getItem("auth-session");
    let isAuthenticated = false;
    let userId = null;

    if (authSession) {
      try {
        const session = JSON.parse(authSession);
        isAuthenticated = !!session.token;
        userId = session.user?.id || session.userId || "unknown";
      } catch (error) {
        console.error("❌ [BlogAPI] Error parsing auth session:", error);
      }
    }

    if (!isAuthenticated) {
      return blogPostsApi.getPopular(count);
    }

    try {
      const result = await authenticatedBlogRequest<BlogPost[]>(
        `${ENDPOINTS.BLOG_POSTS}/popular?count=${count}`
      );
      return result;
    } catch (error) {
      console.error(
        "❌ [BlogAPI] Error getting personalized popular posts:",
        error
      );
      return blogPostsApi.getPopular(count);
    }
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

  // Get user's draft posts
  getDrafts: async (
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResult<BlogPost>> => {
    // Add cache busting parameter to ensure fresh data
    const timestamp = Date.now();
    const url = `${ENDPOINTS.BLOG_POSTS}/drafts?page=${page}&pageSize=${pageSize}&_t=${timestamp}`;
    console.log("🌐 [blogAPI] getDrafts called:", {
      page,
      pageSize,
      timestamp,
      url,
    });

    const raw = await authenticatedBlogRequest<unknown>(url);
    const normalized = normalizePaginated<BlogPost>(raw, pageSize);
    console.log("🌐 [blogAPI] getDrafts response (normalized):", {
      dataLength: normalized.data.length,
      totalItems: normalized.pagination.totalItems,
      firstDraftId: normalized.data[0]?.id,
      firstDraftUpdatedAt: normalized.data[0]?.updatedAt,
    });
    return normalized;
  },

  // Convert published post to draft (for admin or user)
  convertToDraft: async (id: string): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(
      `${ENDPOINTS.BLOG_POSTS}/${id}/convert-to-draft`,
      {
        method: "PATCH",
      }
    );
  },

  // Publish a draft post
  publishDraft: async (id: string): Promise<BlogPost> => {
    return authenticatedBlogRequest<BlogPost>(
      `${ENDPOINTS.BLOG_POSTS}/${id}/publish`,
      {
        method: "PATCH",
      }
    );
  },
};
