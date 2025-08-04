import React from "react";
import { Layout, Button, Typography, Space } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import "../styles/layouts/Header.scss";

const { Header: AntHeader } = Layout;
const { Title } = Typography;

export const Header: React.FC = () => {
  const navigate = useNavigate();
  const { logout, user } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <AntHeader className="header">
      <Title level={4} className="header__title">
        Pancakes Admin
      </Title>
      <Space className="header__user-section">
        <span className="user-welcome">Welcome, {user?.name || "Admin"}</span>
        <Button
          type="text"
          icon={<LogoutOutlined />}
          onClick={handleLogout}
          className="header__logout-btn"
        >
          Logout
        </Button>
      </Space>
    </AntHeader>
  );
};
