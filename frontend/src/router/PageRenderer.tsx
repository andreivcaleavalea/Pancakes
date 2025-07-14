import React from "react";
import HomePage from "../pages/HomePage/HomePage";
import LoginPage from "../pages/LoginPage/LoginPage";
import { useRouter } from "./RouterProvider";

const PageRenderer: React.FC = () => {
  const { currentPage, loginMode } = useRouter();

  switch (currentPage) {
    case "login":
      return <LoginPage initialMode={loginMode} />;
    case "home":
    default:
      return <HomePage />;
  }
};

export default PageRenderer;
