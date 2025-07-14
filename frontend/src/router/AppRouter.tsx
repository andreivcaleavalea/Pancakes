import React, { useState } from "react";
import HomePage from "@pages/HomePage/HomePage";
import LoginPage from "@pages/LoginPage/LoginPage";

export type PageType = "home" | "login";
export type LoginMode = "signin" | "register";

interface AppRouterProps {
  onNavigate: (page: PageType, mode?: LoginMode) => void;
}

const AppRouter: React.FC<AppRouterProps> = ({ onNavigate }) => {
  const [currentPage, setCurrentPage] = useState<PageType>("home");
  const [loginMode, setLoginMode] = useState<LoginMode>("signin");

  const handleNavigate = (page: PageType, mode?: LoginMode) => {
    setCurrentPage(page);
    if (mode) {
      setLoginMode(mode);
    }
    onNavigate(page, mode);
  };

  const renderPage = () => {
    switch (currentPage) {
      case "login":
        return (
          <LoginPage initialMode={loginMode} onNavigate={handleNavigate} />
        );
      case "home":
      default:
        return <HomePage onNavigate={handleNavigate} />;
    }
  };

  return { page: renderPage(), navigate: handleNavigate };
};

export default AppRouter;
