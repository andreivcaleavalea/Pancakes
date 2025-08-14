import React from "react";
import { Badge } from "antd";
import "./NotificationBadge.scss";

interface NotificationBadgeProps {
  count: number;
  size?: "small" | "default";
  showZero?: boolean;
}

export const NotificationBadge: React.FC<NotificationBadgeProps> = ({
  count,
  size = "default",
  showZero = false,
}) => {
  return (
    <Badge
      count={count}
      size={size}
      showZero={showZero}
      color="#ff4d4f"
      className="notification-badge"
      style={{
        position: "relative",
        top: "-2px",
        marginLeft: "4px",
      }}
    />
  );
};
