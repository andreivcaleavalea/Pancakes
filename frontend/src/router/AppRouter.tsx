import React, { useState, useEffect } from "react";
import { useRouter } from "./RouterProvider";
import HomePage from "../pages/HomePage/HomePage";
import LoginPage from "../pages/LoginPage/LoginPage";
import AuthCallback from "../pages/AuthCallback/AuthCallback";
import ProfilePage from "../pages/ProfilePage/ProfilePage";
import PersonalPage from "../pages/PersonalPage/PersonalPage";

const AppRouter: React.FC = () => {
  const { loginMode } = useRouter();
  const [currentPath, setCurrentPath] = useState(window.location.pathname);

  // Listen for URL changes
  useEffect(() => {
    const handleLocationChange = () => {
      console.log('🔄 URL changed to:', window.location.pathname);
      setCurrentPath(window.location.pathname);
    };

    // Listen for pushState/replaceState changes
    const originalPushState = window.history.pushState;
    const originalReplaceState = window.history.replaceState;

    window.history.pushState = function(...args) {
      originalPushState.apply(window.history, args);
      handleLocationChange();
    };

    window.history.replaceState = function(...args) {
      originalReplaceState.apply(window.history, args);
      handleLocationChange();
    };

    // Listen for back/forward button
    window.addEventListener('popstate', handleLocationChange);

    return () => {
      window.history.pushState = originalPushState;
      window.history.replaceState = originalReplaceState;
      window.removeEventListener('popstate', handleLocationChange);
    };
  }, []);

  console.log('🔍 AppRouter - currentPath:', currentPath);

  // Handle auth callback
  if (currentPath === "/auth/callback") {
    console.log('📞 Rendering AuthCallback');
    return <AuthCallback />;
  }

  // Route based on current path
  switch (currentPath) {
    case "/login":
      console.log('🔑 Rendering LoginPage');
      return <LoginPage initialMode={loginMode} />;
    case "/profile":
      console.log('👤 Rendering ProfilePage');
      return <ProfilePage />;
    case "/personal-page":
      console.log('🎨 Rendering PersonalPage');
      return (
        <div style={{ padding: '50px', textAlign: 'center', fontSize: '24px', color: 'green' }}>
          ✅ PERSONAL PAGE IS WORKING! 
          <br />
          Path: {currentPath}
          <br />
          🎉 SUCCESS!
        </div>
      );
    case "/":
    default:
      console.log('🏠 Rendering HomePage');
      return <HomePage />;
  }
};

export default AppRouter;
