import React, { useState, useEffect } from "react";
import { Button, Tooltip } from "antd";
import { FlagOutlined } from "@ant-design/icons";
import { useAuth } from "@/contexts/AuthContext";
import { ReportModal } from "../ReportModal/ReportModal";
import { ReportContentType } from "@/types/report";
import "./ReportButton.scss";

interface ReportButtonProps {
  contentType: ReportContentType;
  contentId: string;
  contentTitle?: string;
  authorId?: string;
  size?: "small" | "middle" | "large";
  type?: "text" | "link" | "primary" | "default";
  className?: string;
}

export const ReportButton: React.FC<ReportButtonProps> = ({
  contentType,
  contentId,
  contentTitle,
  authorId,
  size = "small",
  type = "text",
  className = "",
}) => {
  const { user, isAuthenticated } = useAuth();
  const [modalVisible, setModalVisible] = useState(false);
  const [canReport, setCanReport] = useState(false);

  useEffect(() => {
    // Simple logic: user can report if authenticated and it's not their own content
    if (!isAuthenticated || !user) {
      setCanReport(false);
      return;
    }

    // Don't allow reporting own content
    if (authorId && authorId === user.id) {
      setCanReport(false);
      return;
    }

    // User can report
    setCanReport(true);
  }, [isAuthenticated, user, authorId]);

  const handleClick = () => {
    if (canReport) {
      setModalVisible(true);
    }
  };

  const handleModalClose = () => {
    setModalVisible(false);
    // No need to refresh status - backend validation handles duplicate reports
  };

  // Don't render the button if user is not authenticated or cannot report
  if (!isAuthenticated || !canReport) {
    return null;
  }

  const contentTypeLabel =
    contentType === ReportContentType.BlogPost ? "blog post" : "comment";

  return (
    <>
      <Tooltip title={`Report this ${contentTypeLabel}`}>
        <Button
          type={type}
          size={size}
          icon={<FlagOutlined />}
          onClick={handleClick}
          className={`report-button ${className}`}
          aria-label={`Report ${contentTypeLabel}`}
        >
          Report
        </Button>
      </Tooltip>

      <ReportModal
        visible={modalVisible}
        onClose={handleModalClose}
        contentType={contentType}
        contentId={contentId}
        contentTitle={contentTitle}
      />
    </>
  );
};
