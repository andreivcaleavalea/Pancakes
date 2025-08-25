import { useState, useCallback } from "react";
import { useSavedBlogsContext } from "@/contexts/SavedBlogsContext";

interface UseOptimizedFavoriteReturn {
  isFavorite: boolean;
  loading: boolean;
  toggleFavorite: () => Promise<void>;
}

/**
 * Optimized hook for managing favorite status of blog posts
 * Uses context to avoid individual API calls for each blog post
 *
 * @param blogId - The ID of the blog post
 * @returns Object with favorite status and toggle function
 */
export const useOptimizedFavorite = (
  blogId: string
): UseOptimizedFavoriteReturn => {
  const { isPostSaved, toggleSaved } = useSavedBlogsContext();
  const [loading, setLoading] = useState(false);

  const isFavorite = isPostSaved(blogId);

  const toggleFavorite = useCallback(async () => {
    try {
      setLoading(true);
      console.log(
        `🔄 [useOptimizedFavorite] Toggling favorite for blog ${blogId}...`
      );

      await toggleSaved(blogId);

      console.log(
        `✅ [useOptimizedFavorite] Successfully toggled favorite for blog ${blogId}`
      );
    } catch (error) {
      console.error(
        `❌ [useOptimizedFavorite] Error toggling favorite for blog ${blogId}:`,
        error
      );
      throw error;
    } finally {
      setLoading(false);
    }
  }, [blogId, toggleSaved]);

  return {
    isFavorite,
    loading,
    toggleFavorite,
  };
};

export default useOptimizedFavorite;
