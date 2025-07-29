import { useState, useEffect, useCallback, useMemo, useRef } from "react";
import { postRatingApi } from "@/services/ratingApi";

export const useAverageRatings = (blogPostIds: string[]) => {
  const [averageRatings, setAverageRatings] = useState<
    Record<string, { averageRating: number; totalRatings: number }>
  >({});
  const [loading, setLoading] = useState(false);
  const previousIdsRef = useRef<string[]>([]);

  // Memoize the sorted blog post IDs to prevent unnecessary refetches
  const sortedBlogPostIds = useMemo(() => {
    return [...blogPostIds].sort();
  }, [blogPostIds]);

  // Check if the IDs have actually changed
  const idsChanged = useMemo(() => {
    const prev = previousIdsRef.current;
    if (prev.length !== sortedBlogPostIds.length) return true;
    return !prev.every((id, index) => id === sortedBlogPostIds[index]);
  }, [sortedBlogPostIds]);

  const fetchAverageRatings = useCallback(async () => {
    if (sortedBlogPostIds.length === 0) return;

    try {
      setLoading(true);
      const ratings: Record<
        string,
        { averageRating: number; totalRatings: number }
      > = {};

      // Fetch rating stats for each blog post
      await Promise.all(
        sortedBlogPostIds.map(async (blogPostId) => {
          try {
            const stats = await postRatingApi.getStats(blogPostId);
            // Only include posts with at least one rating
            if (stats.totalRatings > 0) {
              ratings[blogPostId] = {
                averageRating: stats.averageRating,
                totalRatings: stats.totalRatings,
              };
            }
          } catch (error) {
            // Ignore errors for individual posts
            console.debug(
              `Failed to fetch average rating for post ${blogPostId}:`,
              error
            );
          }
        })
      );

      setAverageRatings(ratings);
    } catch (error) {
      console.error("Error fetching average ratings:", error);
    } finally {
      setLoading(false);
    }
  }, [sortedBlogPostIds]);

  useEffect(() => {
    // Only fetch if the IDs have actually changed
    if (idsChanged && sortedBlogPostIds.length > 0) {
      previousIdsRef.current = [...sortedBlogPostIds];
      fetchAverageRatings();
    }
  }, [idsChanged, sortedBlogPostIds, fetchAverageRatings]);

  return {
    averageRatings,
    loading,
    refetchRatings: fetchAverageRatings,
  };
};
