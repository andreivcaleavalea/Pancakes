import { apiRequest } from "@/utils/api";
import type { Comment, CreateCommentDto } from "@/types/comment";

const ENDPOINTS = {
  COMMENTS: "/api/comments",
} as const;

// Comments API
export const commentsApi = {
  // Get comments for a specific blog post
  getByBlogPostId: async (blogPostId: string): Promise<Comment[]> => {
    return apiRequest<Comment[]>(`${ENDPOINTS.COMMENTS}/blog/${blogPostId}`);
  },

  // Get comment by ID
  getById: async (id: string): Promise<Comment> => {
    return apiRequest<Comment>(`${ENDPOINTS.COMMENTS}/${id}`);
  },

  // Create new comment
  create: async (comment: CreateCommentDto): Promise<Comment> => {
    return apiRequest<Comment>(ENDPOINTS.COMMENTS, {
      method: "POST",
      body: JSON.stringify(comment),
    });
  },

  // Update comment
  update: async (id: string, comment: CreateCommentDto): Promise<Comment> => {
    return apiRequest<Comment>(`${ENDPOINTS.COMMENTS}/${id}`, {
      method: "PUT",
      body: JSON.stringify(comment),
    });
  },

  // Delete comment
  delete: async (id: string): Promise<void> => {
    return apiRequest<void>(`${ENDPOINTS.COMMENTS}/${id}`, {
      method: "DELETE",
    });
  },
};
