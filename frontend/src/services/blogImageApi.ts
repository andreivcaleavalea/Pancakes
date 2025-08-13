import { API_CONFIG } from "@/utils/constants";

const BLOG_SERVICE_URL = API_CONFIG.BLOG_API_URL;

interface BlogImageUploadResponse {
  imagePath: string;
  imageUrl: string;
  message: string;
}

interface BlogImageDeleteResponse {
  message: string;
}

// Helper function to get auth headers following the existing pattern
const getAuthHeaders = (): Record<string, string> => {
  const authSession = localStorage.getItem("auth-session");
  const headers: Record<string, string> = {};

  if (authSession) {
    try {
      const session = JSON.parse(authSession);
      const token = session.token || "";
      if (token) {
        headers["Authorization"] = `Bearer ${token}`;
      }
    } catch (error) {
      console.error("Error parsing auth session:", error);
    }
  }

  return headers;
};

const handleResponse = async (response: Response) => {
  if (!response.ok) {
    let errorMessage = `HTTP error! status: ${response.status}`;

    try {
      const errorData = await response.text();
      errorMessage = errorData || errorMessage;
    } catch {
      // If we can't parse the error, use the default message
    }

    if (response.status === 401) {
      // Clear invalid session and redirect to login
      localStorage.removeItem("auth-session");
      window.location.href = "/login";
    }

    throw new Error(errorMessage);
  }
  return response.json();
};

export const blogImageApi = {
  upload: async (formData: FormData): Promise<BlogImageUploadResponse> => {
    const response = await fetch(`${BLOG_SERVICE_URL}/api/blogimages/upload`, {
      method: "POST",
      headers: {
        // Don't set Content-Type for FormData - browser will set it with boundary
        ...getAuthHeaders(),
      },
      body: formData,
    });

    return handleResponse(response);
  },

  delete: async (filename: string): Promise<BlogImageDeleteResponse> => {
    const response = await fetch(
      `${BLOG_SERVICE_URL}/api/blogimages/${filename}`,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          ...getAuthHeaders(),
        },
      }
    );

    return handleResponse(response);
  },

  getImageUrl: (imagePath: string): string => {
    if (imagePath.startsWith("http")) {
      return imagePath; // Already a full URL
    }
    return `${BLOG_SERVICE_URL}/${imagePath}`;
  },
};
