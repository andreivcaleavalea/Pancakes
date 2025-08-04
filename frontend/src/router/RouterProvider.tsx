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
  | "banned"
  | "create-blog"
  | "blog-view"
  | "edit-blog"
  | "saved"
  | "friends"
  | "profile"
  | "personal-page" 
  | "public";
export type LoginMode = "signin" | "register";

interface RouterContextType {
  currentPage: PageType;
  loginMode: LoginMode;
  publicSlug?: string;
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
  const [publicSlug, setPublicSlug] = useState<string | undefined>();

  const updatePageFromPath = () => {
    const path = window.location.pathname;

    // Handle auth callback
    if (path === "/auth/callback") {
      return; // Let AuthCallback component handle this
    }

    // Handle public pages
    if (path.startsWith("/public/")) {
      const slug = path.replace("/public/", "");
      setCurrentPage("public");
      setPublicSlug(slug);
      return;
    }

    // Handle blog pages
    if (path.startsWith("/edit-blog/")) {
      setCurrentPage("edit-blog");
      setBlogId(path.replace("/edit-blog/", ""));
      return;
    }

    if (path.startsWith("/blog/")) {
      setCurrentPage("blog-view");
      setBlogId(path.replace("/blog/", ""));
      return;
    }

    // Handle other pages
    if (path === "/login") {
      setCurrentPage("login");
    } else if (path === "/banned") {
      setCurrentPage("banned");
    } else if (path === "/create-blog") {
      setCurrentPage("create-blog");
    } else if (path === "/friends") {
      setCurrentPage("friends");
    } else if (path === "/saved") {
      setCurrentPage("saved");
    } else if (path === "/profile") {
      setCurrentPage("profile");
    } else if (path === "/personal-page") {
      setCurrentPage("personal-page");
    } else {
      setCurrentPage("home");
    }

    // Handle query parameters
    const params = new URLSearchParams(window.location.search);
    const mode = params.get("mode");
    if (mode === "register" || mode === "signin") {
      setLoginMode(mode);
    }
  };

  useEffect(() => {
    // Update page on initial load
    updatePageFromPath();

    // Listen for back/forward navigation
    const handlePopState = () => {
      updatePageFromPath();
    };

    window.addEventListener('popstate', handlePopState);
    
    return () => {
      window.removeEventListener('popstate', handlePopState);
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
    } else if (page === "banned") {
      url = "/banned";
    }

    window.history.pushState({}, "", url);
  };

  return (
    <RouterContext.Provider value={{ currentPage, loginMode, publicSlug, blogId, navigate }}>
      {children}
    </RouterContext.Provider>
  );
};

export default RouterProvider;
