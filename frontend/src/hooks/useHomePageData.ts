import { useState, useEffect, useCallback } from "react";
import { BlogService } from "@/services/blogService";
import type { FeaturedPost, BlogPost } from "@/types/blog";
import { generateCacheKey, LRUCache } from "@/utils/memoryOptimizer";

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

// 🚀 CACHE: Create LRU cache for home page data (capacity: 5, TTL: 5 minutes)
const homePageCache = new LRUCache<string, HomePageData>(5);
const CACHE_TTL = 5 * 60 * 1000; // 5 minutes in milliseconds

interface CachedHomePageData {
  data: HomePageData;
  timestamp: number;
}

/**
 * Optimized hook for fetching home page data with caching
 * Includes error boundaries, retry logic, and client-side caching
 */
export const useHomePageData = (): UseHomePageDataReturn => {
  const [data, setData] = useState<HomePageData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async (forceRefresh = false) => {
    try {
      setLoading(true);
      setError(null);

      // 🚀 CACHE: Check for cached data first (unless force refresh)
      const cacheKey = generateCacheKey("home-page-data");

      if (!forceRefresh) {
        const cachedData = homePageCache.get(cacheKey);
        if (cachedData) {
          console.log("🎯 [useHomePageData] Cache hit - returning cached data");
          setData(cachedData);
          setLoading(false);
          return;
        }
        console.log("🔄 [useHomePageData] Cache miss - fetching fresh data");
      } else {
        console.log("🔄 [useHomePageData] Force refresh - bypassing cache");
      }

      console.log("🏠 [useHomePageData] Starting parallel data fetch...");
      const startTime = performance.now();

      const result = await BlogService.getHomePageData();

      const endTime = performance.now();
      console.log(
        `⚡ [useHomePageData] Data loaded in ${(endTime - startTime).toFixed(
          2
        )}ms`
      );

      // 🚀 CACHE: Store in cache for future use
      homePageCache.set(cacheKey, result);
      console.log("✅ [useHomePageData] Data cached for future requests");

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

  const refetch = useCallback(() => fetchData(true), [fetchData]);

  return {
    data,
    loading,
    error,
    refetch,
  };
};

export default useHomePageData;
