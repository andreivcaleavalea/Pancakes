import { authenticatedBlogRequest } from "@/utils/blogApi";
import type { Comment, CreateCommentDto } from "@/types/comment";

const ENDPOINTS = {
  COMMENTS: "/api/comments",
} as const;

// Comments API
export const commentsApi = {
  // Get comments for a specific blog post
  getByBlogPostId: async (blogPostId: string): Promise<Comment[]> => {
    return authenticatedBlogRequest<Comment[]>(
      `${ENDPOINTS.COMMENTS}/blog/${blogPostId}`
    );
  },

  // Get comment by ID
  getById: async (id: string): Promise<Comment> => {
    return authenticatedBlogRequest<Comment>(`${ENDPOINTS.COMMENTS}/${id}`);
  },

  // Create new comment (requires authentication)
  create: async (comment: CreateCommentDto): Promise<Comment> => {
    return authenticatedBlogRequest<Comment>(ENDPOINTS.COMMENTS, {
      method: "POST",
      body: JSON.stringify(comment),
    });
  },

  // Update comment (requires authentication)
  update: async (id: string, comment: CreateCommentDto): Promise<Comment> => {
    return authenticatedBlogRequest<Comment>(`${ENDPOINTS.COMMENTS}/${id}`, {
      method: "PUT",
      body: JSON.stringify(comment),
    });
  },

  // Delete comment (requires authentication)
  delete: async (id: string): Promise<void> => {
    return authenticatedBlogRequest<void>(`${ENDPOINTS.COMMENTS}/${id}`, {
      method: "DELETE",
    });
  },
};
