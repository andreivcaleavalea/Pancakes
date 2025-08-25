import { blogPostsApi } from "./blogApi";
import type {
  BlogPost,
  FeaturedPost,
  PaginatedResult,
  BlogPostQueryParams,
} from "@/types/blog";
import { ApiError } from "@/utils/api";
import { LRUCache, generateCacheKey } from "@/utils/memoryOptimizer";

// 🚀 REQUEST DEDUPLICATION: Cache for in-flight requests
const inflightRequests = new Map<string, Promise<any>>();

/**
 * Blog Service - Centralized business logic for blog operations
 * This service handles data transformation and business rules with request deduplication
 */
export class BlogService {
  /**
   * 🚀 REQUEST DEDUPLICATION: Execute function with deduplication
   * Prevents duplicate simultaneous API calls by reusing in-flight requests
   */
  private static async deduplicateRequest<T>(
    key: string,
    fetchFn: () => Promise<T>
  ): Promise<T> {
    // Check if request is already in flight
    if (inflightRequests.has(key)) {
      return inflightRequests.get(key) as Promise<T>;
    }

    // Execute the request and cache the promise
    const promise = fetchFn().finally(() => {
      // Clean up the cache when request completes
      inflightRequests.delete(key);
    });

    inflightRequests.set(key, promise);

    return promise;
  }

  /**
   * Transform BlogPost to include legacy fields for backward compatibility
   */
  private static transformBlogPost(post: BlogPost): BlogPost {
    return {
      ...post,
      description: post.excerpt || post.content.substring(0, 150) + "...",
      date: post.publishedAt || post.createdAt,
      image: post.featuredImage || "/placeholder-image.jpg",
      // Use the author information from the backend (already populated by UserService)
      author: post.authorName || "Unknown Author",
      authorAvatar: post.authorImage || "/default-avatar.png",
    };
  }

  /**
   * Get blog data for home page with request deduplication
   */
  static async getHomePageData(): Promise<{
    featuredPosts: FeaturedPost[];
    horizontalPosts: BlogPost[];
    gridPosts: BlogPost[];
  }> {
    return this.deduplicateRequest("getHomePageData", async () => {
      try {
        // Prepare grid posts params
        const gridPostsParams: BlogPostQueryParams = {
          page: 1,
          pageSize: 6,
          isFeatured: false,
          sortBy: "createdAt",
          sortOrder: "desc",
        };

        // 🚀 OPTIMIZED: Fetch all data in parallel with deduplication for each endpoint
        const [featuredPostsData, horizontalPostsData, gridPostsResult] =
          await Promise.all([
            this.deduplicateRequest("getFeatured-3", () =>
              blogPostsApi.getFeatured(3)
            ),
            this.deduplicateRequest("getPersonalizedPopular-4", () =>
              blogPostsApi.getPersonalizedPopular(4)
            ),
            this.deduplicateRequest(
              `getAll-${generateCacheKey("gridPosts", gridPostsParams)}`,
              () => blogPostsApi.getAll(gridPostsParams)
            ),
          ]);

        // Transform the data
        const featuredPosts = featuredPostsData.map((post) => ({
          ...this.transformBlogPost(post),
          isFeatured: true as const,
        })) as FeaturedPost[];

        const horizontalPosts = horizontalPostsData.map(this.transformBlogPost);
        const gridPosts = gridPostsResult.data.map(this.transformBlogPost);

        return {
          featuredPosts,
          horizontalPosts,
          gridPosts,
        };
      } catch (error) {
        console.error("Error fetching home page data:", error);
        throw new Error("Failed to load blog data");
      }
    });
  }

  /**
   * Get paginated posts for a specific page
   */
  static async getPaginatedPosts(
    page: number,
    pageSize: number = 9,
    tags?: string[]
  ): Promise<PaginatedResult<BlogPost>> {
    try {
      const params: BlogPostQueryParams = {
        page,
        pageSize,
        sortBy: "createdAt",
        sortOrder: "desc",
        ...(tags && tags.length > 0 && { tags }),
      };

      const result = await blogPostsApi.getAll(params);

      return {
        data: result.data.map(this.transformBlogPost),
        pagination: result.pagination,
      };
    } catch (error) {
      console.error("Error fetching paginated posts:", error);
      throw new Error("Failed to load posts");
    }
  }

  /**
   * Get a single blog post by ID
   */
  static async getPostById(id: string): Promise<BlogPost | null> {
    try {
      const post = await blogPostsApi.getById(id);
      return this.transformBlogPost(post);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return null;
      }
      console.error(`Error fetching blog post ${id}:`, error);
      throw new Error("Failed to load blog post");
    }
  }

  /**
   * Toggle featured status of a blog post
   */
  static async toggleFeaturedStatus(id: string): Promise<boolean> {
    try {
      await blogPostsApi.toggleFeatured(id);
      return true;
    } catch (error) {
      console.error(`Error toggling featured status for post ${id}:`, error);
      return false;
    }
  }

  /**
   * Search blog posts
   */
  static async searchPosts(
    searchTerm: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedResult<BlogPost>> {
    try {
      const params: BlogPostQueryParams = {
        page,
        pageSize,
        search: searchTerm,
        sortBy: "createdAt",
        sortOrder: "desc",
      };

      const result = await blogPostsApi.getAll(params);

      return {
        data: result.data.map(this.transformBlogPost),
        pagination: result.pagination,
      };
    } catch (error) {
      console.error("Error searching posts:", error);
      throw new Error("Failed to search posts");
    }
  }
}
