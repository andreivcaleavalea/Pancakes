import React from "react";
import HomePage from "../pages/HomePage/HomePage";
import LoginPage from "../pages/LoginPage/LoginPage";
import AuthCallback from "../pages/AuthCallback/AuthCallback";
import CreateBlogPage from "../pages/CreateBlogPage/CreateBlogPage";
import EditBlogPage from "../pages/EditBlogPage/EditBlogPage";
import BlogViewPage from "../pages/BlogViewPage/BlogViewPage";
import FriendsPage from "../pages/FriendsPage/FriendsPage";
import SavedBlogsPage from "../pages/SavedBlogsPage/SavedBlogsPage";
import ProfilePage from "../pages/ProfilePage/ProfilePage";
import PublicPersonalPage from "../pages/PersonalPage/PublicPersonalPage";
import { useRouter } from "./RouterProvider";
import { PersonalPage } from "@/pages/PersonalPage";

const PageRenderer: React.FC = () => {
  const { currentPage, loginMode } = useRouter();

  // Handle auth callback
  if (window.location.pathname === "/auth/callback") {
    return <AuthCallback />;
  }

  switch (currentPage) {
    case "login":
      return <LoginPage initialMode={loginMode} />;
    case "profile":
      return <ProfilePage />;
    case "personal-page":
      return <PersonalPage />;
    case "public":
      if (!publicSlug) {
        console.error('PageRenderer: No publicSlug provided for public page');
        return <HomePage />;
      }
      return <PublicPersonalPage pageSlug={publicSlug} />;
    case "create-blog":
      return <CreateBlogPage />;
    case "friends":
      return <FriendsPage />;
    case "edit-blog":
      return <EditBlogPage />;
    case "blog-view":
      return <BlogViewPage />;
    case "saved":
      return <SavedBlogsPage />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default PageRenderer;
