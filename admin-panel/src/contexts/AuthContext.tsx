import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";
import { adminApi, AdminUser } from "../services/api";
import {
  setAuthErrorHandler,
  clearAuthErrorHandler,
} from "../utils/authErrorHandler";

interface AuthContextType {
  user: AdminUser | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  forceLogout: () => void;
  isLoading: boolean;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<AdminUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const forceLogout = () => {
    // For when we need to immediately log out due to authentication failure
    setUser(null);
    window.location.href = "/login";
  };

  useEffect(() => {
    // Register the auth error handler for global API errors
    setAuthErrorHandler(forceLogout);

    // Add a small delay to ensure cookies are properly loaded
    const timer = setTimeout(() => {
      validateTokenWithRetry();
    }, 100);

    return () => {
      clearTimeout(timer);
      clearAuthErrorHandler();
    };
  }, []);

  const validateTokenWithRetry = async (retries = 3) => {
    for (let attempt = 1; attempt <= retries; attempt++) {
      try {
        const response = await adminApi.getCurrentAdmin();
        setUser(response.data);
        setIsLoading(false);
        return; // Success, exit retry loop
      } catch (error: any) {
        console.log(
          `Auth validation attempt ${attempt}/${retries}:`,
          error.message
        );

        // On the last attempt, handle the error
        if (attempt === retries) {
          // Only set user to null for actual authentication failures
          if (
            error.response?.status === 401 ||
            error.response?.status === 403
          ) {
            console.log("Authentication failed - user not authenticated");
            setUser(null);
          } else {
            // For network errors or server issues, don't clear the user
            // This prevents redirects on temporary issues
            console.log(
              "Network/server error during auth check - maintaining auth state"
            );
          }
          setIsLoading(false);
        } else {
          // Wait before retrying (exponential backoff)
          await new Promise((resolve) => setTimeout(resolve, attempt * 500));
        }
      }
    }
  };

  const login = async (email: string, password: string) => {
    try {
      const response = await adminApi.login(email, password);
      const { adminUser } = response.data;

      setUser(adminUser);
    } catch (error) {
      if (error instanceof Error) {
        throw new Error(error.message);
      } else if (
        typeof error === "object" &&
        error !== null &&
        "response" in error
      ) {
        const axiosError = error as any;
        if (axiosError.response?.data?.message) {
          throw new Error(axiosError.response.data.message);
        }
      }

      throw new Error(
        "Login failed. Please check your connection and try again."
      );
    }
  };

  const logout = async () => {
    try {
      await adminApi.logout();
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      setUser(null);
      // Redirect to login after logout
      window.location.href = "/login";
    }
  };

  const value = {
    user,
    login,
    logout,
    forceLogout,
    isLoading,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
