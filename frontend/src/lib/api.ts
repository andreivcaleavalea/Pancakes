import { API_CONFIG } from "@/utils/constants";

/**
 * Utility functions for making authenticated API calls in a stateless system.
 * The JWT token contains all user information needed for backend operations.
 */

interface ApiResponse<T = unknown> {
  data?: T;
  error?: string;
  status: number;
}

/**
 * Makes an authenticated API call with the JWT token from localStorage.
 * @param endpoint - API endpoint (relative to base URL)
 * @param options - Fetch options
 * @returns Promise with the API response
 */
export async function authenticatedFetch<T = unknown>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const baseUrl = API_CONFIG.USER_API_URL;
  const url = `${baseUrl}${
    endpoint.startsWith("/") ? endpoint : "/" + endpoint
  }`;

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

  // Prepare headers
  const headers = new Headers(options.headers);
  
  // Only set Content-Type to application/json if body is not FormData
  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });

    const status = response.status;

    // Handle authentication errors
    if (status === 401) {
      // Token is invalid or expired, clear session
      localStorage.removeItem("auth-session");
      window.location.href = "/login";
      return { status, error: "Authentication required" };
    }

    let data;
    const contentType = response.headers.get("content-type");

    if (contentType && contentType.includes("application/json")) {
      data = await response.json();
    } else {
      data = await response.text();
    }

    if (!response.ok) {
      return {
        status,
        error:
          typeof data === "object" && data?.message
            ? data.message
            : "Request failed",
      };
    }

    return { status, data };
  } catch (error) {
    console.error("API call failed:", error);
    return {
      status: 0,
      error: error instanceof Error ? error.message : "Network error",
    };
  }
}

/**
 * Gets the current user information from the backend using the JWT token.
 * @returns Promise with user data or error
 */
export async function getCurrentUser(): Promise<ApiResponse> {
  return authenticatedFetch("/auth/me");
}

/**
 * Validates the current JWT token with the backend.
 * @returns Promise with validation result
 */
export async function validateToken(): Promise<ApiResponse> {
  return authenticatedFetch("/auth/validate");
}

/**
 * Makes a GET request with authentication.
 * @param endpoint - API endpoint
 * @returns Promise with response data
 */
export async function authenticatedGet<T = unknown>(
  endpoint: string
): Promise<ApiResponse<T>> {
  return authenticatedFetch<T>(endpoint, { method: "GET" });
}

/**
 * Makes a POST request with authentication.
 * @param endpoint - API endpoint
 * @param body - Request body
 * @returns Promise with response data
 */
export async function authenticatedPost<T = unknown>(
  endpoint: string,
  body?: Record<string, unknown>
): Promise<ApiResponse<T>> {
  return authenticatedFetch<T>(endpoint, {
    method: "POST",
    body: body ? JSON.stringify(body) : undefined,
  });
}

/**
 * Makes a PUT request with authentication.
 * @param endpoint - API endpoint
 * @param body - Request body
 * @returns Promise with response data
 */
export async function authenticatedPut<T = unknown>(
  endpoint: string,
  body?: Record<string, unknown>
): Promise<ApiResponse<T>> {
  return authenticatedFetch<T>(endpoint, {
    method: "PUT",
    body: body ? JSON.stringify(body) : undefined,
  });
}

/**
 * Makes a DELETE request with authentication.
 * @param endpoint - API endpoint
 * @returns Promise with response data
 */
export async function authenticatedDelete<T = unknown>(
  endpoint: string
): Promise<ApiResponse<T>> {
  return authenticatedFetch<T>(endpoint, { method: "DELETE" });
}

/**
 * Checks if the user is currently authenticated by checking localStorage.
 * Note: This doesn't validate the token with the server.
 * @returns boolean indicating if user appears to be authenticated
 */
export function isAuthenticated(): boolean {
  const authSession = localStorage.getItem("auth-session");
  if (!authSession) return false;

  try {
    const session = JSON.parse(authSession);
    const expiresAt = new Date(session.expires);
    return expiresAt > new Date() && !!session.token;
  } catch {
    return false;
  }
}

/**
 * Gets the user information from localStorage without making an API call.
 * @returns User object if authenticated, null otherwise
 */
export function getLocalUser() {
  const authSession = localStorage.getItem("auth-session");
  if (!authSession) return null;

  try {
    const session = JSON.parse(authSession);
    return session.user || null;
  } catch {
    return null;
  }
}
