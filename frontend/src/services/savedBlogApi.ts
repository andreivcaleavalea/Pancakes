import { authenticatedBlogRequest } from "../utils/blogApi";
import type { SavedBlog, CreateSavedBlogDto } from "@/types/blog";

const ENDPOINTS = {
  SAVED_BLOGS: "/api/savedblogs",
} as const;

// Saved Blogs API
export const savedBlogsApi = {
  // Get all saved blogs for current user
  getAll: async (): Promise<SavedBlog[]> => {
    return authenticatedBlogRequest<SavedBlog[]>(ENDPOINTS.SAVED_BLOGS);
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

  // Check if a blog post is saved
  isBookmarked: async (
    blogPostId: string
  ): Promise<{ isBookmarked: boolean }> => {
    return authenticatedBlogRequest<{ isBookmarked: boolean }>(
      `${ENDPOINTS.SAVED_BLOGS}/check/${blogPostId}`
    );
  },
};
