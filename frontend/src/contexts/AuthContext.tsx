import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";

interface User {
  id: string;
  name: string;
  email: string;
  image: string;
  provider: "google" | "github" | "facebook";
}

interface Session {
  user: User;
  expires: string;
  token: string;
}

interface AuthContextType {
  session: Session | null;
  user: User | null;
  isAuthenticated: boolean;
  loading: boolean;
  signIn: (provider: string) => Promise<void>;
  signOut: () => void;
  updateSession: (session: Session) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

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
        console.error("Error parsing stored session:", error);
        localStorage.removeItem("auth-session");
      }
    }
    setLoading(false);
  }, []);

  const signIn = async (provider: string) => {
    try {
      const authUrl = createOAuthUrl(provider);
      sessionStorage.setItem("oauth-provider", provider);
      window.location.href = authUrl;
    } catch (error) {
      console.error("Sign in error:", error);
      throw error;
    }
  };

  const signOut = () => {
    setSession(null);
    localStorage.removeItem("auth-session");
    sessionStorage.removeItem("oauth-provider");
    sessionStorage.removeItem("oauth-state");
  };

  const updateSession = (newSession: Session) => {
    setSession(newSession);
    localStorage.setItem("auth-session", JSON.stringify(newSession));
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
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

function createOAuthUrl(provider: string): string {
  const redirectUri = `http://localhost:5141/auth/callback`;
  const state = Math.random().toString(36).substring(2, 15);

  sessionStorage.setItem("oauth-state", state);

  switch (provider.toLowerCase()) {
    case "google":
      return (
        `https://accounts.google.com/o/oauth2/v2/auth?` +
        `client_id=${import.meta.env.VITE_GOOGLE_CLIENT_ID}&` +
        `redirect_uri=${encodeURIComponent(redirectUri)}&` +
        `response_type=code&` +
        `scope=openid email profile&` +
        `state=${state}`
      );

    case "github":
      return (
        `https://github.com/login/oauth/authorize?` +
        `client_id=${import.meta.env.VITE_GITHUB_CLIENT_ID}&` +
        `redirect_uri=${encodeURIComponent(redirectUri)}&` +
        `scope=user:email&` +
        `state=${state}`
      );

    case "facebook":
      return (
        `https://www.facebook.com/v18.0/dialog/oauth?` +
        `client_id=${import.meta.env.VITE_FACEBOOK_APP_ID}&` +
        `redirect_uri=${encodeURIComponent(redirectUri)}&` +
        `scope=email,public_profile&` +
        `state=${state}`
      );

    default:
      throw new Error(`Unsupported provider: ${provider}`);
  }
}

export default AuthContext;
