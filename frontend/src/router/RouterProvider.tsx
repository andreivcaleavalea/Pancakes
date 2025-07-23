import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";

export type PageType =
  | "home"
  | "login"
  | "create-blog"
  | "blog-view"
  | "edit-blog";
export type LoginMode = "signin" | "register";

interface RouterContextType {
  currentPage: PageType;
  loginMode: LoginMode;
  blogId?: string;
  navigate: (page: PageType, mode?: LoginMode, blogId?: string) => void;
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
  const [blogId, setBlogId] = useState<string | undefined>();

  useEffect(() => {
    const handleRouteChange = () => {
      const path = window.location.pathname;

      // Handle auth callback
      if (path === "/auth/callback") {
        return; // Let AuthCallback component handle this
      }

      if (path === "/login") {
        setCurrentPage("login");
      } else if (path === "/create-blog") {
        setCurrentPage("create-blog");
      } else if (path.startsWith("/edit-blog/")) {
        setCurrentPage("edit-blog");
        setBlogId(path.replace("/edit-blog/", ""));
      } else if (path.startsWith("/blog/")) {
        setCurrentPage("blog-view");
        setBlogId(path.replace("/blog/", ""));
      } else {
        setCurrentPage("home");
      }

      const params = new URLSearchParams(window.location.search);
      const mode = params.get("mode");
      if (mode === "register" || mode === "signin") {
        setLoginMode(mode);
      }
    };

    // Handle initial route
    handleRouteChange();

    // Listen for browser back/forward navigation
    const handlePopState = () => {
      handleRouteChange();
    };

    window.addEventListener("popstate", handlePopState);

    return () => {
      window.removeEventListener("popstate", handlePopState);
    };
  }, []);

  const navigate = (page: PageType, mode?: LoginMode, blogIdParam?: string) => {
    setCurrentPage(page);
    if (mode) {
      setLoginMode(mode);
    }
    if (blogIdParam) {
      setBlogId(blogIdParam);
    }

    // Update URL
    let url = `/${page === "home" ? "" : page}`;
    if (page === "login" && mode) {
      url += `?mode=${mode}`;
    } else if (page === "blog-view" && blogIdParam) {
      url = `/blog/${blogIdParam}`;
    } else if (page === "edit-blog" && blogIdParam) {
      url = `/edit-blog/${blogIdParam}`;
    }

    window.history.pushState({}, "", url);
  };

  return (
    <RouterContext.Provider
      value={{ currentPage, loginMode, blogId, navigate }}
    >
      {children}
    </RouterContext.Provider>
  );
};

export default RouterProvider;
