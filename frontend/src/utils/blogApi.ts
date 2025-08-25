import { API_CONFIG, ERROR_MESSAGES } from "./constants";

// API Error handling
export class ApiError extends Error {
  public status: number;
  public statusText: string;

  constructor(message: string, status: number, statusText: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.statusText = statusText;
  }
}

// Public API request handler for reading blog content (no authentication required)
export const publicBlogRequest = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  const url = `${API_CONFIG.BLOG_API_URL}${endpoint}`;

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), API_CONFIG.TIMEOUT);

  const defaultHeaders: Record<string, string> = {
    "Content-Type": "application/json",
  };

  const defaultOptions: RequestInit = {
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
    signal: controller.signal,
    ...options,
  };

  try {
    const response = await fetch(url, defaultOptions);
    clearTimeout(timeoutId);

    if (!response.ok) {
      let errorMessage: string = ERROR_MESSAGES.GENERIC;

      switch (response.status) {
        case 404:
          errorMessage = ERROR_MESSAGES.NOT_FOUND;
          break;
        case 401:
          errorMessage = "Authentication required for this action.";
          break;
        case 403:
          errorMessage =
            "Access forbidden. You do not have permission to perform this action.";
          break;
        case 422:
          errorMessage = ERROR_MESSAGES.VALIDATION;
          break;
        default:
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      }

      throw new ApiError(errorMessage, response.status, response.statusText);
    }

    // Handle empty responses (like DELETE operations)
    const contentType = response.headers.get("content-type");
    if (!contentType || !contentType.includes("application/json")) {
      return {} as T;
    }

    const data = await response.json();
    return data as T;
  } catch (error) {
    clearTimeout(timeoutId);

    if (error instanceof ApiError) {
      throw error;
    }

    if (error instanceof Error) {
      if (error.name === "AbortError") {
        throw new ApiError("Request timeout", 408, "Request Timeout");
      }

      // Network or other errors
      throw new ApiError(ERROR_MESSAGES.NETWORK, 0, error.message);
    }

    throw new ApiError(ERROR_MESSAGES.GENERIC, 500, "Unknown error");
  }
};

// Authenticated API request handler specifically for Blog Service
export const authenticatedBlogRequest = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  const url = `${API_CONFIG.BLOG_API_URL}${endpoint}`;
  // Get token from localStorage
  const authSession = localStorage.getItem("auth-session");
  let token = "";
  let userId = null;

  console.log("🔐 [AuthBlogAPI] Making authenticated request:", { 
    endpoint, 
    method: options.method || 'GET',
    hasAuthSession: !!authSession 
  });

  if (authSession) {
    try {
      const session = JSON.parse(authSession);
      token = session.token || "";
      userId = session.user?.id || session.userId || 'unknown';
      
      console.log("👤 [AuthBlogAPI] Auth session found:", { 
        hasToken: !!token, 
        tokenLength: token.length,
        userId,
        userInfo: session.user ? {
          id: session.user.id,
          name: session.user.name,
          email: session.user.email
        } : 'No user info'
      });
    } catch (error) {
      console.error("❌ [AuthBlogAPI] Error parsing auth session:", error);
    }
  } else {
    console.log("⚠️ [AuthBlogAPI] No auth session found in localStorage");
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), API_CONFIG.TIMEOUT);

  const defaultHeaders: Record<string, string> = {
    "Content-Type": "application/json",
  };

  // Add authentication header if token exists
  if (token) {
    defaultHeaders["Authorization"] = `Bearer ${token}`;
  }

  const defaultOptions: RequestInit = {
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
    signal: controller.signal,
    ...options,
  };

  try {
    console.log("📤 [AuthBlogAPI] Sending request:", { 
      url, 
      hasAuthHeader: !!defaultHeaders["Authorization"],
      headers: Object.keys(defaultHeaders) 
    });

    const response = await fetch(url, defaultOptions);
    clearTimeout(timeoutId);

    console.log("📥 [AuthBlogAPI] Response received:", { 
      status: response.status,
      statusText: response.statusText,
      ok: response.ok,
      url: response.url
    });

    if (!response.ok) {
      let errorMessage: string = ERROR_MESSAGES.GENERIC;

      switch (response.status) {
        case 404:
          errorMessage = ERROR_MESSAGES.NOT_FOUND;
          break;
        case 401:
          errorMessage = "Authentication required. Please log in.";
          console.log("🚫 [AuthBlogAPI] 401 Unauthorized - clearing auth session");
          // Clear invalid session
          localStorage.removeItem("auth-session");
          window.location.href = "/login";
          break;
        case 403:
          errorMessage =
            "Access forbidden. You do not have permission to perform this action.";
          break;
        case 422:
          errorMessage = ERROR_MESSAGES.VALIDATION;
          break;
        default:
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
      }

      console.error("❌ [AuthBlogAPI] Request failed:", { 
        status: response.status, 
        statusText: response.statusText, 
        errorMessage 
      });
      throw new ApiError(errorMessage, response.status, response.statusText);
    }

    // Handle empty responses (like DELETE operations)
    const contentType = response.headers.get("content-type");
    if (!contentType || !contentType.includes("application/json")) {
      console.log("📄 [AuthBlogAPI] Non-JSON response, returning empty object");
      return {} as T;
    }

    const data = await response.json();
    console.log("✅ [AuthBlogAPI] Request successful:", { 
      endpoint,
      dataType: Array.isArray(data) ? `array[${data.length}]` : typeof data,
      userId
    });
    
    return data as T;
  } catch (error) {
    clearTimeout(timeoutId);

    if (error instanceof ApiError) {
      throw error;
    }

    if (error instanceof Error) {
      if (error.name === "AbortError") {
        throw new ApiError("Request timeout", 408, "Request Timeout");
      }

      // Network or other errors
      throw new ApiError(ERROR_MESSAGES.NETWORK, 0, error.message);
    }

    throw new ApiError(ERROR_MESSAGES.GENERIC, 500, "Unknown error");
  }
};
