import { useState, useEffect, useCallback } from "react";
import { commentLikeApi } from "@/services/commentLikeApi";
import type { CommentLikeStats } from "@/types/rating";

export const useCommentLikes = (commentId: string) => {
  const [stats, setStats] = useState<CommentLikeStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStats = useCallback(async () => {
    if (!commentId) return;

    try {
      setLoading(true);
      setError(null);
      const likeStats = await commentLikeApi.getStats(commentId);
      setStats(likeStats);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load like stats"
      );
      console.error("Error fetching like stats:", err);
      // Set default stats to ensure UI works
      setStats({
        commentId,
        likeCount: 0,
        dislikeCount: 0,
        userLike: undefined,
      });
    } finally {
      setLoading(false);
    }
  }, [commentId]);

  const submitLike = useCallback(
    async (isLike: boolean) => {
      if (!commentId) throw new Error("No comment ID provided");

      try {
        await commentLikeApi.createOrUpdate({
          commentId,
          isLike,
        });

        // Refresh stats after successful like/dislike
        await fetchStats();
      } catch (err) {
        throw new Error(
          err instanceof Error ? err.message : "Failed to submit like"
        );
      }
    },
    [commentId, fetchStats]
  );

  const deleteLike = useCallback(async () => {
    if (!commentId) throw new Error("No comment ID provided");

    try {
      await commentLikeApi.delete(commentId);
      // Refresh stats after successful deletion
      await fetchStats();
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to delete like"
      );
    }
  }, [commentId, fetchStats]);

  const toggleLike = useCallback(async () => {
    if (stats?.userLike === true) {
      // User already liked - remove the like
      await deleteLike();
    } else {
      // User hasn't liked or disliked - add like
      await submitLike(true);
    }
  }, [stats?.userLike, submitLike, deleteLike]);

  const toggleDislike = useCallback(async () => {
    if (stats?.userLike === false) {
      // User already disliked - remove the dislike
      await deleteLike();
    } else {
      // User hasn't disliked or liked - add dislike
      await submitLike(false);
    }
  }, [stats?.userLike, submitLike, deleteLike]);

  useEffect(() => {
    fetchStats();
  }, [fetchStats]);

  return {
    stats,
    loading,
    error,
    submitLike,
    deleteLike,
    toggleLike,
    toggleDislike,
    refetch: fetchStats,
  };
};
