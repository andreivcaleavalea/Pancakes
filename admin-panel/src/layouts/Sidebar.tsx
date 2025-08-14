import React from "react";
import { Layout, Menu, Typography } from "antd";
import { useNavigate, useLocation } from "react-router-dom";
import {
  DashboardOutlined,
  UserOutlined,
  FileTextOutlined,
  BarChartOutlined,
  SettingOutlined,
  FlagOutlined,
} from "@ant-design/icons";
import "../styles/layouts/Sidebar.scss";

const { Sider } = Layout;
const { Title } = Typography;

interface SidebarProps {
  isMobile?: boolean;
  mobileMenuOpen?: boolean;
  onMenuClick?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  isMobile = false,
  mobileMenuOpen = false,
  onMenuClick,
}) => {
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems = [
    {
      key: "dashboard",
      icon: <DashboardOutlined />,
      label: "Dashboard",
      onClick: () => {
        navigate("/dashboard");
        onMenuClick?.();
      },
    },
    {
      key: "users",
      icon: <UserOutlined />,
      label: "Users",
      onClick: () => {
        navigate("/users");
        onMenuClick?.();
      },
    },
    {
      key: "content",
      icon: <FileTextOutlined />,
      label: "Content",
      onClick: () => {
        navigate("/content");
        onMenuClick?.();
      },
    },
    {
      key: "analytics",
      icon: <BarChartOutlined />,
      label: "Analytics",
      onClick: () => {
        navigate("/analytics");
        onMenuClick?.();
      },
    },
    {
      key: "reports",
      icon: <FlagOutlined />,
      label: "Reports",
      onClick: () => {
        navigate("/reports");
        onMenuClick?.();
      },
    },
    {
      key: "settings",
      icon: <SettingOutlined />,
      label: "Settings",
      onClick: () => {
        navigate("/settings");
        onMenuClick?.();
      },
    },
  ];

  const currentPath = location.pathname.split("/")[1] || "dashboard";

  return (
    <Sider
      className={`sidebar ${isMobile && mobileMenuOpen ? "mobile-open" : ""}`}
      theme="dark"
    >
      <div className="sidebar__brand">
        <Title level={3} className="sidebar__title">
          {isMobile ? "Admin" : "Admin Panel"}
        </Title>
      </div>
      <Menu
        theme="dark"
        mode="inline"
        selectedKeys={[currentPath]}
        items={menuItems}
        className="sidebar__menu"
      />
    </Sider>
  );
};
