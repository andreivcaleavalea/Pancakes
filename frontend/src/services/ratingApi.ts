import { authenticatedBlogRequest } from "@/utils/blogApi";
import type {
  CreatePostRatingDto,
  PostRating,
  PostRatingStats,
} from "@/types/rating";

// Cache removed - backend handles caching professionally

export const postRatingApi = {
  // Get rating statistics for a blog post - backend handles caching
  getStats: async (blogPostId: string): Promise<PostRatingStats> => {
    return await authenticatedBlogRequest<PostRatingStats>(
      `/api/postratings/stats/${blogPostId}`
    );
  },

  // Create or update a rating
  createOrUpdate: async (
    data: Omit<CreatePostRatingDto, "userId">
  ): Promise<PostRating> => {
    // Get token directly (same as console test that worked)
    const authSession = localStorage.getItem("auth-session");
    if (!authSession) {
      throw new Error("No authentication session found");
    }

    const session = JSON.parse(authSession);
    const token = session.token;

    if (!token) {
      throw new Error("No authentication token found");
    }

    const apiBase = (await import("@/utils/constants")).API_CONFIG.BLOG_API_URL;
    const url = `${apiBase}/api/postratings`;
    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const result = await response.json();

    return result;
  },

  // Delete a rating
  delete: async (blogPostId: string): Promise<void> => {
    const result = await authenticatedBlogRequest<void>(
      `/api/postratings/${blogPostId}`,
      {
        method: "DELETE",
      }
    );

    return result;
  },
};
