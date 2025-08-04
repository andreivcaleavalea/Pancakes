import React from "react";
import { Layout as AntLayout } from "antd";
import { Outlet } from "react-router-dom";
import { Header } from "./Header";
import { Sidebar } from "./Sidebar";
import { Footer } from "./Footer";
import "../styles/layouts/Layout.scss";

const { Content } = AntLayout;

export const Layout: React.FC = () => {
  return (
    <AntLayout className="main-layout">
      <Sidebar />
      <AntLayout>
        <Header />
        <Content className="main-layout__content">
          <Outlet />
        </Content>
        <Footer />
      </AntLayout>
    </AntLayout>
  );
};
