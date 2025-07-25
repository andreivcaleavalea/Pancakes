import React from "react";
import { useRouter } from "./RouterProvider";
import HomePage from "@pages/HomePage/HomePage";
import LoginPage from "@pages/LoginPage/LoginPage";
import AuthCallback from "@pages/AuthCallback/AuthCallback";
import CreateBlogPage from "@pages/CreateBlogPage/CreateBlogPage";
import FriendsPage from "@pages/FriendsPage/FriendsPage";
import SavedBlogsPage from "@pages/SavedBlogsPage/SavedBlogsPage";

const AppRouter: React.FC = () => {
  const { currentPage, loginMode } = useRouter();

  // Handle auth callback
  if (window.location.pathname === "/auth/callback") {
    return <AuthCallback />;
  }

  switch (currentPage) {
    case "login":
      return <LoginPage initialMode={loginMode} />;
    case "create-blog":
      return <CreateBlogPage />;
    case "friends":
      return <FriendsPage />;
    case "saved":
      return <SavedBlogsPage />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default AppRouter;
