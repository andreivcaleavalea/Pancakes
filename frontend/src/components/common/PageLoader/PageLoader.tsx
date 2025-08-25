import React from "react";
import { Spin, Space } from "antd";
import { LoadingOutlined } from "@ant-design/icons";

interface PageLoaderProps {
  message?: string;
  size?: "small" | "default" | "large";
  fullScreen?: boolean;
}

const PageLoader: React.FC<PageLoaderProps> = ({
  message = "Loading...",
  size = "large",
  fullScreen = false,
}) => {
  const antIcon = (
    <LoadingOutlined style={{ fontSize: size === "large" ? 48 : 24 }} spin />
  );

  const content = (
    <Space direction="vertical" align="center" size="large">
      <Spin indicator={antIcon} />
      <span
        style={{
          fontSize: size === "large" ? 16 : 14,
          color: "#666",
        }}
      >
        {message}
      </span>
    </Space>
  );

  if (fullScreen) {
    return (
      <div
        style={{
          position: "fixed",
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          backgroundColor: "rgba(255, 255, 255, 0.8)",
          zIndex: 1000,
        }}
      >
        {content}
      </div>
    );
  }

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "200px",
        padding: "40px",
      }}
    >
      {content}
    </div>
  );
};

export default PageLoader;
