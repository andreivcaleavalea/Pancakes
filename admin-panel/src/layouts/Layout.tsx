import React, { useState, useEffect } from "react";

import { Layout as AntLayout } from "antd";
import { Outlet } from "react-router-dom";
import { Header } from "./Header";
import { Sidebar } from "./Sidebar";
import { Footer } from "./Footer";
import "../styles/layouts/Layout.scss";

const { Content } = AntLayout;

export const Layout: React.FC = () => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth <= 768);
      if (window.innerWidth > 768) {
        setMobileMenuOpen(false);
      }
    };

    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen);
  };

  const closeMobileMenu = () => {
    setMobileMenuOpen(false);
  };

  return (
    <AntLayout className="main-layout">
      {/* Mobile Overlay */}
      {isMobile && (
        <div 
          className={`sidebar-overlay ${mobileMenuOpen ? 'active' : ''}`}
          onClick={closeMobileMenu}
        />
      )}
      
      <Sidebar 
        isMobile={isMobile}
        mobileMenuOpen={mobileMenuOpen}
        onMenuClick={closeMobileMenu}
      />
      <AntLayout>
        <Header 
          isMobile={isMobile}
          onMenuToggle={toggleMobileMenu}
        />
        <Content className="main-layout__content">
          <Outlet />
        </Content>
        <Footer />
      </AntLayout>
    </AntLayout>
  );
};
