import { useState, useEffect, useCallback, useMemo, useRef } from "react";
import { postRatingApi } from "@/services/ratingApi";

export const useUserRatings = (blogPostIds: string[]) => {
  const [userRatings, setUserRatings] = useState<Record<string, number>>({});
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

  const fetchUserRatings = useCallback(async () => {
    if (sortedBlogPostIds.length === 0) return;

    try {
      setLoading(true);
      const ratings: Record<string, number> = {};

      // Fetch rating stats for each blog post
      await Promise.all(
        sortedBlogPostIds.map(async (blogPostId) => {
          try {
            const stats = await postRatingApi.getStats(blogPostId);
            if (stats.userRating && stats.userRating > 0) {
              ratings[blogPostId] = stats.userRating;
            }
          } catch (error) {
            // Ignore errors for individual posts
            console.debug(
              `Failed to fetch rating for post ${blogPostId}:`,
              error
            );
          }
        })
      );

      setUserRatings(ratings);
    } catch (error) {
      console.error("Error fetching user ratings:", error);
    } finally {
      setLoading(false);
    }
  }, [sortedBlogPostIds]);

  useEffect(() => {
    // Only fetch if the IDs have actually changed
    if (idsChanged && sortedBlogPostIds.length > 0) {
      previousIdsRef.current = [...sortedBlogPostIds];
      fetchUserRatings();
    }
  }, [idsChanged, sortedBlogPostIds, fetchUserRatings]);

  return {
    userRatings,
    loading,
    refetchRatings: fetchUserRatings,
  };
};
