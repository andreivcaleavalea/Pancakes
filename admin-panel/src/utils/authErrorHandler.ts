// Global authentication error handler
// This is used by API interceptors to handle 401/403 errors gracefully

let authErrorHandler: (() => void) | null = null;

export const setAuthErrorHandler = (handler: () => void) => {
  authErrorHandler = handler;
};

export const handleAuthError = () => {
  if (authErrorHandler) {
    authErrorHandler();
  } else {
    // Fallback if auth context isn't available
    console.warn("No auth error handler set, redirecting to login");
    window.location.href = "/login";
  }
};

export const clearAuthErrorHandler = () => {
  authErrorHandler = null;
};
