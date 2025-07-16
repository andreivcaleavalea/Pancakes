import { blogPostsApi } from './blogApi';
import type { BlogPost, FeaturedPost, PaginatedResult, BlogPostQueryParams } from '@/types/blog';
import { ApiError } from '@/utils/api';

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
      description: post.excerpt || post.content.substring(0, 150) + '...',
      date: post.publishedAt || post.createdAt,
      image: post.featuredImage || '/placeholder-image.jpg',
      // Note: author and authorAvatar will need to be fetched from UserService later
      author: 'Author Name', // TODO: Implement user service integration
      authorAvatar: '/default-avatar.png' // TODO: Implement user service integration
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
      // Fetch featured posts
      const featuredPostsData = await blogPostsApi.getFeatured(3);
      const featuredPosts = featuredPostsData.map(post => ({
        ...this.transformBlogPost(post),
        isFeatured: true as const
      })) as FeaturedPost[];

      // Fetch horizontal posts (popular posts)
      const horizontalPostsData = await blogPostsApi.getPopular(4);
      const horizontalPosts = horizontalPostsData.map(this.transformBlogPost);

      // Fetch grid posts (recent posts excluding featured and popular)
      const gridPostsParams: BlogPostQueryParams = {
        page: 1,
        pageSize: 6,
        isFeatured: false,
        sortBy: 'createdAt',
        sortOrder: 'desc'
      };
      const gridPostsResult = await blogPostsApi.getAll(gridPostsParams);
      const gridPosts = gridPostsResult.data.map(this.transformBlogPost);

      return {
        featuredPosts,
        horizontalPosts,
        gridPosts
      };
    } catch (error) {
      console.error('Error fetching home page data:', error);
      throw new Error('Failed to load blog data');
    }
  }

  /**
   * Get paginated posts for a specific page
   */
  static async getPaginatedPosts(
    page: number,
    pageSize: number = 9
  ): Promise<PaginatedResult<BlogPost>> {
    try {
      const params: BlogPostQueryParams = {
        page,
        pageSize,
        sortBy: 'createdAt',
        sortOrder: 'desc'
      };
      
      const result = await blogPostsApi.getAll(params);
      
      return {
        data: result.data.map(this.transformBlogPost),
        pagination: result.pagination
      };
    } catch (error) {
      console.error('Error fetching paginated posts:', error);
      throw new Error('Failed to load posts');
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
      throw new Error('Failed to load blog post');
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
        sortBy: 'createdAt',
        sortOrder: 'desc'
      };
      
      const result = await blogPostsApi.getAll(params);
      
      return {
        data: result.data.map(this.transformBlogPost),
        pagination: result.pagination
      };
    } catch (error) {
      console.error('Error searching posts:', error);
      throw new Error('Failed to search posts');
    }
  }
}
