import React from "react";
import { Button } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import "./FloatingActionButton.scss";

interface FloatingActionButtonProps {
  onClick: () => void;
  icon?: React.ReactNode;
  className?: string;
  disabled?: boolean;
}

const FloatingActionButton: React.FC<FloatingActionButtonProps> = ({
  onClick,
  icon = <PlusOutlined />,
  className = "",
  disabled = false,
}) => {
  return (
    <Button
      type="primary"
      shape="circle"
      icon={icon}
      size="large"
      onClick={onClick}
      disabled={disabled}
      className={`floating-action-button ${className}`}
      style={{
        width: "56px",
        height: "56px",
        borderRadius: "50%",
        padding: "0",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    />
  );
};

export default FloatingActionButton;
