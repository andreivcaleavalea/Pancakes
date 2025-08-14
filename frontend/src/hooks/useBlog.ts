import { useState, useEffect, useCallback, useRef } from "react";
import { BlogService } from "@/services/blogService";
import { isAuthenticated } from "@/lib/api";
import type { BlogPost, FeaturedPost } from "@/types/blog";

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
    gridPosts: [],
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
      setError(err instanceof Error ? err.message : "Failed to load blog data");
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
export const usePaginatedPosts = (
  page: number,
  pageSize: number,
  tags?: string[]
) => {
  const [data, setData] = useState<BlogPost[]>([]);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 9,
    totalPages: 0,
    totalItems: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPosts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await BlogService.getPaginatedPosts(page, pageSize, tags);
      setData(result.data);
      setPagination(result.pagination);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load posts");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, tags]);

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

    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, [breakpoint]);

  return isMobile;
};

/**
 * Custom hook for fetching user's draft posts
 */
export const useDrafts = (page: number = 1, pageSize: number = 10) => {
  const [data, setData] = useState<BlogPost[]>([]);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalPages: 0,
    totalItems: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Use ref to track if a call is in progress without causing re-renders
  const isFetchingRef = useRef(false);

  const fetchDrafts = useCallback(async () => {
    // Prevent multiple simultaneous calls
    if (isFetchingRef.current) {
      console.log("🚫 [useDrafts] fetchDrafts skipped - already fetching");
      return;
    }

    console.log("🔄 [useDrafts] fetchDrafts called with:", { page, pageSize });

    // Check authentication status
    const authSession = localStorage.getItem("auth-session");
    console.log("🔐 [useDrafts] Auth check:", {
      hasAuthSession: !!authSession,
      sessionLength: authSession?.length || 0,
    });

    try {
      isFetchingRef.current = true;
      setLoading(true);
      setError(null);

      // Import blogPostsApi dynamically to avoid circular dependencies
      const { blogPostsApi } = await import("@/services/blogApi");
      console.log("📡 [useDrafts] Making API call to getDrafts...");

      const result = await blogPostsApi.getDrafts(page, pageSize);
      console.log("📥 [useDrafts] API response received:", {
        dataLength: result.data?.length || 0,
        totalItems: result.pagination?.totalItems || 0,
        firstDraftTitle: result.data?.[0]?.title,
        firstDraftUpdatedAt: result.data?.[0]?.updatedAt,
        allDraftTitles: result.data?.map((d) => d.title),
      });

      setData(result.data || []);
      setPagination(
        result.pagination || {
          currentPage: 1,
          pageSize: 10,
          totalPages: 0,
          totalItems: 0,
          hasNextPage: false,
          hasPreviousPage: false,
        }
      );
      console.log("✅ [useDrafts] State updated successfully");
    } catch (err) {
      console.error("❌ [useDrafts] Error fetching drafts:", err);
      setError(err instanceof Error ? err.message : "Failed to load drafts");
    } finally {
      setLoading(false);
      isFetchingRef.current = false;
    }
  }, [page, pageSize]); // Only depend on page and pageSize

  useEffect(() => {
    fetchDrafts();
  }, [fetchDrafts]);

  return {
    data,
    pagination,
    loading,
    error,
    refetch: fetchDrafts, // Expose refetch function for manual refresh
  };
};

/**
 * Custom hook for fetching a single blog post by ID
 */
export const useBlogPost = (id: string | undefined) => {
  const [data, setData] = useState<BlogPost | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPost = useCallback(async () => {
    if (!id) {
      setError("No blog ID provided");
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const post = await BlogService.getPostById(id);
      setData(post);
      if (!post) {
        setError("Blog post not found");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load blog post");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchPost();
  }, [fetchPost]);

  return { data, loading, error, refetch: fetchPost };
};

/**
 * Custom hook for favorite/save functionality
 */
export const useFavorite = (
  blogId: string,
  initialIsFavorite: boolean = false
) => {
  const [isFavorite, setIsFavorite] = useState(initialIsFavorite);
  const [loading, setLoading] = useState(false);

  const toggleFavorite = useCallback(async () => {
    // Check if user is authenticated before attempting to toggle favorite
    if (!isAuthenticated()) {
      throw new Error("Please log in to save blogs to your favorites.");
    }

    try {
      setLoading(true);

      // Import here to avoid circular dependency
      const { savedBlogsApi } = await import("../services/savedBlogApi");

      if (isFavorite) {
        // Unsave the blog
        await savedBlogsApi.unsave(blogId);
        setIsFavorite(false);
      } else {
        // Save the blog
        await savedBlogsApi.save({ blogPostId: blogId });
        setIsFavorite(true);
      }
    } catch (error) {
      console.error("Error toggling favorite:", error);
      // Re-throw the error so the BlogCard can handle it
      throw error;
    } finally {
      setLoading(false);
    }
  }, [blogId, isFavorite]);

  // Check initial favorite status (only for authenticated users)
  useEffect(() => {
    const checkFavoriteStatus = async () => {
      // Skip favorite check for unauthenticated users
      if (!isAuthenticated()) {
        setIsFavorite(false);
        return;
      }

      try {
        const { savedBlogsApi } = await import("../services/savedBlogApi");
        const result = await savedBlogsApi.isBookmarked(blogId);
        setIsFavorite(result.isBookmarked);
      } catch (error) {
        console.error("Error checking favorite status:", error);
        // Keep the initial value if API call fails
        setIsFavorite(false);
      }
    };

    if (blogId) {
      checkFavoriteStatus();
    }
  }, [blogId]);

  return { isFavorite, loading, toggleFavorite };
};
