import React from "react";
import { useRouter } from "./RouterProvider";
import HomePage from "@pages/HomePage/HomePage";
import LoginPage from "@pages/LoginPage/LoginPage";
import AuthCallback from "@pages/AuthCallback/AuthCallback";

const AppRouter: React.FC = () => {
  const { currentPage, loginMode } = useRouter();

  // Handle auth callback
  if (window.location.pathname === "/auth/callback") {
    return <AuthCallback />;
  }

  switch (currentPage) {
    case "login":
      return <LoginPage initialMode={loginMode} />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default AppRouter;
