import React, { useState, useEffect } from "react";
import { Layout } from "antd";
import Header from "../Header/Header";
import Footer from "../Footer/Footer";
import "./MainLayout.scss";

const { Content } = Layout;

interface MainLayoutProps {
  children: React.ReactNode;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    window.addEventListener("resize", handleResize);
    handleResize();

    return () => window.removeEventListener("resize", handleResize);
  }, []);

  return (
    <Layout className="main-layout">
      <Header />
      <Content className="main-layout__content">
        <div
          className={`main-layout__container ${
            isMobile ? "main-layout__container--mobile" : ""
          }`}
        >
          {children}
        </div>
      </Content>
      <Footer />
    </Layout>
  );
};

export default MainLayout;
