import React from "react";
import { Layout, Menu, Typography } from "antd";
import { useNavigate, useLocation } from "react-router-dom";
import {
  DashboardOutlined,
  UserOutlined,
  FileTextOutlined,
  BarChartOutlined,
  SettingOutlined,
} from "@ant-design/icons";
import "../styles/layouts/Sidebar.scss";

const { Sider } = Layout;
const { Title } = Typography;

export const Sidebar: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems = [
    {
      key: "dashboard",
      icon: <DashboardOutlined />,
      label: "Dashboard",
      onClick: () => navigate("/dashboard"),
    },
    {
      key: "users",
      icon: <UserOutlined />,
      label: "Users",
      onClick: () => navigate("/users"),
    },
    {
      key: "content",
      icon: <FileTextOutlined />,
      label: "Content",
      onClick: () => navigate("/content"),
    },
    {
      key: "analytics",
      icon: <BarChartOutlined />,
      label: "Analytics",
      onClick: () => navigate("/analytics"),
    },
    {
      key: "settings",
      icon: <SettingOutlined />,
      label: "Settings",
      onClick: () => navigate("/settings"),
    },
  ];

  const currentPath = location.pathname.split("/")[1] || "dashboard";

  return (
    <Sider className="sidebar" theme="dark">
      <div className="sidebar__brand">
        <Title level={3} className="sidebar__title">
          Admin Panel
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
