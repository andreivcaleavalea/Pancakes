import { apiRequest } from "@/utils/api";
import type {
  CreateCommentLikeDto,
  CommentLike,
  CommentLikeStats,
} from "@/types/rating";

export const commentLikeApi = {
  // Get like statistics for a comment
  getStats: async (commentId: string): Promise<CommentLikeStats> => {
    return apiRequest<CommentLikeStats>(`/api/commentlikes/stats/${commentId}`);
  },

  // Create or update a like/dislike
  createOrUpdate: async (
    data: Omit<CreateCommentLikeDto, "userIdentifier">
  ): Promise<CommentLike> => {
    return apiRequest<CommentLike>(`/api/commentlikes`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  // Delete a like/dislike
  delete: async (commentId: string): Promise<void> => {
    return apiRequest<void>(`/api/commentlikes/${commentId}`, {
      method: "DELETE",
    });
  },
};
