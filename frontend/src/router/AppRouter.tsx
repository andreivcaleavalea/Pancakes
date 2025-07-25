import React from "react";
import { useRouter } from "./RouterProvider";
import HomePage from "@pages/HomePage/HomePage";
import LoginPage from "@pages/LoginPage/LoginPage";
import AuthCallback from "@pages/AuthCallback/AuthCallback";
import ProfilePage from "@pages/ProfilePage/ProfilePage";
import PublicPersonalPage from "@pages/PersonalPage/PublicPersonalPage";

const AppRouter: React.FC = () => {
  const { currentPage, loginMode, publicSlug } = useRouter();

  console.log('AppRouter: currentPage:', currentPage, 'publicSlug:', publicSlug); // Debug logging

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
      // Navigate to profile page for personal-page route
      window.history.pushState({}, "", "/profile");
      return <ProfilePage />;
    case "public":
      console.log('AppRouter: Rendering PublicPersonalPage with slug:', publicSlug); // Debug logging
      if (!publicSlug) {
        console.error('AppRouter: No publicSlug provided for public page');
        return <HomePage />;
      }
      return <PublicPersonalPage pageSlug={publicSlug} />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default AppRouter;
