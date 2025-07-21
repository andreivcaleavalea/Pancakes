import { apiRequest } from "@/utils/api";
import type {
  CreatePostRatingDto,
  PostRating,
  PostRatingStats,
} from "@/types/rating";

export const postRatingApi = {
  // Get rating statistics for a blog post
  getStats: async (blogPostId: string): Promise<PostRatingStats> => {
    return apiRequest<PostRatingStats>(`/api/postratings/stats/${blogPostId}`);
  },

  // Create or update a rating
  createOrUpdate: async (
    data: Omit<CreatePostRatingDto, "userIdentifier">
  ): Promise<PostRating> => {
    return apiRequest<PostRating>(`/api/postratings`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  // Delete a rating
  delete: async (blogPostId: string): Promise<void> => {
    return apiRequest<void>(`/api/postratings/${blogPostId}`, {
      method: "DELETE",
    });
  },
};
