import { publicBlogRequest } from "@/utils/blogApi";

const ENDPOINTS = {
  TAGS_POPULAR: "/api/tags/popular",
  TAGS_SEARCH: "/api/tags/search",
} as const;

export const tagsApi = {
  // Get popular tags
  getPopular: async (limit: number = 20): Promise<string[]> => {
    const res = await publicBlogRequest<string[]>(
      `${ENDPOINTS.TAGS_POPULAR}?limit=${limit}`
    );
    return Array.isArray(res) ? res : [];
  },

  // Search tags
  search: async (query: string, limit: number = 10): Promise<string[]> => {
    if (!query || query.length < 2) {
      return [];
    }

    const res = await publicBlogRequest<string[]>(
      `${ENDPOINTS.TAGS_SEARCH}?query=${encodeURIComponent(
        query
      )}&limit=${limit}`
    );
    return Array.isArray(res) ? res : [];
  },
};
