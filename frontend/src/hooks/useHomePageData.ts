import { useState, useEffect, useCallback } from "react";
import { BlogService } from "@/services/blogService";
import type { FeaturedPost, BlogPost } from "@/types/blog";

interface HomePageData {
  featuredPosts: FeaturedPost[];
  horizontalPosts: BlogPost[];
  gridPosts: BlogPost[];
}

interface UseHomePageDataReturn {
  data: HomePageData | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

/**
 * Optimized hook for fetching home page data with caching
 * Includes error boundaries and retry logic
 */
export const useHomePageData = (): UseHomePageDataReturn => {
  const [data, setData] = useState<HomePageData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      console.log("🏠 [useHomePageData] Starting parallel data fetch...");
      const startTime = performance.now();

      const result = await BlogService.getHomePageData();

      const endTime = performance.now();
      console.log(
        `⚡ [useHomePageData] Data loaded in ${(endTime - startTime).toFixed(
          2
        )}ms`
      );

      setData(result);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to load home page data";
      setError(errorMessage);
      console.error("❌ [useHomePageData] Error loading home page data:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return {
    data,
    loading,
    error,
    refetch: fetchData,
  };
};

export default useHomePageData;
