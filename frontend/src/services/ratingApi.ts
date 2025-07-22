import { apiRequest } from "@/utils/api";
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
  getStats: async (blogPostId: string): Promise<PostRatingStats> => {
    // Check cache first
    const cached = ratingStatsCache.get(blogPostId);
    const now = Date.now();

    if (cached && now - cached.timestamp < CACHE_DURATION) {
      console.log(`🎯 Using cached rating stats for ${blogPostId}`);
      return cached.data;
    }

    // Make API call if not cached or expired
    const data = await apiRequest<PostRatingStats>(
      `/api/postratings/stats/${blogPostId}`
    );

    // Cache the result
    ratingStatsCache.set(blogPostId, {
      data,
      timestamp: now,
    });

    return data;
  },

  // Create or update a rating
  createOrUpdate: async (
    data: Omit<CreatePostRatingDto, "userIdentifier">
  ): Promise<PostRating> => {
    const result = await apiRequest<PostRating>(`/api/postratings`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    // Invalidate cache for this blog post since rating was updated
    ratingStatsCache.delete(data.blogPostId);

    return result;
  },

  // Delete a rating
  delete: async (blogPostId: string): Promise<void> => {
    const result = await apiRequest<void>(`/api/postratings/${blogPostId}`, {
      method: "DELETE",
    });

    // Invalidate cache for this blog post since rating was deleted
    ratingStatsCache.delete(blogPostId);

    return result;
  },
};
