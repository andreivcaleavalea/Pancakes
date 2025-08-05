import { authenticatedBlogRequest } from "@/utils/blogApi";
import { API_CONFIG } from "@/utils/constants";
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
  // Returns updated comment if soft deleted, null if hard deleted
  delete: async (id: string): Promise<Comment | null> => {
    // Get token from localStorage
    const authSession = localStorage.getItem("auth-session");
    let token = "";

    if (authSession) {
      try {
        const session = JSON.parse(authSession);
        token = session.token || "";
      } catch (error) {
        console.error("Error parsing auth session:", error);
      }
    }

    if (!token) {
      throw new Error("Authentication required. Please log in.");
    }

    const fullUrl = `${API_CONFIG.BLOG_API_URL}${ENDPOINTS.COMMENTS}/${id}`;

    const response = await fetch(fullUrl, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `HTTP error! status: ${response.status}`);
    }

    // If 204 No Content, it was hard deleted
    if (response.status === 204) {
      return null;
    }

    // If 200 OK, it was soft deleted, return the updated comment
    return response.json();
  },
};
