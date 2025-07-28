import React from "react";
import HomePage from "../pages/HomePage/HomePage";
import LoginPage from "../pages/LoginPage/LoginPage";
import AuthCallback from "../pages/AuthCallback/AuthCallback";
import ProfilePage from "../pages/ProfilePage/ProfilePage";
import PublicPersonalPage from "../pages/PersonalPage/PublicPersonalPage";
import { useRouter } from "./RouterProvider";
import { PersonalPageRefactored } from "@/pages/PersonalPage";

const PageRenderer: React.FC = () => {
  const { currentPage, loginMode, publicSlug } = useRouter();

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
      return <PersonalPageRefactored />;
    case "public":
      if (!publicSlug) {
        console.error('PageRenderer: No publicSlug provided for public page');
        return <HomePage />;
      }
      return <PublicPersonalPage pageSlug={publicSlug} />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default PageRenderer;
