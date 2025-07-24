import { useState, useEffect, useCallback } from "react";
import { savedBlogsApi } from "../services/savedBlogApi";
import type { SavedBlog, CreateSavedBlogDto } from "@/types/blog";

interface UseSavedBlogsState {
  savedBlogs: SavedBlog[];
  loading: boolean;
  error: string | null;
  saveBlog: (createDto: CreateSavedBlogDto) => Promise<void>;
  unsaveBlog: (blogPostId: string) => Promise<void>;
  isBookmarked: (blogPostId: string) => Promise<boolean>;
  refetch: () => Promise<void>;
}

export const useSavedBlogs = (): UseSavedBlogsState => {
  const [savedBlogs, setSavedBlogs] = useState<SavedBlog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchSavedBlogs = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const blogs = await savedBlogsApi.getAll();
      setSavedBlogs(blogs);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch saved blogs";
      setError(errorMessage);
      console.error("Error fetching saved blogs:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  const saveBlog = useCallback(async (createDto: CreateSavedBlogDto) => {
    try {
      const savedBlog = await savedBlogsApi.save(createDto);
      setSavedBlogs((prev) => [savedBlog, ...prev]);
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to save blog"
      );
    }
  }, []);

  const unsaveBlog = useCallback(async (blogPostId: string) => {
    try {
      await savedBlogsApi.unsave(blogPostId);
      setSavedBlogs((prev) =>
        prev.filter((sb) => sb.blogPostId !== blogPostId)
      );
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to unsave blog"
      );
    }
  }, []);

  const isBookmarked = useCallback(
    async (blogPostId: string): Promise<boolean> => {
      try {
        const result = await savedBlogsApi.isBookmarked(blogPostId);
        return result.isBookmarked;
      } catch (err) {
        console.error("Error checking bookmark status:", err);
        return false;
      }
    },
    []
  );

  useEffect(() => {
    fetchSavedBlogs();
  }, [fetchSavedBlogs]);

  return {
    savedBlogs,
    loading,
    error,
    saveBlog,
    unsaveBlog,
    isBookmarked,
    refetch: fetchSavedBlogs,
  };
};
