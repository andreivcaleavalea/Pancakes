import React, { useEffect, useState } from "react";
import { Typography, Button, Card, Alert, Space, Divider } from "antd";
import { ExclamationCircleOutlined, HomeOutlined, MailOutlined } from "@ant-design/icons";
import { useRouter } from "../../router/RouterProvider";
import "./BannedPage.scss";

const { Title, Text, Paragraph } = Typography;

interface BanInfo {
  reason?: string;
  expirationDate?: string;
  isPermanent: boolean;
  message: string;
}

const BannedPage: React.FC = () => {
  const [banInfo, setBanInfo] = useState<BanInfo | null>(null);
  const { navigate } = useRouter();

  useEffect(() => {
    const banInfoStr = sessionStorage.getItem("ban-info");
    if (banInfoStr) {
      try {
        const data = JSON.parse(banInfoStr);
        const parsedBanInfo = parseBanMessage(data.message);
        setBanInfo(parsedBanInfo);
      } catch (error) {
        console.error("Failed to parse ban info:", error);
        // If we can't parse the ban info, redirect to login
        navigate("login");
      }
    } else {
      // No ban info available, redirect to login
      navigate("login");
    }
  }, [navigate]);

  const parseBanMessage = (message: string): BanInfo => {
    // Parse the ban message to extract details
    // Expected format: "Your account has been banned: [reason]. Ban expires on [date]" or "...This is a permanent ban"
    
    let reason = "";
    let expirationDate = "";
    let isPermanent = false;

    // Extract reason (everything after "banned:" and before ". Ban expires" or ". This is")
    const reasonMatch = message.match(/banned:\s*([^.]+)/i);
    if (reasonMatch) {
      reason = reasonMatch[1].trim();
    }

    // Check if it's permanent
    if (message.toLowerCase().includes("permanent ban")) {
      isPermanent = true;
    } else {
      // Extract expiration date
      const dateMatch = message.match(/ban expires on\s*([^.]+)/i);
      if (dateMatch) {
        expirationDate = dateMatch[1].trim();
      }
    }

    return {
      reason: reason || "Violation of community guidelines",
      expirationDate,
      isPermanent,
      message
    };
  };

  const formatExpirationDate = (dateStr: string): string => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateStr;
    }
  };

  const handleContactSupport = () => {
    // You can implement this to open email client or support form
    window.location.href = "mailto:support@pancakes.com?subject=Account Ban Appeal";
  };

  const handleReturnToLogin = () => {
    // Clean up ban info and return to login
    sessionStorage.removeItem("ban-info");
    navigate("login");
  };

  if (!banInfo) {
    return null; // Will redirect to login
  }

  return (
    <div className="banned-page">
      <div className="banned-page__container">
        <Card className="banned-page__card">
          <div className="banned-page__header">
            <ExclamationCircleOutlined className="banned-page__icon" />
            <Title level={2} className="banned-page__title">
              Account Access Restricted
            </Title>
          </div>

          <Alert
            message="Your account has been temporarily suspended"
            type="error"
            showIcon
            className="banned-page__alert"
          />

          <div className="banned-page__content">
            <Space direction="vertical" size="large" style={{ width: "100%" }}>
              <div>
                <Title level={4}>Reason for suspension:</Title>
                <Paragraph className="banned-page__reason">
                  {banInfo.reason}
                </Paragraph>
              </div>

              <Divider />

              <div>
                <Title level={4}>Duration:</Title>
                {banInfo.isPermanent ? (
                  <Alert
                    message="This is a permanent suspension"
                    type="warning"
                    showIcon
                  />
                ) : banInfo.expirationDate ? (
                  <div>
                    <Text strong>Your account will be restored on:</Text>
                    <br />
                    <Text className="banned-page__expiration">
                      {formatExpirationDate(banInfo.expirationDate)}
                    </Text>
                  </div>
                ) : (
                  <Text>Please contact support for more information.</Text>
                )}
              </div>

              <Divider />

              <div>
                <Title level={4}>What can you do?</Title>
                <ul className="banned-page__actions-list">
                  <li>Review our community guidelines to understand our policies</li>
                  <li>If you believe this is an error, contact our support team</li>
                  {!banInfo.isPermanent && banInfo.expirationDate && (
                    <li>Wait for the suspension period to end and try logging in again</li>
                  )}
                </ul>
              </div>
            </Space>
          </div>

          <div className="banned-page__footer">
            <Space size="middle">
              <Button
                type="primary"
                icon={<MailOutlined />}
                onClick={handleContactSupport}
              >
                Contact Support
              </Button>
              <Button
                icon={<HomeOutlined />}
                onClick={handleReturnToLogin}
              >
                Return to Login
              </Button>
            </Space>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default BannedPage;