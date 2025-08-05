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

  useEffect(() => {
    // Check for existing session in localStorage
    const storedSession = localStorage.getItem("auth-session");
    if (storedSession) {
      try {
        const parsedSession = JSON.parse(storedSession);

        // Check if session is still valid
        if (new Date(parsedSession.expires) > new Date()) {
          setSession(parsedSession);
        } else {
          localStorage.removeItem("auth-session");
        }
      } catch (error) {
        console.error("❌ AuthContext: Error parsing stored session:", error);
        localStorage.removeItem("auth-session");
      }
    }

    setLoading(false);
  }, []);

  const signIn = async (provider: string) => {
    try {
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
      }
    } catch (error) {
      console.error("❌ AuthContext: Error calling logout endpoint:", error);
    } finally {
      setSession(null);
      localStorage.removeItem("auth-session");
      sessionStorage.removeItem("oauth-provider");
      sessionStorage.removeItem("oauth-state");
    }
  };

  const updateSession = (newSession: Session) => {
    setSession(newSession);
    localStorage.setItem("auth-session", JSON.stringify(newSession));
  };

  const updateUser = (userData: Partial<User>) => {
    if (session) {
      const updatedUser = { ...session.user, ...userData };
      const updatedSession = { ...session, user: updatedUser };
      setSession(updatedSession);
      localStorage.setItem("auth-session", JSON.stringify(updatedSession));
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
