import { blogPostsApi } from "./blogApi";
import type {
  BlogPost,
  FeaturedPost,
  PaginatedResult,
  BlogPostQueryParams,
} from "@/types/blog";
import { ApiError } from "@/utils/api";

/**
 * Blog Service - Centralized business logic for blog operations
 * This service handles data transformation and business rules
 */
export class BlogService {
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
   * Get blog data for home page
   */
  static async getHomePageData(): Promise<{
    featuredPosts: FeaturedPost[];
    horizontalPosts: BlogPost[];
    gridPosts: BlogPost[];
  }> {
    try {
      console.log("🏠 [BlogService] Loading home page data in parallel...");

      // Prepare grid posts params
      const gridPostsParams: BlogPostQueryParams = {
        page: 1,
        pageSize: 6,
        isFeatured: false,
        sortBy: "createdAt",
        sortOrder: "desc",
      };

      // Fetch all data in parallel for faster loading
      const [featuredPostsData, horizontalPostsData, gridPostsResult] =
        await Promise.all([
          blogPostsApi.getFeatured(3),
          blogPostsApi.getPersonalizedPopular(4),
          blogPostsApi.getAll(gridPostsParams),
        ]);

      // Transform the data
      const featuredPosts = featuredPostsData.map((post) => ({
        ...this.transformBlogPost(post),
        isFeatured: true as const,
      })) as FeaturedPost[];

      const horizontalPosts = horizontalPostsData.map(this.transformBlogPost);
      const gridPosts = gridPostsResult.data.map(this.transformBlogPost);

      console.log("✅ [BlogService] All home page data loaded:", {
        featuredCount: featuredPosts.length,
        horizontalCount: horizontalPosts.length,
        gridCount: gridPosts.length,
      });

      return {
        featuredPosts,
        horizontalPosts,
        gridPosts,
      };
    } catch (error) {
      console.error("Error fetching home page data:", error);
      throw new Error("Failed to load blog data");
    }
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
