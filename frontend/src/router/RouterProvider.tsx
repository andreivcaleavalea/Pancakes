import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";

export type PageType = "home" | "login";
export type LoginMode = "signin" | "register";

interface RouterContextType {
  currentPage: PageType;
  loginMode: LoginMode;
  navigate: (page: PageType, mode?: LoginMode) => void;
}

const RouterContext = createContext<RouterContextType | undefined>(undefined);

export const useRouter = (): RouterContextType => {
  const context = useContext(RouterContext);
  if (!context) {
    throw new Error("useRouter must be used within a RouterProvider");
  }
  return context;
};

interface RouterProviderProps {
  children: ReactNode;
}

export const RouterProvider: React.FC<RouterProviderProps> = ({ children }) => {
  const [currentPage, setCurrentPage] = useState<PageType>("home");
  const [loginMode, setLoginMode] = useState<LoginMode>("signin");

  useEffect(() => {
    const path = window.location.pathname;

    // Handle auth callback
    if (path === "/auth/callback") {
      return; // Let AuthCallback component handle this
    }

    if (path === "/login") {
      setCurrentPage("login");
    } else {
      setCurrentPage("home");
    }

    const params = new URLSearchParams(window.location.search);
    const mode = params.get("mode");
    if (mode === "register" || mode === "signin") {
      setLoginMode(mode);
    }
  }, []);

  const navigate = (page: PageType, mode?: LoginMode) => {
    setCurrentPage(page);
    if (mode) {
      setLoginMode(mode);
    }

    // Update URL
    let url = `/${page === "home" ? "" : page}`;
    if (page === "login" && mode) {
      url += `?mode=${mode}`;
    }

    window.history.pushState({}, "", url);
  };

  return (
    <RouterContext.Provider value={{ currentPage, loginMode, navigate }}>
      {children}
    </RouterContext.Provider>
  );
};

export default RouterProvider;
