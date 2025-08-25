import { useState, useEffect, useCallback } from 'react';
import type { BlogPostQueryParams } from '@/types/blog';

export interface UseApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useApi<T>(
  apiCall: () => Promise<T>
): UseApiState<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await apiCall();
      setData(result);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An error occurred';
      setError(errorMessage);
      console.error('API call failed:', err);
    } finally {
      setLoading(false);
    }
  }, [apiCall]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return {
    data,
    loading,
    error,
    refetch: fetchData
  };
}

// Specialized hook for blog posts
export function useBlogPosts(params?: BlogPostQueryParams) {
  const [blogPostsApi, setBlogPostsApi] = useState<typeof import('@/services/blogApi').blogPostsApi | null>(null);
  
  useEffect(() => {
    // Dynamic import to avoid circular dependencies
    import('@/services/blogApi').then(module => {
      setBlogPostsApi(module.blogPostsApi);
    });
  }, []);

  const apiCall = useCallback(() => {
    if (!blogPostsApi) {
      return Promise.resolve({ data: [], pagination: { currentPage: 1, pageSize: 10, totalPages: 0, totalItems: 0, hasNextPage: false, hasPreviousPage: false } });
    }
    return blogPostsApi.getAll(params);
  }, [blogPostsApi, params]);

  return useApi(apiCall);
}

// Hook for featured posts
export function useFeaturedPosts(count: number = 5) {
  const [blogPostsApi, setBlogPostsApi] = useState<typeof import('@/services/blogApi').blogPostsApi | null>(null);
  
  useEffect(() => {
    import('@/services/blogApi').then(module => {
      setBlogPostsApi(module.blogPostsApi);
    });
  }, []);

  const apiCall = useCallback(() => {
    if (!blogPostsApi) {
      return Promise.resolve([]);
    }
    return blogPostsApi.getFeatured(count);
  }, [blogPostsApi, count]);

  return useApi(apiCall);
}

// Hook for popular posts (PUBLIC - no personalization)
export function usePopularPosts(count: number = 5) {
  const [blogPostsApi, setBlogPostsApi] = useState<typeof import('@/services/blogApi').blogPostsApi | null>(null);
  
  useEffect(() => {
    import('@/services/blogApi').then(module => {
      setBlogPostsApi(module.blogPostsApi);
    });
  }, []);

  const apiCall = useCallback(() => {
    console.log("🔧 [usePopularPosts] Getting public popular posts (no personalization):", { count });
    if (!blogPostsApi) {
      return Promise.resolve([]);
    }
    return blogPostsApi.getPopular(count);
  }, [blogPostsApi, count]);

  return useApi(apiCall);
}

// Hook for personalized popular posts (AUTHENTICATED - with personalization)
export function usePersonalizedPopularPosts(count: number = 5) {
  const [blogPostsApi, setBlogPostsApi] = useState<typeof import('@/services/blogApi').blogPostsApi | null>(null);
  
  useEffect(() => {
    import('@/services/blogApi').then(module => {
      setBlogPostsApi(module.blogPostsApi);
    });
  }, []);

  const apiCall = useCallback(() => {
    console.log("🎯 [usePersonalizedPopularPosts] Getting personalized popular posts:", { count });
    if (!blogPostsApi) {
      return Promise.resolve([]);
    }
    return blogPostsApi.getPersonalizedPopular(count);
  }, [blogPostsApi, count]);

  return useApi(apiCall);
}
