import { authenticatedBlogRequest } from "../utils/blogApi";
import type { SavedBlog, CreateSavedBlogDto } from "@/types/blog";

const ENDPOINTS = {
  SAVED_BLOGS: "/api/savedblogs",
} as const;

// Saved Blogs API
export const savedBlogsApi = {
  // Get all saved blogs for current user
  getAll: async (): Promise<SavedBlog[]> => {
    const result = await authenticatedBlogRequest<SavedBlog[]>(
      ENDPOINTS.SAVED_BLOGS
    );
    return result;
  },

  // Save a blog post
  save: async (createDto: CreateSavedBlogDto): Promise<SavedBlog> => {
    return authenticatedBlogRequest<SavedBlog>(ENDPOINTS.SAVED_BLOGS, {
      method: "POST",
      body: JSON.stringify(createDto),
    });
  },

  // Unsave a blog post
  unsave: async (blogPostId: string): Promise<void> => {
    return authenticatedBlogRequest<void>(
      `${ENDPOINTS.SAVED_BLOGS}/${blogPostId}`,
      {
        method: "DELETE",
      }
    );
  },

  // Check if a blog post is saved (DEPRECATED - use context instead for better performance)
  isBookmarked: async (
    blogPostId: string
  ): Promise<{ isBookmarked: boolean }> => {
    console.warn(
      "⚠️ [SavedBlogsAPI] DEPRECATED: Individual check call for blog",
      blogPostId,
      "- use SavedBlogsContext instead for better performance!"
    );
    return authenticatedBlogRequest<{ isBookmarked: boolean }>(
      `${ENDPOINTS.SAVED_BLOGS}/check/${blogPostId}`
    );
  },
};
