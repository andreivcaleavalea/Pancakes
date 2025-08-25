import React, {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
} from "react";
import { savedBlogsApi } from "@/services/savedBlogApi";
import type { SavedBlog } from "@/types/blog";

interface SavedBlogsContextType {
  savedBlogIds: Set<string>;
  savedBlogs: SavedBlog[];
  isLoading: boolean;
  isPostSaved: (blogPostId: string) => boolean;
  toggleSaved: (blogPostId: string) => Promise<void>;
  refreshSavedBlogs: () => Promise<void>;
}

const SavedBlogsContext = createContext<SavedBlogsContextType | undefined>(
  undefined
);

interface SavedBlogsProviderProps {
  children: React.ReactNode;
}

export const SavedBlogsProvider: React.FC<SavedBlogsProviderProps> = ({
  children,
}) => {
  const [savedBlogs, setSavedBlogs] = useState<SavedBlog[]>([]);
  const [savedBlogIds, setSavedBlogIds] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(false);

  const refreshSavedBlogs = useCallback(async () => {
    // Skip if user is not authenticated
    const authSession = localStorage.getItem("auth-session");
    if (!authSession) {
      console.log(
        "🔐 [SavedBlogsContext] No auth session - clearing saved blogs"
      );
      setSavedBlogs([]);
      setSavedBlogIds(new Set());
      return;
    }

    try {
      setIsLoading(true);
      console.log("🔄 [SavedBlogsContext] Fetching all saved blogs...");

      const blogs = await savedBlogsApi.getAll();
      const blogIds = new Set(blogs.map((blog) => blog.blogPostId));

      setSavedBlogs(blogs);
      setSavedBlogIds(blogIds);

      console.log("✅ [SavedBlogsContext] Saved blogs loaded:", {
        count: blogs.length,
        blogIds: Array.from(blogIds),
      });
    } catch (error) {
      console.error(
        "❌ [SavedBlogsContext] Error fetching saved blogs:",
        error
      );
      // Clear state on error (user might not be authenticated)
      setSavedBlogs([]);
      setSavedBlogIds(new Set());
    } finally {
      setIsLoading(false);
    }
  }, []);

  const isPostSaved = useCallback(
    (blogPostId: string): boolean => {
      return savedBlogIds.has(blogPostId);
    },
    [savedBlogIds]
  );

  const toggleSaved = useCallback(
    async (blogPostId: string) => {
      const authSession = localStorage.getItem("auth-session");
      if (!authSession) {
        throw new Error("Please log in to save blogs to your favorites.");
      }

      try {
        const isSaved = savedBlogIds.has(blogPostId);

        console.log(
          `🔄 [SavedBlogsContext] ${
            isSaved ? "Unsaving" : "Saving"
          } blog ${blogPostId}...`
        );

        if (isSaved) {
          // Unsave the blog
          await savedBlogsApi.unsave(blogPostId);

          // Update local state immediately for better UX
          setSavedBlogs((prev) =>
            prev.filter((blog) => blog.blogPostId !== blogPostId)
          );
          setSavedBlogIds((prev) => {
            const newSet = new Set(prev);
            newSet.delete(blogPostId);
            return newSet;
          });

          console.log("✅ [SavedBlogsContext] Blog unsaved successfully");
        } else {
          // Save the blog
          const savedBlog = await savedBlogsApi.save({ blogPostId });

          // Update local state immediately for better UX
          setSavedBlogs((prev) => [savedBlog, ...prev]);
          setSavedBlogIds((prev) => new Set([...prev, blogPostId]));

          console.log("✅ [SavedBlogsContext] Blog saved successfully");
        }
      } catch (error) {
        console.error(
          "❌ [SavedBlogsContext] Error toggling saved status:",
          error
        );
        throw error;
      }
    },
    [savedBlogIds]
  );

  // Load saved blogs on mount and when auth changes
  useEffect(() => {
    refreshSavedBlogs();

    // Listen for auth changes
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === "auth-session") {
        console.log(
          "🔄 [SavedBlogsContext] Auth session changed - refreshing saved blogs"
        );
        refreshSavedBlogs();
      }
    };

    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, [refreshSavedBlogs]);

  const value: SavedBlogsContextType = {
    savedBlogIds,
    savedBlogs,
    isLoading,
    isPostSaved,
    toggleSaved,
    refreshSavedBlogs,
  };

  return (
    <SavedBlogsContext.Provider value={value}>
      {children}
    </SavedBlogsContext.Provider>
  );
};

export const useSavedBlogsContext = (): SavedBlogsContextType => {
  const context = useContext(SavedBlogsContext);
  if (context === undefined) {
    throw new Error(
      "useSavedBlogsContext must be used within a SavedBlogsProvider"
    );
  }
  return context;
};

export default SavedBlogsContext;
