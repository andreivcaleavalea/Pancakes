import { useState, useEffect, useCallback } from 'react';
import { BlogService } from '@/services/blogService';
import type { BlogPost, FeaturedPost } from '@/types/blog';

/**
 * Custom hook for managing blog data on the home page
 */
export const useBlogData = () => {
  const [data, setData] = useState<{
    featuredPosts: FeaturedPost[];
    horizontalPosts: BlogPost[];
    gridPosts: BlogPost[];
  }>({
    featuredPosts: [],
    horizontalPosts: [],
    gridPosts: []
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);
      const blogData = await BlogService.getHomePageData();
      setData(blogData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load blog data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  return { data, loading, error, refetch: fetchData };
};

/**
 * Custom hook for managing paginated posts
 */
export const usePaginatedPosts = (page: number, pageSize: number) => {
  const [data, setData] = useState<BlogPost[]>([]);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 9,
    totalPages: 0,
    totalItems: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPosts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await BlogService.getPaginatedPosts(page, pageSize);
      setData(result.data);
      setPagination(result.pagination);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load posts');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchPosts();
  }, [fetchPosts]);

  return { data, pagination, loading, error, refetch: fetchPosts };
};

/**
 * Custom hook for responsive design
 */
export const useResponsive = (breakpoint: number = 768) => {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= breakpoint);
    };

    // Set initial value
    handleResize();

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [breakpoint]);

  return isMobile;
};

/**
 * Custom hook for managing favorite status
 */
export const useFavorite = (postId: string, initialState: boolean = false) => {
  const [isFavorite, setIsFavorite] = useState(initialState);
  const [loading, setLoading] = useState(false);

  const toggleFavorite = async () => {
    setLoading(true);
    try {
      const success = await BlogService.toggleFeaturedStatus(postId);
      if (success) {
        setIsFavorite(!isFavorite);
        return true;
      }
      return false;
    } catch (error) {
      console.error('Error toggling favorite:', error);
      return false;
    } finally {
      setLoading(false);
    }
  };

  return { isFavorite, loading, toggleFavorite };
};
