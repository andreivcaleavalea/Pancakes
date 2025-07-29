import { useState, useEffect, useCallback, useRef } from "react";
import { postRatingApi } from "@/services/ratingApi";
import type { PostRatingStats } from "@/types/rating";

export const usePostRating = (blogPostId: string) => {
  const [stats, setStats] = useState<PostRatingStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const lastFetchedIdRef = useRef<string>("");

  const fetchStats = useCallback(async () => {
    if (!blogPostId || blogPostId.trim() === "") {
      setLoading(false);
      return;
    }

    // Avoid refetching the same ID
    if (lastFetchedIdRef.current === blogPostId && stats) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const ratingStats = await postRatingApi.getStats(blogPostId);
      setStats(ratingStats);
      lastFetchedIdRef.current = blogPostId;
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load rating stats"
      );
      console.error("Error fetching rating stats:", err);
      // Set default stats to ensure UI works
      setStats({
        blogPostId,
        averageRating: 0,
        totalRatings: 0,
        userRating: undefined,
        ratingDistribution: {},
      });
      lastFetchedIdRef.current = blogPostId;
    } finally {
      setLoading(false);
    }
  }, [blogPostId, stats]);

  const submitRating = useCallback(
    async (rating: number) => {
      if (!blogPostId) throw new Error("No blog post ID provided");

      if (!blogPostId || blogPostId.trim() === "") {
        throw new Error("BlogPostId is empty or invalid");
      }

      try {
        const requestData = {
          blogPostId: blogPostId.trim(),
          rating,
        };

        await postRatingApi.createOrUpdate(requestData);

        // Refresh stats after successful rating
        lastFetchedIdRef.current = ""; // Force refetch
        await fetchStats();
      } catch (err) {
        console.error("Rating submission failed:", err);
        throw new Error(
          err instanceof Error ? err.message : "Failed to submit rating"
        );
      }
    },
    [blogPostId, fetchStats]
  );

  const deleteRating = useCallback(async () => {
    if (!blogPostId) throw new Error("No blog post ID provided");

    try {
      await postRatingApi.delete(blogPostId);
      // Refresh stats after successful deletion
      lastFetchedIdRef.current = ""; // Force refetch
      await fetchStats();
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to delete rating"
      );
    }
  }, [blogPostId, fetchStats]);

  useEffect(() => {
    // Only fetch if blogPostId changed
    if (blogPostId && blogPostId !== lastFetchedIdRef.current) {
      fetchStats();
    } else if (!blogPostId) {
      setLoading(false);
      setStats(null);
    }
  }, [blogPostId, fetchStats]);

  return {
    stats,
    loading,
    error,
    submitRating,
    deleteRating,
    refetch: fetchStats,
  };
};
