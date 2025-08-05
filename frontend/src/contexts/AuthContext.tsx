import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";
import { initiateOAuthLogin, type OAuthProvider } from "../lib/auth";

interface User {
  id: string;
  name: string;
  email: string;
  image: string;
  provider: "google" | "github";
}

interface Session {
  user: User;
  token: string;
  expires: string;
}

interface AuthContextType {
  session: Session | null;
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  signIn: (provider: string) => Promise<void>;
  signOut: () => Promise<void>;
  updateSession: (session: Session) => void;
  updateUser: (userData: Partial<User>) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [session, setSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(true);

  const user = session?.user || null;
  const isAuthenticated = !!session;

  // Log authentication state changes
  useEffect(() => {
    console.log("🔐 AuthContext: Authentication state changed:", {
      isAuthenticated,
      userId: user?.id,
      userName: user?.name,
      provider: user?.provider,
      loading,
    });
  }, [isAuthenticated, user?.id, user?.name, user?.provider, loading]);

  useEffect(() => {
    console.log("🔐 AuthContext: Checking for existing session...");

    // Check for existing session in localStorage
    const storedSession = localStorage.getItem("auth-session");
    if (storedSession) {
      try {
        const parsedSession = JSON.parse(storedSession);
        console.log("🔐 AuthContext: Found stored session:", {
          userId: parsedSession.user?.id,
          userName: parsedSession.user?.name,
          provider: parsedSession.user?.provider,
          expires: parsedSession.expires,
          isValid: new Date(parsedSession.expires) > new Date(),
        });

        // Check if session is still valid
        if (new Date(parsedSession.expires) > new Date()) {
          setSession(parsedSession);
          console.log("✅ AuthContext: Session restored successfully");
        } else {
          console.log("⏰ AuthContext: Session expired, removing...");
          localStorage.removeItem("auth-session");
        }
      } catch (error) {
        console.error("❌ AuthContext: Error parsing stored session:", error);
        localStorage.removeItem("auth-session");
      }
    } else {
      console.log("🔐 AuthContext: No stored session found");
    }

    setLoading(false);
    console.log("🔐 AuthContext: Initialization complete");
  }, []);

  const signIn = async (provider: string) => {
    try {
      console.log(`🔐 AuthContext: Initiating sign-in with ${provider}...`);
      // Use the simplified OAuth system from auth.ts
      initiateOAuthLogin(provider.toLowerCase() as OAuthProvider);
    } catch (error) {
      console.error("❌ AuthContext: Sign in error:", error);
      throw error;
    }
  };

  const signOut = async () => {
    console.log("🔐 AuthContext: Signing out user...", {
      userId: session?.user?.id,
      userName: session?.user?.name,
    });

    try {
      // Optionally call backend logout endpoint for logging/analytics
      // In a stateless system, the backend doesn't need to clear any state
      if (session?.token) {
        await fetch(`${import.meta.env.VITE_API_BASE_URL}/auth/logout`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${session.token}`,
          },
        });
        console.log("🔐 AuthContext: Backend logout endpoint called");
      }
    } catch (error) {
      console.error("❌ AuthContext: Error calling logout endpoint:", error);
    } finally {
      setSession(null);
      localStorage.removeItem("auth-session");
      sessionStorage.removeItem("oauth-provider");
      sessionStorage.removeItem("oauth-state");
      console.log("✅ AuthContext: User signed out successfully");
    }
  };

  const updateSession = (newSession: Session) => {
    console.log("🔐 AuthContext: Updating session:", {
      userId: newSession.user?.id,
      userName: newSession.user?.name,
      provider: newSession.user?.provider,
      expires: newSession.expires,
    });

    setSession(newSession);
    localStorage.setItem("auth-session", JSON.stringify(newSession));
    console.log("✅ AuthContext: Session updated successfully");
  };

  const updateUser = (userData: Partial<User>) => {
    if (session) {
      console.log("🔐 AuthContext: Updating user data:", userData);
      const updatedUser = { ...session.user, ...userData };
      const updatedSession = { ...session, user: updatedUser };
      setSession(updatedSession);
      localStorage.setItem("auth-session", JSON.stringify(updatedSession));
      console.log("✅ AuthContext: User data updated successfully");
    } else {
      console.log("⚠️ AuthContext: Cannot update user - no active session");
    }
  };

  return (
    <AuthContext.Provider
      value={{
        session,
        user,
        isAuthenticated,
        loading,
        signIn,
        signOut,
        updateSession,
        updateUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
