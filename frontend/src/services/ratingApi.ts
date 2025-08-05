import { authenticatedBlogRequest } from "@/utils/blogApi";
import type {
  CreatePostRatingDto,
  PostRating,
  PostRatingStats,
} from "@/types/rating";

// Simple in-memory cache to prevent duplicate API calls
const ratingStatsCache = new Map<
  string,
  {
    data: PostRatingStats;
    timestamp: number;
  }
>();

const CACHE_DURATION = 5000; // 5 seconds cache

export const postRatingApi = {
  // Get rating statistics for a blog post
  getStats: async (
    blogPostId: string,
    forceRefresh: boolean = false
  ): Promise<PostRatingStats> => {
    // Check cache first (unless force refresh is requested)
    if (!forceRefresh) {
      const cached = ratingStatsCache.get(blogPostId);
      const now = Date.now();

      if (cached && now - cached.timestamp < CACHE_DURATION) {
        return cached.data;
      }
    }

    // Make API call if not cached, expired, or force refresh requested
    const data = await authenticatedBlogRequest<PostRatingStats>(
      `/api/postratings/stats/${blogPostId}`
    );

    // Cache the result
    const now = Date.now();
    ratingStatsCache.set(blogPostId, {
      data,
      timestamp: now,
    });

    return data;
  },

  // Create or update a rating
  createOrUpdate: async (
    data: Omit<CreatePostRatingDto, "userId">
  ): Promise<PostRating> => {
    // Get token directly (same as console test that worked)
    const authSession = localStorage.getItem("auth-session");
    if (!authSession) {
      throw new Error("No authentication session found");
    }

    const session = JSON.parse(authSession);
    const token = session.token;

    if (!token) {
      throw new Error("No authentication token found");
    }

    const response = await fetch(`http://localhost:5001/api/postratings`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const result = await response.json();

    // Invalidate cache for this blog post since rating was updated
    ratingStatsCache.delete(data.blogPostId);

    return result;
  },

  // Delete a rating
  delete: async (blogPostId: string): Promise<void> => {
    const result = await authenticatedBlogRequest<void>(
      `/api/postratings/${blogPostId}`,
      {
        method: "DELETE",
      }
    );

    // Invalidate cache for this blog post since rating was deleted
    ratingStatsCache.delete(blogPostId);

    return result;
  },
};
