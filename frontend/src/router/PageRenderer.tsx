import React from "react";
import HomePage from "../pages/HomePage/HomePage";
import LoginPage from "../pages/LoginPage/LoginPage";
import AuthCallback from "../pages/AuthCallback/AuthCallback";
import CreateBlogPage from "../pages/CreateBlogPage/CreateBlogPage";
import EditBlogPage from "../pages/EditBlogPage/EditBlogPage";
import BlogViewPage from "../pages/BlogViewPage/BlogViewPage";
import { useRouter } from "./RouterProvider";

const PageRenderer: React.FC = () => {
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
    case "edit-blog":
      return <EditBlogPage />;
    case "blog-view":
      return <BlogViewPage />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default PageRenderer;
