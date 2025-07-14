import React, { createContext, useContext, useState, ReactNode } from "react";

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

  const navigate = (page: PageType, mode?: LoginMode) => {
    setCurrentPage(page);
    if (mode) {
      setLoginMode(mode);
    }
  };

  return (
    <RouterContext.Provider value={{ currentPage, loginMode, navigate }}>
      {children}
    </RouterContext.Provider>
  );
};
