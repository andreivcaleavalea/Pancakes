import React from "react";
import { Layout, Button, Typography, Space } from "antd";
import { LogoutOutlined, MenuOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import "../styles/layouts/Header.scss";

const { Header: AntHeader } = Layout;
const { Title } = Typography;

interface HeaderProps {
  isMobile?: boolean;
  onMenuToggle?: () => void;
}

export const Header: React.FC<HeaderProps> = ({ isMobile = false, onMenuToggle }) => {
  const navigate = useNavigate();
  const { logout, user } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <AntHeader className="header">
      <div className="header__mobile-menu">
        {isMobile && (
          <Button
            type="text"
            icon={<MenuOutlined />}
            onClick={onMenuToggle}
            className="header__menu-toggle"
          />
        )}
        <Title level={4} className="header__title">
          {isMobile ? "Pancakes" : "Pancakes Admin"}
        </Title>
      </div>
      
      <Space className="header__user-section">
        {!isMobile && (
          <span className="user-welcome">Welcome, {user?.name || "Admin"}</span>
        )}
        <Button
          type="text"
          icon={<LogoutOutlined />}
          onClick={handleLogout}
          className="header__logout-btn"
        >
          {isMobile ? "" : "Logout"}
        </Button>
      </Space>
    </AntHeader>
  );
};
